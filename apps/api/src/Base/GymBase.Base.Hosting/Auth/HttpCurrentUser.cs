using System.Security.Claims;
using GymBase.Base.Auth;
using Microsoft.AspNetCore.Http;

namespace GymBase.Base.Hosting.Auth;

// TODO(AUTH): chốt tên claim khi phát hành JWT; JWT phải có tên đăng nhập và Id dạng Guid. Mục 12.
/// <summary>Đọc người dùng hiện tại từ claims của request đã xác thực.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private static readonly string[] IdClaims = [ClaimTypes.NameIdentifier, "sub"];
    private static readonly string[] NameClaims = [ClaimTypes.Name, "unique_name", "preferred_username", "name"];

    public Guid? UserId => Guid.TryParse(FindClaim(IdClaims), out var id) ? id : null;

    public string? UserName => FindClaim(NameClaims);

    private string? FindClaim(string[] claimTypes)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return claimTypes.Select(user.FindFirstValue).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
