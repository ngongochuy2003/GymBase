using System.Collections.Concurrent;
using GymBase.Base.Auth;

namespace GymBase.TestDoubles;

/// <summary>
/// Bộ xác minh ID token giả: chỉ chấp nhận các token đã <see cref="Accept"/>, mọi token khác coi như không hợp lệ.
/// Dùng ở ranh giới với Google, nơi không thể gọi mạng thật trong test.
/// </summary>
public sealed class FakeGoogleIdTokenVerifier : IGoogleIdTokenVerifier
{
    private readonly ConcurrentDictionary<string, GoogleIdentity> _tokens = new();

    public void Accept(string idToken, GoogleIdentity identity) => _tokens[idToken] = identity;

    public Task<GoogleIdentity?> VerifyAsync(string idToken, CancellationToken cancellationToken) =>
        Task.FromResult(_tokens.GetValueOrDefault(idToken));
}
