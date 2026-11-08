using System.Reflection;
using GymBase.AMISBase.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GymBase.UnitTests.AMISBase;

public class RoutePrefixConventionTests
{
    private sealed class SampleController : BaseApiController;

    [Theory]
    [InlineData(ApiChannel.Web, "api/web/v1/[controller]")]
    [InlineData(ApiChannel.Mobile, "api/mobile/v1/[controller]")]
    public void Prefixes_controller_route_with_channel(string channel, string expected)
    {
        var selector = new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel(
                typeof(BaseApiController).GetCustomAttribute<RouteAttribute>()!),
        };
        var controller = new ControllerModel(typeof(SampleController).GetTypeInfo(), []);
        controller.Selectors.Add(selector);
        var application = new ApplicationModel();
        application.Controllers.Add(controller);

        var convention = new MvcOptions().UseApiChannel(channel).Conventions.Single();
        convention.Apply(application);

        Assert.Equal(expected, selector.AttributeRouteModel!.Template);
    }

    [Fact]
    public void Leaves_conventional_routes_untouched()
    {
        var selector = new SelectorModel();
        var controller = new ControllerModel(typeof(SampleController).GetTypeInfo(), []);
        controller.Selectors.Add(selector);
        var application = new ApplicationModel();
        application.Controllers.Add(controller);

        new RoutePrefixConvention(ApiChannel.Web).Apply(application);

        Assert.Null(selector.AttributeRouteModel);
    }
}
