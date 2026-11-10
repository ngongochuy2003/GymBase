namespace GymBase.Base.Auth;

/// <summary>Thông tin đưa vào access token.</summary>
public sealed record AccessTokenRequest(Guid UserId, string UserName, string Role, Guid SessionId);

/// <summary>Access token vừa cấp và số giây còn hiệu lực.</summary>
public sealed record IssuedAccessToken(string Token, int ExpiresInSeconds);

/// <summary>Cấp access token. Hiện thực nằm ở GymBase.Base.Hosting để tầng nghiệp vụ không phụ thuộc thư viện JWT.</summary>
public interface ITokenIssuer
{
    IssuedAccessToken CreateAccessToken(AccessTokenRequest request);
}
