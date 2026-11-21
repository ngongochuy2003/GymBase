using FluentValidation;
using GymBase.Base.Auth;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.Base.Validation;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.DLAuth.Contract.DLDangNhapNgoai;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.Entities.Auth;
using GymBase.Entities.Auth.Enums;
using Microsoft.AspNetCore.Identity;

namespace GymBase.BLAuth.BLXacThuc;

/// <summary>
/// Đăng nhập bằng mật khẩu, đăng nhập Google và tự đăng ký.
/// Mọi trường hợp đăng nhập không thành công đều trả cùng một lỗi để không lộ trạng thái tài khoản.
/// </summary>
public sealed class DangNhapService(
    ITaiKhoanRepository users,
    IPasswordHasher<tai_khoan> passwordHasher,
    MatKhauVerifier passwordVerifier,
    PhienIssuer sessionIssuer,
    LoginThrottle throttle,
    TimeProvider timeProvider,
    IValidator<DangNhapDto> loginValidator,
    IValidator<DangKyDto> registerValidator,
    IValidator<DangNhapGoogleDto> googleValidator,
    IGoogleIdTokenVerifier googleVerifier,
    IDangNhapNgoaiRepository externalLogins)
{
    private const int MaxDisplayNameLength = 100;

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    /// <exception cref="UnauthorizedException"><see cref="AuthErrorCodes.InvalidCredentials"/>: đăng nhập không thành công.</exception>
    public async Task<KetQuaDangNhapDto> BLDangNhapAsync(DangNhapDto dto, CancellationToken cancellationToken)
    {
        await loginValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var email = EmailRules.Normalize(dto.email);
        var isLocked = throttle.IsLocked(email);
        var user = await users.DLFindByEmailAsync(email, cancellationToken);

        // Luôn băm, kể cả khi không có tài khoản hoặc đang khóa, để thời gian phản hồi tương đương.
        var verification = passwordVerifier.Verify(user, dto.mat_khau);

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

        return await sessionIssuer.StartAsync(user, dto.ten_thiet_bi, cancellationToken);
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

        return TaiKhoanMapper.ToDto(user);
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

            return await sessionIssuer.StartAsync(linked, dto.ten_thiet_bi, cancellationToken);
        }

        var created = await CreateGoogleAccountAsync(identity, cancellationToken);
        return await sessionIssuer.StartAsync(created, dto.ten_thiet_bi, cancellationToken);
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

    /// <summary>Họ tên lấy từ Google (cột <c>ho_ten</c> tối đa 100 ký tự); không có thì lấy phần trước <c>@</c> của email.</summary>
    private static string DisplayName(string? googleName, string email)
    {
        var name = string.IsNullOrWhiteSpace(googleName) ? EmailRules.LocalPart(email) : googleName.Trim();
        return name.Length <= MaxDisplayNameLength ? name : name[..MaxDisplayNameLength];
    }

    private static UnauthorizedException InvalidCredentials() =>
        new("Email hoặc mật khẩu không đúng.", AuthErrorCodes.InvalidCredentials);
}
