using MaintenanceModeMiddleware.Configuration.Builders;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class StatusCodeOptionsBuilderTest : StatusCodeOptionsBuilderTestBase
    {
        public StatusCodeOptionsBuilderTest()
            : base(new StatusCodeOptionsBuilder())
        { }
    }
}
