using Microsoft.Extensions.Caching.Memory;

namespace GymBase.BLAuth.XacThuc;

/// <summary>
/// Khóa ngắn và tăng dần theo tên đăng nhập khi sai nhiều lần, tính cả tên không tồn tại để không lộ tài khoản.
/// Lưu trong bộ nhớ vì V1 chỉ có một host; tắt host thì bộ đếm về 0.
/// </summary>
public sealed class LoginThrottle(TimeProvider timeProvider) : IDisposable
{
    private const int FreeAttempts = 4;
    private const int MaxLockMinutes = 15;

    private static readonly TimeSpan ForgetAfter = TimeSpan.FromHours(1);

    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 100_000 });
    private readonly Lock _gate = new();

    private sealed record Attempts(int Failures, DateTimeOffset? LockedUntil);

    public bool IsLocked(string userName)
    {
        lock (_gate)
        {
            return _cache.TryGetValue(Key(userName), out Attempts? attempts)
                && attempts!.LockedUntil > timeProvider.GetUtcNow();
        }
    }

    /// <summary>Ghi một lần sai. Từ lần thứ 5 khóa 1 phút, mỗi lần sai tiếp theo gấp đôi, tối đa 15 phút.</summary>
    public void RecordFailure(string userName)
    {
        var key = Key(userName);
        lock (_gate)
        {
            var failures = (_cache.TryGetValue(key, out Attempts? attempts) ? attempts!.Failures : 0) + 1;
            DateTimeOffset? lockedUntil = failures > FreeAttempts
                ? timeProvider.GetUtcNow().AddMinutes(LockMinutes(failures))
                : null;

            _cache.Set(key, new Attempts(failures, lockedUntil), new MemoryCacheEntryOptions
            {
                Size = 1,
                SlidingExpiration = ForgetAfter,
            });
        }
    }

    public void Reset(string userName)
    {
        lock (_gate)
        {
            _cache.Remove(Key(userName));
        }
    }

    public void Dispose() => _cache.Dispose();

    private static int LockMinutes(int failures)
    {
        var doublings = Math.Min(failures - FreeAttempts - 1, 4); // 2^4 = 16 đã vượt mức tối đa
        return Math.Min(1 << doublings, MaxLockMinutes);
    }

    private static string Key(string userName) => userName.Trim().ToLowerInvariant();
}
