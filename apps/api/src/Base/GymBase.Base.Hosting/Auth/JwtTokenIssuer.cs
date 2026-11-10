using GymBase.Base.Auth;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GymBase.Base.Hosting.Auth;

/// <summary>Cấp access token JWT ký HS256.</summary>
public sealed class JwtTokenIssuer(JwtSettings settings, TimeProvider timeProvider) : ITokenIssuer
{
    private readonly JsonWebTokenHandler _handler = new() { SetDefaultTimesOnTokenCreation = false };

    private readonly SigningCredentials _credentials = new(new SymmetricSecurityKey(settings.SigningKey), SecurityAlgorithms.HmacSha256);

    public IssuedAccessToken CreateAccessToken(AccessTokenRequest request)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now + settings.AccessTokenLifetime,
            TokenType = JwtSettings.TokenType,
            SigningCredentials = _credentials,
            Claims = new Dictionary<string, object>
            {
                [JwtClaimNames.Subject] = request.UserId.ToString(),
                [JwtClaimNames.Session] = request.SessionId.ToString(),
                [JwtClaimNames.TokenId] = Guid.CreateVersion7().ToString(),
                [JwtClaimNames.UserName] = request.UserName,
                [JwtClaimNames.Role] = request.Role,
            },
        });

        return new IssuedAccessToken(token, (int)settings.AccessTokenLifetime.TotalSeconds);
    }
}
