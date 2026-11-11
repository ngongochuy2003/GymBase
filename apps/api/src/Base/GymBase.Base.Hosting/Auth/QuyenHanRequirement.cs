using GymBase.Base.Auth;
using Microsoft.AspNetCore.Authorization;

namespace GymBase.Base.Hosting.Auth;

/// <summary>Điều kiện về hồ sơ và trạng thái tài khoản, một trong các <see cref="AppPolicies"/> dựa trên hồ sơ.</summary>
public sealed class QuyenHanRequirement(string policy) : IAuthorizationRequirement
{
    public string Policy { get; } = policy;
}

/// <summary>
/// Đọc quyền từ database mỗi request (không tin claim cũ trong token) rồi so với điều kiện. Không thỏa thì không gọi
/// <c>Succeed</c>, framework trả 403.
/// </summary>
public sealed class QuyenHanHandler(IQuyenHanProvider provider) : AuthorizationHandler<QuyenHanRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, QuyenHanRequirement requirement)
    {
        if (!Guid.TryParse(context.User.FindFirst(JwtClaimNames.Subject)?.Value, out var taiKhoanId))
        {
            return;
        }

        var quyenHan = await provider.BLLayQuyenHanAsync(taiKhoanId, CancellationToken.None);

        var thoa = requirement.Policy switch
        {
            AppPolicies.RequireProfile => quyenHan.da_thiet_lap_ho_so,
            AppPolicies.RequirePtProfile => quyenHan.la_pt,
            AppPolicies.RequireVerifiedPt => quyenHan.la_pt_da_xac_minh,
            _ => false,
        };

        if (thoa)
        {
            context.Succeed(requirement);
        }
    }
}
