using GymBase.Base.Auth;
using GymBase.DLAuth.Contract.DLPhienDangNhap;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using Microsoft.Extensions.Caching.Memory;

namespace GymBase.BLAuth.BLXacThuc;

/// <summary>
/// Nhớ ngắn hạn kết quả "phiên còn hiệu lực" để mỗi request có xác thực không phải truy vấn DB. Chỉ lưu kết quả đúng.
/// Thu hồi phiên trong chính host này gọi <see cref="Invalidate"/> hoặc <see cref="InvalidateUser"/> nên có hiệu lực
/// ngay; việc khác (admin khóa tài khoản) có hiệu lực sau tối đa <see cref="Ttl"/>. Lưu trong bộ nhớ vì V1 chỉ có một
/// host (giống <see cref="LoginThrottle"/>); nhiều host thì mỗi host trễ tối đa <see cref="Ttl"/>.
/// </summary>
public sealed class PhienHieuLucCache : IDisposable
{
    public static readonly TimeSpan Ttl = TimeSpan.FromSeconds(20);

    private readonly MemoryCache _valid = new(new MemoryCacheOptions { SizeLimit = 100_000 });
    private readonly MemoryCache _invalidatedUsers = new(new MemoryCacheOptions { SizeLimit = 100_000 });

    /// <summary>true nếu phiên vừa được xác nhận hợp lệ và người dùng chưa bị thu hồi hàng loạt sau đó.</summary>
    public bool IsKnownValid(Guid sessionId, Guid userId) =>
        _valid.TryGetValue(sessionId, out Guid cachedUser) && cachedUser == userId && !_invalidatedUsers.TryGetValue(userId, out _);

    public void MarkValid(Guid sessionId, Guid userId) =>
        _valid.Set(sessionId, userId, new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = Ttl });

    public void Invalidate(Guid sessionId) => _valid.Remove(sessionId);

    /// <summary>Thu hồi nhiều phiên của một người: bỏ qua cache của người đó trong <see cref="Ttl"/> tới.</summary>
    public void InvalidateUser(Guid userId) =>
        _invalidatedUsers.Set(userId, true, new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = Ttl });

    public void Dispose()
    {
        _valid.Dispose();
        _invalidatedUsers.Dispose();
    }
}

/// <summary>Kiểm tra phiên của access token trong DB, qua <see cref="PhienHieuLucCache"/>.</summary>
public sealed class PhienHieuLucService(
    IPhienDangNhapRepository sessions,
    ITaiKhoanRepository users,
    PhienHieuLucCache cache) : IPhienHieuLucProvider
{
    public async Task<bool> BLKiemTraConHieuLucAsync(Guid sessionId, Guid taiKhoanId, CancellationToken cancellationToken)
    {
        if (cache.IsKnownValid(sessionId, taiKhoanId))
        {
            return true;
        }

        var session = await sessions.DLGetByIdAsync(sessionId, cancellationToken);
        if (session is null || session.tai_khoan_id != taiKhoanId || session.ngay_thu_hoi is not null)
        {
            return false;
        }

        if (await users.DLGetByIdAsync(taiKhoanId, cancellationToken) is not { dang_hoat_dong: true })
        {
            return false;
        }

        cache.MarkValid(sessionId, taiKhoanId);
        return true;
    }
}
