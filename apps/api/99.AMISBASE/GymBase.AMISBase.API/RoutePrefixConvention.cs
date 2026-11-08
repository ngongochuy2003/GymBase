using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GymBase.AMISBase.API;

/// <summary>Kênh client mà một API host phục vụ; quyết định tiền tố route của mọi controller trong host.</summary>
public static class ApiChannel
{
    /// <summary>Các host cho trang web quản trị (SystemAPI, DIAPI, BIZAPI…): /api/web/v1/...</summary>
    public const string Web = "web";

    /// <summary>Host cho app mobile (MobAPI): /api/mobile/v1/...</summary>
    public const string Mobile = "mobile";
}

/// <summary>
/// Thêm tiền tố vào route của mọi controller dùng attribute routing, ví dụ
/// <c>v1/users</c> → <c>api/web/v1/users</c>. Nhờ vậy controller không phải tự ghi tiền tố,
/// và cùng một controller viết cho web có thể chép sang MobAPI mà chỉ đổi kênh ở Program.cs.
/// </summary>
public sealed class RoutePrefixConvention(string prefix) : IApplicationModelConvention
{
    private readonly AttributeRouteModel _prefix = new(new RouteAttribute(prefix.Trim('/')));

    public void Apply(ApplicationModel application)
    {
        foreach (var selector in application.Controllers.SelectMany(c => c.Selectors))
        {
            if (selector.AttributeRouteModel is not null)
            {
                selector.AttributeRouteModel =
                    AttributeRouteModel.CombineAttributeRouteModel(_prefix, selector.AttributeRouteModel);
            }
        }
    }
}

public static class RoutePrefixMvcOptionsExtensions
{
    /// <summary>Gắn tiền tố <c>api/{kênh}</c> (<see cref="ApiChannel"/>) cho mọi controller của host.</summary>
    public static MvcOptions UseApiChannel(this MvcOptions options, string channel)
    {
        options.Conventions.Add(new RoutePrefixConvention($"api/{channel}"));
        return options;
    }
}
