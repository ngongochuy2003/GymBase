using GymBase.Base.Auth;
using GymBase.Base.Exceptions;
using GymBase.Base.Hosting.Auth;
using GymBase.BLBiz;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace GymBase.API;

/// <summary>
/// Policy <see cref="AppPolicies.RequireProfile"/> không đạt vì tài khoản chưa thiết lập hồ sơ thì trả 403 kèm mã
/// <see cref="BizErrorCodes.ProfileRequired"/>, để app dẫn người dùng tới bước thiết lập hồ sơ. Các policy khác giữ cách
/// trả mặc định. Đặt ở host vì mã thuộc nhóm Biz mà Base.Hosting không tham chiếu được; lỗi ném ở đây đi qua
/// <c>UseExceptionHandler</c> nên ra ProblemDetails như mọi lỗi nghiệp vụ.
/// </summary>
public sealed class ProfileRequiredResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public Task HandleAsync(
        RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        var thieuHoSo = authorizeResult.Forbidden
            && authorizeResult.AuthorizationFailure is { } failure
            && failure.FailedRequirements.OfType<QuyenHanRequirement>().Any(r => r.Policy == AppPolicies.RequireProfile);
        if (thieuHoSo)
        {
            throw new ForbiddenException("Bạn cần thiết lập hồ sơ trước khi dùng chức năng này.", BizErrorCodes.ProfileRequired);
        }

        return _default.HandleAsync(next, context, policy, authorizeResult);
    }
}
