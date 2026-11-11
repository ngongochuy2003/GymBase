using GymBase.Base.Crud;
using GymBase.Entities.Auth;

namespace GymBase.DLAuth.Contract.DLPhienDangNhap;

/// <summary>Truy cập phiên đăng nhập.</summary>
public interface IPhienDangNhapRepository : IBaseRepository<phien_dang_nhap>
{
    /// <summary>Tìm phiên có token hiện tại hoặc token vừa thay thế khớp với hash, kể cả phiên đã thu hồi.</summary>
    Task<phien_dang_nhap?> DLFindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>
    /// Đổi refresh token một cách nguyên tử: chỉ thành công khi phiên chưa thu hồi và token hiện tại vẫn là
    /// <paramref name="expectedHash"/>. Hai request cùng lúc thì chỉ một request thắng.
    /// </summary>
    Task<bool> DLRotateAsync(
        Guid sessionId, string expectedHash, string newHash, string previousHash,
        DateTime rotatedAt, DateTime expiresAt, CancellationToken cancellationToken);

    /// <summary>Thu hồi một phiên; phiên đã thu hồi thì giữ nguyên.</summary>
    Task DLRevokeAsync(Guid sessionId, DateTime revokedAt, CancellationToken cancellationToken);

    /// <summary>Thu hồi mọi phiên của người dùng, trừ <paramref name="exceptSessionId"/>.</summary>
    Task DLRevokeAllOfUserAsync(Guid userId, Guid? exceptSessionId, DateTime revokedAt, CancellationToken cancellationToken);
}
