using FluentValidation;
using GymBase.Base.Auth;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.Base.Validation;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace GymBase.BLAuth.BLXacThuc;

/// <summary>Đổi mật khẩu của chính mình và Admin đặt lại mật khẩu; cả hai đều thu hồi phiên.</summary>
public sealed class MatKhauService(
    ITaiKhoanRepository users,
    IPhienDangNhapRepository sessions,
    PhienHieuLucCache sessionCache,
    IPasswordHasher<tai_khoan> passwordHasher,
    MatKhauVerifier passwordVerifier,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    IValidator<DoiMatKhauDto> changePasswordValidator,
    IValidator<DatLaiMatKhauDto> resetPasswordValidator)
{
    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    /// <summary>Đổi mật khẩu của chính mình và thu hồi các phiên khác.</summary>
    /// <exception cref="ValidationFailedException">Mật khẩu hiện tại sai hoặc mật khẩu mới không hợp lệ.</exception>
    /// <exception cref="UnauthorizedException">Token không còn gắn với người dùng hợp lệ.</exception>
    public async Task BLDoiMatKhauAsync(DoiMatKhauDto dto, CancellationToken cancellationToken)
    {
        await changePasswordValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var user = (currentUser.UserId is { } id ? await users.DLGetByIdAsync(id, cancellationToken) : null)
            ?? throw new UnauthorizedException();

        if (passwordVerifier.Verify(user, dto.mat_khau_cu) == PasswordVerificationResult.Failed)
        {
            throw new ValidationFailedException(nameof(DoiMatKhauDto.mat_khau_cu), "Mật khẩu hiện tại không đúng.");
        }

        await SetPasswordAsync(user, dto.mat_khau_moi, cancellationToken);
        await sessions.DLRevokeAllOfUserAsync(user.Id, currentUser.SessionId, UtcNow, cancellationToken);
        sessionCache.InvalidateUser(user.Id);
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
        sessionCache.InvalidateUser(user.Id);
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
}
