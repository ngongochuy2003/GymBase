using GymBase.Base.Hosting.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace GymBase.IntegrationTests;

/// <summary>
/// Phần của bộ xác minh ID token chạy được mà không gọi mạng: chưa cấu hình client ID, và chuỗi không phải JWT.
/// Chữ ký, aud, iss, hết hạn do thư viện của Google kiểm tra; phần đó chỉ thử được với token thật của Google.
/// </summary>
public class GoogleIdTokenVerifierTests
{
    private static GoogleIdTokenVerifier Verifier(params string[] clientIds)
    {
        var settings = clientIds.Select((id, index) => KeyValuePair.Create<string, string?>($"{GoogleIdTokenVerifier.ClientIdsSection}:{index}", id));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new GoogleIdTokenVerifier(configuration, NullLogger<GoogleIdTokenVerifier>.Instance);
    }

    [Fact]
    public async Task Without_client_ids_every_token_is_rejected()
    {
        Assert.Null(await Verifier().VerifyAsync("a.b.c", CancellationToken.None));
    }

    [Fact]
    public async Task Blank_client_ids_count_as_not_configured()
    {
        Assert.Null(await Verifier("", "  ").VerifyAsync("a.b.c", CancellationToken.None));
    }

    [Theory]
    [InlineData("khong-phai-jwt")]
    [InlineData("chi.hai-phan")]
    [InlineData("a.b.c")]
    public async Task A_string_that_is_not_a_google_signed_jwt_is_rejected(string token)
    {
        Assert.Null(await Verifier("client-id.apps.googleusercontent.com").VerifyAsync(token, CancellationToken.None));
    }
}
