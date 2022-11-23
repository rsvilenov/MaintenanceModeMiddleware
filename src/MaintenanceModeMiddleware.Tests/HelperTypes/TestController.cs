using Microsoft.AspNetCore.Mvc;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    internal class TestController : Controller
    {
        public IActionResult Get()
        {
            return null;
        }
    }

    [Area(AreaName)]
    internal class TestControllerWithAreaAttribute : Controller
    {
        public const string AreaName = "testArea";

        public IActionResult Get()
        {
            return null;
        }
    }
}
