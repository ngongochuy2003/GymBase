using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using GymBase.Base.Auth;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.Base.Validation;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace GymBase.BLAuth.BLXacThuc;

/// <summary>
/// Đăng nhập, xoay vòng refresh token, đăng xuất, đổi mật khẩu. Thiết kế: docs/api/xac-thuc.md.
/// Mọi trường hợp đăng nhập không thành công đều trả cùng một lỗi để không lộ trạng thái tài khoản.
/// </summary>
public sealed class XacThucService(
    ITaiKhoanRepository users,
    IPhienDangNhapRepository sessions,
    IPasswordHasher<tai_khoan> passwordHasher,
    ITokenIssuer tokenIssuer,
    LoginThrottle throttle,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    IValidator<DangNhapDto> loginValidator,
    IValidator<LamMoiTokenDto> refreshValidator,
    IValidator<DoiMatKhauDto> changePasswordValidator,
    IValidator<DatLaiMatKhauDto> resetPasswordValidator)
{
    private static readonly TimeSpan RefreshInactivity = TimeSpan.FromDays(30);
    private static readonly TimeSpan RefreshMaxLifetime = TimeSpan.FromDays(90);

    /// <summary>Trong khoảng này, token vừa thay thế gửi lại được coi là do mất mạng, không phải bị đánh cắp.</summary>
    private static readonly TimeSpan RotationGrace = TimeSpan.FromSeconds(30);

    private static readonly tai_khoan PlaceholderUser = new();

    private static string? s_dummyHash;

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    /// <exception cref="UnauthorizedException"><see cref="AuthErrorCodes.InvalidCredentials"/>: đăng nhập không thành công.</exception>
    public async Task<KetQuaDangNhapDto> BLLoginAsync(DangNhapDto dto, CancellationToken cancellationToken)
    {
        await loginValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var userName = dto.ten_dang_nhap.Trim();
        var isLocked = throttle.IsLocked(userName);
        var user = await users.DLFindByTenDangNhapAsync(userName, cancellationToken);

        // Luôn băm, kể cả khi không có tài khoản hoặc đang khóa, để thời gian phản hồi tương đương.
        var verification = VerifyPassword(user, dto.mat_khau);

        if (isLocked || user is null || !user.dang_hoat_dong || verification == PasswordVerificationResult.Failed)
        {
            if (!isLocked)
            {
                throttle.RecordFailure(userName);
            }

            throw InvalidCredentials();
        }

        throttle.Reset(userName);

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.mat_khau_hash = passwordHasher.HashPassword(user, dto.mat_khau);
            await users.DLUpdateAsync(user, cancellationToken);
        }

        var now = UtcNow;
        var refreshToken = NewRefreshToken();
        var session = new phien_dang_nhap
        {
            tai_khoan_id = user.Id,
            token_hash = Hash(refreshToken),
            ngay_het_han = now + RefreshInactivity,
            ngay_het_han_toi_da = now + RefreshMaxLifetime,
            ten_thiet_bi = string.IsNullOrWhiteSpace(dto.ten_thiet_bi) ? null : dto.ten_thiet_bi.Trim(),
            ngay_tao = now,
        };
        await sessions.DLInsertAsync(session, cancellationToken);

        return BuildResult(user, session.Id, refreshToken);
    }

    /// <summary>Đổi refresh token lấy cặp token mới. Token cũ bị dùng lại sau khoảng ân hạn thì thu hồi cả phiên.</summary>
    /// <exception cref="UnauthorizedException">Token không hợp lệ, hết hạn, hoặc tài khoản không còn hoạt động.</exception>
    public async Task<KetQuaDangNhapDto> BLRefreshAsync(LamMoiTokenDto dto, CancellationToken cancellationToken)
    {
        await refreshValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var presentedHash = Hash(dto.refresh_token);
        var session = await sessions.DLFindByTokenHashAsync(presentedHash, cancellationToken) ?? throw new UnauthorizedException();
        var now = UtcNow;

        if (session.ngay_thu_hoi is not null || now >= session.ngay_het_han || now >= session.ngay_het_han_toi_da)
        {
            throw new UnauthorizedException();
        }

        var isPreviousToken = session.token_hash != presentedHash;
        if (isPreviousToken && now - session.ngay_xoay_vong > RotationGrace)
        {
            await sessions.DLRevokeAsync(session.Id, now, cancellationToken);
            throw new UnauthorizedException();
        }

        var user = await users.DLGetByIdAsync(session.tai_khoan_id, cancellationToken);
        if (user is not { dang_hoat_dong: true })
        {
            await sessions.DLRevokeAsync(session.Id, now, cancellationToken);
            throw new UnauthorizedException();
        }

        var refreshToken = NewRefreshToken();

        // Khi gửi lại trong khoảng ân hạn, giữ nguyên mốc xoay vòng để không kéo dài khoảng ân hạn mãi.
        var rotated = await sessions.DLRotateAsync(
            session.Id,
            session.token_hash,
            Hash(refreshToken),
            presentedHash,
            isPreviousToken ? session.ngay_xoay_vong!.Value : now,
            Min(now + RefreshInactivity, session.ngay_het_han_toi_da),
            cancellationToken);

        if (!rotated)
        {
            throw new UnauthorizedException();
        }

        return BuildResult(user, session.Id, refreshToken);
    }

    /// <summary>Thu hồi phiên hiện tại.</summary>
    public async Task BLLogoutAsync(CancellationToken cancellationToken)
    {
        if (currentUser.SessionId is { } sessionId)
        {
            await sessions.DLRevokeAsync(sessionId, UtcNow, cancellationToken);
        }
    }

    /// <exception cref="UnauthorizedException">Token không còn gắn với người dùng hợp lệ.</exception>
    public async Task<TaiKhoanDto> BLGetCurrentUserAsync(CancellationToken cancellationToken) =>
        ToDto(await GetCurrentUserEntityAsync(cancellationToken));

    /// <summary>Đổi mật khẩu của chính mình và thu hồi các phiên khác.</summary>
    /// <exception cref="ValidationFailedException">Mật khẩu hiện tại sai hoặc mật khẩu mới không hợp lệ.</exception>
    public async Task BLChangePasswordAsync(DoiMatKhauDto dto, CancellationToken cancellationToken)
    {
        await changePasswordValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var user = await GetCurrentUserEntityAsync(cancellationToken);
        if (VerifyPassword(user, dto.mat_khau_cu) == PasswordVerificationResult.Failed)
        {
            throw new ValidationFailedException(nameof(DoiMatKhauDto.mat_khau_cu), "Mật khẩu hiện tại không đúng.");
        }

        await SetPasswordAsync(user, dto.mat_khau_moi, nameof(DoiMatKhauDto.mat_khau_moi), cancellationToken);
        await sessions.DLRevokeAllOfUserAsync(user.Id, currentUser.SessionId, UtcNow, cancellationToken);
    }

    /// <summary>Admin đặt lại mật khẩu cho người dùng và thu hồi mọi phiên của người đó.</summary>
    /// <exception cref="NotFoundException">Không có người dùng.</exception>
    public async Task BLResetPasswordAsync(Guid userId, DatLaiMatKhauDto dto, CancellationToken cancellationToken)
    {
        await resetPasswordValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var user = await users.DLGetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy người dùng hoặc người dùng đã bị xóa.");

        await SetPasswordAsync(user, dto.mat_khau_moi, nameof(DatLaiMatKhauDto.mat_khau_moi), cancellationToken);
        await sessions.DLRevokeAllOfUserAsync(user.Id, null, UtcNow, cancellationToken);
    }

    /// <summary>Tạo tài khoản Admin đầu tiên, hoặc đặt lại mật khẩu nếu tên đã tồn tại. Chỉ dùng từ dòng lệnh.</summary>
    public async Task BLCreateOrResetAdminAsync(string userName, string password, CancellationToken cancellationToken)
    {
        await resetPasswordValidator.ValidateOrThrowAsync(new DatLaiMatKhauDto { mat_khau_moi = password }, cancellationToken);

        var name = userName.Trim();
        var user = await users.DLFindByTenDangNhapAsync(name, cancellationToken);
        if (user is null)
        {
            user = new tai_khoan { ten_dang_nhap = name, ho_ten = name, vai_tro = AppRoles.Admin };
            EnsureNotUserName(password, name, nameof(DatLaiMatKhauDto.mat_khau_moi));
            user.mat_khau_hash = passwordHasher.HashPassword(user, password);
            user.MarkCreated("create-admin", UtcNow);
            await users.DLInsertAsync(user, cancellationToken);
            return;
        }

        user.vai_tro = AppRoles.Admin;
        await SetPasswordAsync(user, password, nameof(DatLaiMatKhauDto.mat_khau_moi), cancellationToken);
        await sessions.DLRevokeAllOfUserAsync(user.Id, null, UtcNow, cancellationToken);
    }

    private async Task SetPasswordAsync(tai_khoan user, string password, string field, CancellationToken cancellationToken)
    {
        EnsureNotUserName(password, user.ten_dang_nhap ?? string.Empty, field);

        user.mat_khau_hash = passwordHasher.HashPassword(user, password);
        user.MarkUpdated(currentUser.UserName, UtcNow);

        if (!await users.DLUpdateAsync(user, cancellationToken))
        {
            throw new NotFoundException("Không tìm thấy người dùng hoặc người dùng đã bị xóa.");
        }
    }

    private async Task<tai_khoan> GetCurrentUserEntityAsync(CancellationToken cancellationToken)
    {
        var user = currentUser.UserId is { } id ? await users.DLGetByIdAsync(id, cancellationToken) : null;
        return user ?? throw new UnauthorizedException();
    }

    private PasswordVerificationResult VerifyPassword(tai_khoan? user, string password)
    {
        s_dummyHash ??= passwordHasher.HashPassword(PlaceholderUser, Guid.NewGuid().ToString());

        var hasHash = !string.IsNullOrEmpty(user?.mat_khau_hash);

        try
        {
            var result = passwordHasher.VerifyHashedPassword(
                user ?? PlaceholderUser, hasHash ? user!.mat_khau_hash! : s_dummyHash, password);
            return hasHash ? result : PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            // Hash không đúng định dạng, ví dụ dữ liệu mẫu: coi như sai mật khẩu.
            return PasswordVerificationResult.Failed;
        }
    }

    private KetQuaDangNhapDto BuildResult(tai_khoan user, Guid sessionId, string refreshToken)
    {
        var token = tokenIssuer.CreateAccessToken(new AccessTokenRequest(user.Id, user.ten_dang_nhap ?? user.email ?? user.Id.ToString(), user.vai_tro, sessionId));
        return new KetQuaDangNhapDto(token.Token, token.ExpiresInSeconds, refreshToken, ToDto(user));
    }

    private static void EnsureNotUserName(string password, string userName, string field)
    {
        if (PasswordRules.ContainsUserName(password, userName))
        {
            throw new ValidationFailedException(field, "Mật khẩu không được chứa tên đăng nhập.");
        }
    }

    private static UnauthorizedException InvalidCredentials() =>
        new("Tên đăng nhập hoặc mật khẩu không đúng.", AuthErrorCodes.InvalidCredentials);

    private static TaiKhoanDto ToDto(tai_khoan user) => new(
        user.Id, user.ten_dang_nhap, user.ho_ten, user.email, user.so_dien_thoai,
        user.vai_tro, user.dang_hoat_dong, user.ngay_tao, user.ngay_chinh_sua);

    private static string NewRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private static string Hash(string token) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
}
