using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GymBase.Base.Hosting;

/// <summary>
/// Kênh client của một controller, suy ra từ namespace: controller trong thư mục <c>Mob/Controllers</c>
/// (namespace có đoạn <c>Mob</c>) phục vụ mobile, còn lại phục vụ web.
/// </summary>
public static class ApiChannel
{
    /// <summary>Controller cho web (thư mục <c>Controllers/</c>): /api/v1/web/...</summary>
    public const string Web = "web";

    /// <summary>Controller cho mobile (thư mục <c>Mob/Controllers/</c>): /api/v1/mob/...</summary>
    public const string Mobile = "mob";

    public static string Of(Type controllerType) =>
        controllerType.Namespace?.Split('.').Contains("Mob") == true ? Mobile : Web;
}

/// <summary>
/// Thêm tiền tố <c>api/v1/{kênh}</c> vào route của mọi controller dùng attribute routing, ví dụ
/// <c>[controller]</c> → <c>api/v1/web/users</c> hoặc <c>api/v1/mob/users</c>. Kênh cũng được gắn làm
/// route value <c>area</c>, để hai controller cùng tên (web và mob) tạo link đúng kênh, ví dụ header
/// <c>Location</c> của <c>CreatedAtAction</c>.
/// </summary>
public sealed class ChannelRouteConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            var channel = ApiChannel.Of(controller.ControllerType);
            controller.RouteValues["area"] = channel;

            var prefix = new AttributeRouteModel(new RouteAttribute($"api/v1/{channel}"));
            foreach (var selector in controller.Selectors.Where(s => s.AttributeRouteModel is not null))
            {
                selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(prefix, selector.AttributeRouteModel);
            }
        }
    }
}

public static class ChannelRouteMvcOptionsExtensions
{
    /// <summary>Gắn tiền tố <c>api/v1/web</c> hoặc <c>api/v1/mob</c> (<see cref="ApiChannel"/>) cho mọi controller.</summary>
    public static MvcOptions UseApiChannels(this MvcOptions options)
    {
        options.Conventions.Add(new ChannelRouteConvention());
        return options;
    }
}
