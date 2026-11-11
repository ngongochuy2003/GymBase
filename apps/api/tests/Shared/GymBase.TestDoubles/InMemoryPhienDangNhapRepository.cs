using GymBase.DLAuth.Contract;
using GymBase.Entities.Auth;

namespace GymBase.TestDoubles;

/// <summary>Repository phiên đăng nhập giả cho test.</summary>
public sealed class InMemoryPhienDangNhapRepository(InMemoryStore<phien_dang_nhap> store)
    : InMemoryBaseRepository<phien_dang_nhap>(store), IPhienDangNhapRepository
{
    public Task<phien_dang_nhap?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        Task.FromResult(Store.All().FirstOrDefault(s => s.token_hash == tokenHash || s.token_hash_truoc == tokenHash));

    public Task<bool> RotateAsync(
        Guid sessionId, string expectedHash, string newHash, string previousHash,
        DateTime rotatedAt, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var session = Store.Find(sessionId);
        if (session is null || session.token_hash != expectedHash || session.ngay_thu_hoi is not null)
        {
            return Task.FromResult(false);
        }

        session.token_hash = newHash;
        session.token_hash_truoc = previousHash;
        session.ngay_xoay_vong = rotatedAt;
        session.ngay_het_han = expiresAt;
        return Task.FromResult(Store.TryReplace(session));
    }

    public Task RevokeAsync(Guid sessionId, DateTime revokedAt, CancellationToken cancellationToken)
    {
        var session = Store.Find(sessionId);
        if (session is { ngay_thu_hoi: null })
        {
            session.ngay_thu_hoi = revokedAt;
            Store.TryReplace(session);
        }

        return Task.CompletedTask;
    }

    public Task RevokeAllOfUserAsync(Guid userId, Guid? exceptSessionId, DateTime revokedAt, CancellationToken cancellationToken)
    {
        foreach (var session in Store.All().Where(s => s.tai_khoan_id == userId && s.ngay_thu_hoi is null && s.Id != exceptSessionId))
        {
            session.ngay_thu_hoi = revokedAt;
            Store.TryReplace(session);
        }

        return Task.CompletedTask;
    }
}
