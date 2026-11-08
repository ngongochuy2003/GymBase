using GymBase.Base.Auth;
using Microsoft.AspNetCore.Http;

namespace GymBase.Base.Hosting.Auth;

/// <summary>Đọc người dùng hiện tại từ claims của request đã xác thực. Tên claim: docs/api/xac-thuc.md, mục 3.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId => ReadGuid(JwtClaimNames.Subject);

    public string? UserName => ReadClaim(JwtClaimNames.UserName);

    public Guid? SessionId => ReadGuid(JwtClaimNames.Session);

    private Guid? ReadGuid(string claimType) => Guid.TryParse(ReadClaim(claimType), out var value) ? value : null;

    private string? ReadClaim(string claimType)
    {
        var user = httpContextAccessor.HttpContext?.User;
        return user?.Identity?.IsAuthenticated == true ? user.FindFirst(claimType)?.Value : null;
    }
}
