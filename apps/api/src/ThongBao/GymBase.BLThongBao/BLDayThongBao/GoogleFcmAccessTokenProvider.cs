using Google.Apis.Auth.OAuth2;

namespace GymBase.BLThongBao.BLDayThongBao;

/// <summary>Access token từ file service account của Firebase. Thư viện Google tự cache và gia hạn token.</summary>
public sealed class GoogleFcmAccessTokenProvider : IFcmAccessTokenProvider
{
    private const string Scope = "https://www.googleapis.com/auth/firebase.messaging";

    private readonly ITokenAccess _credential;

    /// <exception cref="FileNotFoundException">Không có file service account.</exception>
    public GoogleFcmAccessTokenProvider(string credentialsPath)
    {
        _credential = CredentialFactory.FromFile<ServiceAccountCredential>(credentialsPath)
            .ToGoogleCredential()
            .CreateScoped(Scope);
    }

    public Task<string> GetAccessTokenAsync(CancellationToken ct) =>
        _credential.GetAccessTokenForRequestAsync(cancellationToken: ct);
}
