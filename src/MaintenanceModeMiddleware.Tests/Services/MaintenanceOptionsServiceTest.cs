using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Services;
using NSubstitute;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Services
{
    public class MaintenanceOptionsServiceTest
    {
        private readonly MaintenanceOptionsService _maintenanceOptionsService;

        public MaintenanceOptionsServiceTest()
        {
            _maintenanceOptionsService = new MaintenanceOptionsService();
        }

        [Fact]
        public void SetStartupOptions_WithNull_ShouldThrow()
        {
            Action action = () => _maintenanceOptionsService.SetStartupOptions(null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void SetCurrentOptions_WithNull_ShouldNotThrow()
        {
            Action action = () => _maintenanceOptionsService.SetCurrentOptions(null);
            action.ShouldNotThrow();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SetOptions_WithOptions_OptionsShouldMatch(bool useSetCurrentOptions)
        {
            OptionCollection options = new OptionCollection();
            IOption option = Substitute.For<IOption>();
            options.Add(option);

            if (useSetCurrentOptions)
            {
                _maintenanceOptionsService.SetStartupOptions(options);
            }
            else
            {
                _maintenanceOptionsService.SetStartupOptions(options);
            }

            _maintenanceOptionsService.GetOptions()
                .ShouldNotBeNull()
                .GetSingleOrDefault<IOption>()
                .ShouldBeEquivalentTo(option);
        }
    }
}
