using MaintenanceModeMiddleware.Configuration.Builders;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class StatusCodeOptionsBuilderTest : StatusCodeOptionsBuilderTestBase<StatusCodeOptionsBuilder>
    {
        public StatusCodeOptionsBuilderTest()
            : base(new StatusCodeOptionsBuilder())
        { }
    }
}
