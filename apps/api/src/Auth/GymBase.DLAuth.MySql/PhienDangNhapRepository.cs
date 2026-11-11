using GymBase.DLAuth.Contract;
using GymBase.Entities.Auth;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLAuth.MySql;

/// <summary>Truy cập bảng <c>phien_dang_nhap</c>.</summary>
public sealed class PhienDangNhapRepository(AppDbContext db) : BaseRepository<phien_dang_nhap>(db), IPhienDangNhapRepository
{
    public Task<phien_dang_nhap?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        Set.FirstOrDefaultAsync(s => s.token_hash == tokenHash || s.token_hash_truoc == tokenHash, cancellationToken);

    public async Task<bool> RotateAsync(
        Guid sessionId, string expectedHash, string newHash, string previousHash,
        DateTime rotatedAt, DateTime expiresAt, CancellationToken cancellationToken) =>
        await Set
            .Where(s => s.Id == sessionId && s.token_hash == expectedHash && s.ngay_thu_hoi == null)
            .ExecuteUpdateAsync(
                update => update
                    .SetProperty(s => s.token_hash, newHash)
                    .SetProperty(s => s.token_hash_truoc, previousHash)
                    .SetProperty(s => s.ngay_xoay_vong, rotatedAt)
                    .SetProperty(s => s.ngay_het_han, expiresAt),
                cancellationToken) == 1;

    public Task RevokeAsync(Guid sessionId, DateTime revokedAt, CancellationToken cancellationToken) =>
        Set.Where(s => s.Id == sessionId && s.ngay_thu_hoi == null)
            .ExecuteUpdateAsync(update => update.SetProperty(s => s.ngay_thu_hoi, revokedAt), cancellationToken);

    public Task RevokeAllOfUserAsync(Guid userId, Guid? exceptSessionId, DateTime revokedAt, CancellationToken cancellationToken) =>
        Set.Where(s => s.tai_khoan_id == userId && s.ngay_thu_hoi == null && s.Id != exceptSessionId)
            .ExecuteUpdateAsync(update => update.SetProperty(s => s.ngay_thu_hoi, revokedAt), cancellationToken);
}
