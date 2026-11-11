using System.Reflection;
using GymBase.Base.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GymBase.ArchitectureTests
{
    /// <summary>Tiền tố route theo kênh: controller trong Mob/Controllers là mobile, còn lại là web.</summary>
    public class ChannelRouteConventionTests
    {
        [Theory]
        [InlineData(typeof(Samples.Controllers.SampleController), "api/v1/web/[controller]", "web")]
        [InlineData(typeof(Samples.Mob.Controllers.SampleController), "api/v1/mob/[controller]", "mob")]
        public void Prefixes_route_and_sets_area_by_namespace(Type controllerType, string expectedRoute, string expectedArea)
        {
            var (application, controller, selector) = Build(controllerType, withRoute: true);

            new MvcOptions().UseApiChannels().Conventions.Single().Apply(application);

            Assert.Equal(expectedRoute, selector.AttributeRouteModel!.Template);
            Assert.Equal(expectedArea, controller.RouteValues["area"]);
        }

        [Fact]
        public void Leaves_conventional_routes_untouched()
        {
            var (application, _, selector) = Build(typeof(Samples.Controllers.SampleController), withRoute: false);

            new ChannelRouteConvention().Apply(application);

            Assert.Null(selector.AttributeRouteModel);
        }

        private static (ApplicationModel, ControllerModel, SelectorModel) Build(Type controllerType, bool withRoute)
        {
            var selector = new SelectorModel
            {
                AttributeRouteModel = withRoute
                    ? new AttributeRouteModel(typeof(BaseApiController).GetCustomAttribute<RouteAttribute>()!)
                    : null,
            };
            var controller = new ControllerModel(controllerType.GetTypeInfo(), []);
            controller.Selectors.Add(selector);
            var application = new ApplicationModel();
            application.Controllers.Add(controller);
            return (application, controller, selector);
        }
    }
}

namespace GymBase.ArchitectureTests.Samples.Controllers
{
    public sealed class SampleController : BaseApiController;
}

namespace GymBase.ArchitectureTests.Samples.Mob.Controllers
{
    public sealed class SampleController : BaseApiController;
}
