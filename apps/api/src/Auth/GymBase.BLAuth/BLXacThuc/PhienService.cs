using FluentValidation;
using GymBase.Base.Auth;
using GymBase.Base.Exceptions;
using GymBase.Base.Validation;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.DLAuth.Contract.DLTaiKhoan;

namespace GymBase.BLAuth.BLXacThuc;

/// <summary>Vòng đời phiên đăng nhập: xoay vòng refresh token và đăng xuất.</summary>
public sealed class PhienService(
    ITaiKhoanRepository users,
    IPhienDangNhapRepository sessions,
    PhienHieuLucCache sessionCache,
    PhienIssuer sessionIssuer,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    IValidator<LamMoiTokenDto> refreshValidator)
{
    /// <summary>Trong khoảng này, token vừa thay thế gửi lại được coi là do mất mạng, không phải bị đánh cắp.</summary>
    private static readonly TimeSpan RotationGrace = TimeSpan.FromSeconds(30);

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    /// <summary>Đổi refresh token lấy cặp token mới. Token cũ bị dùng lại sau khoảng ân hạn thì thu hồi cả phiên.</summary>
    /// <exception cref="UnauthorizedException">Token không hợp lệ, hết hạn, hoặc tài khoản không còn hoạt động.</exception>
    public async Task<KetQuaDangNhapDto> BLLamMoiTokenAsync(LamMoiTokenDto dto, CancellationToken cancellationToken)
    {
        await refreshValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var presentedHash = PhienIssuer.Hash(dto.refresh_token);
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
            sessionCache.Invalidate(session.Id);
            throw new UnauthorizedException();
        }

        var user = await users.DLGetByIdAsync(session.tai_khoan_id, cancellationToken);
        if (user is not { dang_hoat_dong: true })
        {
            await sessions.DLRevokeAsync(session.Id, now, cancellationToken);
            sessionCache.Invalidate(session.Id);
            throw new UnauthorizedException();
        }

        var refreshToken = PhienIssuer.NewRefreshToken();

        // Khi gửi lại trong khoảng ân hạn, giữ nguyên mốc xoay vòng để không kéo dài khoảng ân hạn mãi.
        var rotated = await sessions.DLRotateAsync(
            session.Id,
            session.token_hash,
            PhienIssuer.Hash(refreshToken),
            presentedHash,
            isPreviousToken ? session.ngay_xoay_vong!.Value : now,
            Min(now + PhienIssuer.RefreshInactivity, session.ngay_het_han_toi_da),
            cancellationToken);

        if (!rotated)
        {
            throw new UnauthorizedException();
        }

        return sessionIssuer.BuildResult(user, session.Id, refreshToken);
    }

    /// <summary>Thu hồi phiên hiện tại.</summary>
    public async Task BLDangXuatAsync(CancellationToken cancellationToken)
    {
        if (currentUser.SessionId is { } sessionId)
        {
            await sessions.DLRevokeAsync(sessionId, UtcNow, cancellationToken);
            sessionCache.Invalidate(sessionId);
        }
    }

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
}
