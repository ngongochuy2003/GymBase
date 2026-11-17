using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using GymBase.Base.Auth;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.Base.Validation;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.DLAuth.Contract.DLDangNhapNgoai;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.Entities.Auth;
using GymBase.Entities.Auth.Enums;
using Microsoft.AspNetCore.Identity;

namespace GymBase.BLAuth.BLXacThuc;

/// <summary>
/// Đăng nhập, xoay vòng refresh token, đăng xuất, đổi mật khẩu.
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
    IValidator<DatLaiMatKhauDto> resetPasswordValidator,
    IValidator<DangKyDto> registerValidator,
    IValidator<DangNhapGoogleDto> googleValidator,
    IGoogleIdTokenVerifier googleVerifier,
    IDangNhapNgoaiRepository externalLogins)
{
    private const int MaxDisplayNameLength = 100;

    private static readonly TimeSpan RefreshInactivity = TimeSpan.FromDays(30);
    private static readonly TimeSpan RefreshMaxLifetime = TimeSpan.FromDays(90);

    /// <summary>Trong khoảng này, token vừa thay thế gửi lại được coi là do mất mạng, không phải bị đánh cắp.</summary>
    private static readonly TimeSpan RotationGrace = TimeSpan.FromSeconds(30);

    private static readonly tai_khoan PlaceholderUser = new();

    private static string? s_dummyHash;

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    /// <exception cref="UnauthorizedException"><see cref="AuthErrorCodes.InvalidCredentials"/>: đăng nhập không thành công.</exception>
    public async Task<KetQuaDangNhapDto> BLDangNhapAsync(DangNhapDto dto, CancellationToken cancellationToken)
    {
        await loginValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var email = EmailRules.Normalize(dto.email);
        var isLocked = throttle.IsLocked(email);
        var user = await users.DLFindByEmailAsync(email, cancellationToken);

        // Luôn băm, kể cả khi không có tài khoản hoặc đang khóa, để thời gian phản hồi tương đương.
        var verification = VerifyPassword(user, dto.mat_khau);

        if (isLocked || user is null || !user.dang_hoat_dong || verification == PasswordVerificationResult.Failed)
        {
            if (!isLocked)
            {
                throttle.RecordFailure(email);
            }

            throw InvalidCredentials();
        }

        throttle.Reset(email);

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.mat_khau_hash = passwordHasher.HashPassword(user, dto.mat_khau);
            await users.DLUpdateAsync(user, cancellationToken);
        }

        return await StartSessionAsync(user, dto.ten_thiet_bi, cancellationToken);
    }

    // TODO(VERIFY): chưa xác minh email và số điện thoại, nên email mới chỉ là định danh đăng nhập chứ chưa chứng minh quyền sở hữu; làm cùng quên mật khẩu qua email.
    /// <summary>
    /// Người mới tự đăng ký bằng mật khẩu. Không tạo phiên: app đưa người dùng về màn đăng nhập. Tài khoản mới có <c>is_dang_nhap_lan_dau = true</c>. Chưa xác minh email và số điện thoại (nếu có).
    /// </summary>
    /// <exception cref="ValidationFailedException">Dữ liệu không hợp lệ hoặc chưa đồng ý điều khoản.</exception>
    /// <exception cref="ConflictException"><see cref="AuthErrorCodes.UserEmailTaken"/>: email đã thuộc tài khoản khác.</exception>
    public async Task<TaiKhoanDto> BLDangKyAsync(DangKyDto dto, CancellationToken cancellationToken)
    {
        await registerValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var email = EmailRules.Normalize(dto.email);
        if (await users.DLExistsByEmailAsync(email, null, cancellationToken))
        {
            throw new ConflictException(nameof(DangKyDto.email), $"Email '{email}' đã được dùng.", AuthErrorCodes.UserEmailTaken);
        }

        var now = UtcNow;
        var user = new tai_khoan
        {
            email = email,
            ho_ten = dto.ho_ten.Trim(),
            so_dien_thoai = PhoneRules.NormalizeMobile(dto.so_dien_thoai),
            vai_tro = AppRoles.User,
        };
        user.mat_khau_hash = passwordHasher.HashPassword(user, dto.mat_khau);
        user.MarkCreated(email, now);
        await users.DLInsertAsync(user, cancellationToken);

        return ToDto(user);
    }

    /// <summary>
    /// Đăng nhập bằng Google; lần đầu thì tạo tài khoản (không mật khẩu, họ tên và email lấy từ Google) rồi liên kết.
    /// Tài khoản được tìm theo <c>sub</c> của Google, không theo email. Email trùng tài khoản đã có thì từ chối, không tự liên kết (ADR-0001, docs/api/xac-thuc.md mục 8).
    /// </summary>
    /// <exception cref="UnauthorizedException">
    /// <see cref="AuthErrorCodes.GoogleTokenInvalid"/>, <see cref="AuthErrorCodes.GoogleEmailUnverified"/>, hoặc tài khoản đã liên kết bị khóa.
    /// </exception>
    /// <exception cref="ConflictException">
    /// <see cref="AuthErrorCodes.EmailUsedPassword"/> hoặc <see cref="AuthErrorCodes.UserEmailTaken"/>: email Google đã thuộc tài khoản khác.
    /// </exception>
    public async Task<KetQuaDangNhapDto> BLDangNhapGoogleAsync(DangNhapGoogleDto dto, CancellationToken cancellationToken)
    {
        await googleValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var identity = await googleVerifier.VerifyAsync(dto.id_token, cancellationToken)
            ?? throw new UnauthorizedException("Không xác minh được tài khoản Google. Vui lòng thử lại.", AuthErrorCodes.GoogleTokenInvalid);

        var link = await externalLogins.DLFindByMaNgoaiAsync(NhaCungCap.google, identity.Subject, cancellationToken);
        if (link is not null)
        {
            var linked = await users.DLGetByIdAsync(link.tai_khoan_id, cancellationToken);
            if (linked is not { dang_hoat_dong: true })
            {
                throw new UnauthorizedException("Không thể đăng nhập bằng Google với tài khoản này.");
            }

            return await StartSessionAsync(linked, dto.ten_thiet_bi, cancellationToken);
        }

        var created = await CreateGoogleAccountAsync(identity, cancellationToken);
        return await StartSessionAsync(created, dto.ten_thiet_bi, cancellationToken);
    }

    private async Task<tai_khoan> CreateGoogleAccountAsync(GoogleIdentity identity, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identity.Email) || !identity.EmailVerified)
        {
            throw new UnauthorizedException("Tài khoản Google chưa có email đã xác minh.", AuthErrorCodes.GoogleEmailUnverified);
        }

        var email = EmailRules.Normalize(identity.Email);
        var sameEmail = await users.DLFindByEmailAsync(email, cancellationToken);
        if (sameEmail is not null)
        {
            throw string.IsNullOrEmpty(sameEmail.mat_khau_hash)
                ? new ConflictException(nameof(DangKyDto.email), $"Email '{email}' đã được dùng.", AuthErrorCodes.UserEmailTaken)
                : new ConflictException(
                    nameof(DangKyDto.email),
                    "Email này đã đăng ký bằng mật khẩu. Hãy đăng nhập bằng mật khẩu.",
                    AuthErrorCodes.EmailUsedPassword);
        }

        var now = UtcNow;
        var user = new tai_khoan
        {
            email = email,
            ho_ten = DisplayName(identity.Name, email),
            vai_tro = AppRoles.User,
        };
        user.MarkCreated(email, now);
        await users.DLInsertAsync(user, cancellationToken);

        try
        {
            await externalLogins.DLInsertAsync(
                new dang_nhap_ngoai { tai_khoan_id = user.Id, nha_cung_cap = NhaCungCap.google, ma_ngoai = identity.Subject, ngay_tao = now },
                cancellationToken);
        }
        catch
        {
            // Không để lại tài khoản mồ côi: lần sau email này sẽ bị báo trùng mà không đăng nhập được.
            await users.DLDeleteAsync(user.Id, CancellationToken.None);
            throw;
        }

        return user;
    }

    private async Task<KetQuaDangNhapDto> StartSessionAsync(tai_khoan user, string? deviceName, CancellationToken cancellationToken)
    {
        var now = UtcNow;
        var refreshToken = NewRefreshToken();
        var session = new phien_dang_nhap
        {
            tai_khoan_id = user.Id,
            token_hash = Hash(refreshToken),
            ngay_het_han = now + RefreshInactivity,
            ngay_het_han_toi_da = now + RefreshMaxLifetime,
            ten_thiet_bi = string.IsNullOrWhiteSpace(deviceName) ? null : deviceName.Trim(),
            ngay_tao = now,
        };
        await sessions.DLInsertAsync(session, cancellationToken);

        return BuildResult(user, session.Id, refreshToken);
    }

    /// <summary>Đổi refresh token lấy cặp token mới. Token cũ bị dùng lại sau khoảng ân hạn thì thu hồi cả phiên.</summary>
    /// <exception cref="UnauthorizedException">Token không hợp lệ, hết hạn, hoặc tài khoản không còn hoạt động.</exception>
    public async Task<KetQuaDangNhapDto> BLLamMoiTokenAsync(LamMoiTokenDto dto, CancellationToken cancellationToken)
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
    public async Task BLDangXuatAsync(CancellationToken cancellationToken)
    {
        if (currentUser.SessionId is { } sessionId)
        {
            await sessions.DLRevokeAsync(sessionId, UtcNow, cancellationToken);
        }
    }

    /// <exception cref="UnauthorizedException">Token không còn gắn với người dùng hợp lệ.</exception>
    public async Task<TaiKhoanDto> BLLayTaiKhoanHienTaiAsync(CancellationToken cancellationToken) =>
        ToDto(await GetCurrentUserEntityAsync(cancellationToken));

    /// <summary>
    /// Người dùng đã bấm một trong hai nút của màn onboarding: đánh dấu <c>is_dang_nhap_lan_dau = false</c>. Gọi lại nhiều
    /// lần vẫn an toàn.
    /// </summary>
    /// <exception cref="UnauthorizedException">Token không còn gắn với người dùng hợp lệ.</exception>
    public async Task<TaiKhoanDto> BLHoanTatDangNhapLanDauAsync(CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserEntityAsync(cancellationToken);
        if (user.is_dang_nhap_lan_dau)
        {
            user.is_dang_nhap_lan_dau = false;
            user.MarkUpdated(currentUser.UserName, UtcNow);
            await users.DLUpdateAsync(user, cancellationToken);
        }

        return ToDto(user);
    }

    /// <summary>Đổi mật khẩu của chính mình và thu hồi các phiên khác.</summary>
    /// <exception cref="ValidationFailedException">Mật khẩu hiện tại sai hoặc mật khẩu mới không hợp lệ.</exception>
    public async Task BLDoiMatKhauAsync(DoiMatKhauDto dto, CancellationToken cancellationToken)
    {
        await changePasswordValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var user = await GetCurrentUserEntityAsync(cancellationToken);
        if (VerifyPassword(user, dto.mat_khau_cu) == PasswordVerificationResult.Failed)
        {
            throw new ValidationFailedException(nameof(DoiMatKhauDto.mat_khau_cu), "Mật khẩu hiện tại không đúng.");
        }

        await SetPasswordAsync(user, dto.mat_khau_moi, cancellationToken);
        await sessions.DLRevokeAllOfUserAsync(user.Id, currentUser.SessionId, UtcNow, cancellationToken);
    }

    /// <summary>Admin đặt lại mật khẩu cho người dùng và thu hồi mọi phiên của người đó.</summary>
    /// <exception cref="NotFoundException">Không có người dùng.</exception>
    public async Task BLDatLaiMatKhauAsync(Guid userId, DatLaiMatKhauDto dto, CancellationToken cancellationToken)
    {
        await resetPasswordValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var user = await users.DLGetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Không tìm thấy người dùng hoặc người dùng đã bị xóa.");

        await SetPasswordAsync(user, dto.mat_khau_moi, cancellationToken);
        await sessions.DLRevokeAllOfUserAsync(user.Id, null, UtcNow, cancellationToken);
    }

    private async Task SetPasswordAsync(tai_khoan user, string password, CancellationToken cancellationToken)
    {
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
            return PasswordVerificationResult.Failed;
        }
    }

    private KetQuaDangNhapDto BuildResult(tai_khoan user, Guid sessionId, string refreshToken)
    {
        var token = tokenIssuer.CreateAccessToken(new AccessTokenRequest(user.Id, user.ten_dang_nhap ?? user.email ?? user.Id.ToString(), user.vai_tro, sessionId));
        return new KetQuaDangNhapDto(token.Token, token.ExpiresInSeconds, refreshToken, ToDto(user));
    }

    /// <summary>Họ tên lấy từ Google (cột <c>ho_ten</c> tối đa 100 ký tự); không có thì lấy phần trước <c>@</c> của email.</summary>
    private static string DisplayName(string? googleName, string email)
    {
        var name = string.IsNullOrWhiteSpace(googleName) ? EmailRules.LocalPart(email) : googleName.Trim();
        return name.Length <= MaxDisplayNameLength ? name : name[..MaxDisplayNameLength];
    }

    private static UnauthorizedException InvalidCredentials() =>
        new("Email hoặc mật khẩu không đúng.", AuthErrorCodes.InvalidCredentials);

    private static TaiKhoanDto ToDto(tai_khoan user) => new(
        user.Id, user.ten_dang_nhap, user.ho_ten, user.email, user.so_dien_thoai,
        user.vai_tro, user.dang_hoat_dong, user.ngay_tao, user.ngay_chinh_sua, user.is_dang_nhap_lan_dau);

    private static string NewRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private static string Hash(string token) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
}
