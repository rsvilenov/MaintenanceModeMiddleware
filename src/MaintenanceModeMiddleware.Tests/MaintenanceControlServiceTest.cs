using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using NSubstitute;
using Shouldly;
using System;
using System.Threading;
using Xunit;
using Xunit.Extensions.Ordering;

namespace MaintenanceModeMiddleware.Tests
{
    [TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]
    public class MaintenanceControlServiceTest
    {

        private const string testUserName = "testUser";

        [Fact]
        public void Constructor()
        {
            IServiceProvider svcProvider = Substitute.For<IServiceProvider>();
            bool delegateCalled = false;
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) => delegateCalled = true;
            Action testAction = () => new MaintenanceControlService(svcProvider, optionBuilderDelegate);

            testAction.ShouldNotThrow();

            delegateCalled.ShouldBeTrue();
        }

        [Fact]
        public void EnterMaintenance_EndsOn()
        {
            IServiceProvider svcProvider = Substitute.For<IServiceProvider>();
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(svcProvider, optionBuilderDelegate);
            Action testAction = () => svc.EnterMaintanence(DateTime.Now.AddSeconds(5));

            testAction.ShouldNotThrow();

            svc.IsMaintenanceModeOn.ShouldBeTrue();
            svc.EndsOn.ShouldNotBeNull();

            TimeSpan delay = svc.EndsOn.Value - DateTime.Now;
            Thread.Sleep((int)delay.TotalMilliseconds + 1000);
            svc.IsMaintenanceModeOn
                .ShouldBeFalse($"The maintenance mode didn't automatically end after the set {nameof(svc.EndsOn)} date: {svc.EndsOn}.");
        }

        [Fact]
        public void EnterMaintenance()
        {
            IServiceProvider svcProvider = Substitute.For<IServiceProvider>();
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(svcProvider, optionBuilderDelegate);
            Action testAction = () => svc.EnterMaintanence();

            testAction.ShouldNotThrow();

            svc.IsMaintenanceModeOn.ShouldBeTrue();
            svc.EndsOn.ShouldBeNull();
        }

        [Fact]
        public void EnterMaintenance_WithMiddlewareOptions()
        {
            IServiceProvider svcProvider = Substitute.For<IServiceProvider>();
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(svcProvider, optionBuilderDelegate);
            Func<OptionCollection> testFunc = () =>
            {
                svc.EnterMaintanence(null, options =>
                {
                    options.BypassUser(testUserName);
                });

                ICanOverrideMiddlewareOptions overrider = svc;
                return overrider.GetOptionsToOverride();
            };

            OptionCollection overridenMiddlewareOpts = testFunc.ShouldNotThrow();
            
            overridenMiddlewareOpts
                .ShouldNotBeNull()
                .Any<IOption>()
                .ShouldBeTrue();

            overridenMiddlewareOpts
                .GetSingleOrDefault<BypassUserNameOption>()
                .Value.ShouldBe(testUserName);
        }

        [Fact]
        public void LeaveMaintenance()
        {
            IServiceProvider svcProvider = Substitute.For<IServiceProvider>();
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(svcProvider, optionBuilderDelegate);
            Func<bool[]> testAction = () =>
            {
                svc.EnterMaintanence();
                bool isOn = svc.IsMaintenanceModeOn;
                svc.LeaveMaintanence();
                bool isStillOn = svc.IsMaintenanceModeOn;
                return new bool[] { isOn, isStillOn };
            };
            
            bool[] result = testAction.ShouldNotThrow();

            result[0].ShouldBeTrue();
            result[1].ShouldBeFalse();
        }

        [Fact]
        [Order(1)]
        public void StoreState()
        {
            IServiceProvider svcProvider = Substitute.For<IServiceProvider>();
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseStateStore(new InMemoryStateStore());
            };
            MaintenanceControlService svc = new MaintenanceControlService(svcProvider, optionBuilderDelegate);
            Action testAction = () =>
            {
                svc.EnterMaintanence(DateTime.Today, options =>
                {
                    options.BypassUser(testUserName);
                });
            };

            testAction.ShouldNotThrow();
        }

        [Fact]
        [Order(2)]
        public void RestoreState()
        {
            IServiceProvider svcProvider = Substitute.For<IServiceProvider>();
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseStateStore(new InMemoryStateStore());
            };
            MaintenanceControlService svc = new MaintenanceControlService(svcProvider, optionBuilderDelegate);
            Func<OptionCollection> testFunc = () =>
            {
                ICanRestoreState restorer = svc;
                restorer.RestoreState();

                ICanOverrideMiddlewareOptions overrider = svc;
                return overrider.GetOptionsToOverride();
            };

            OptionCollection restoredOptions = testFunc.ShouldNotThrow();
            restoredOptions.ShouldNotBeNull()
                .Any<IOption>().ShouldBeTrue();
            restoredOptions.GetSingleOrDefault<BypassUserNameOption>()
                .Value.ShouldBe(testUserName);
        }

    }
}
