using MaintenanceModeMiddleware.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Extensions
{
    public class DependencyInjectionExtensionTest
    {
        [Fact]
        public void AddMaintenance_WhenUsed_ShouldAddServices()
        {
            var svcCollection = new ServiceCollection();

            svcCollection.AddMaintenance();

            svcCollection.ShouldNotBeEmpty();
        }
    }
}
