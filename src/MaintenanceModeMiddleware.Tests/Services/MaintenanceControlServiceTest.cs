using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using Shouldly;
using System;
using System.Threading;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Services
{
    public class MaintenanceControlServiceTest
    {
        private const string testUserName = "testUser";
        private readonly IPathMapperService _pathMapperSvc = FakePathMapperService.Create();

        [Fact]
        public void Constructor()
        {
            IStateStoreService stateStoreSvc = Substitute.For<IStateStoreService>();
            bool delegateCalled = false;
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) => delegateCalled = true;
            Action testAction = () => 
                new MaintenanceControlService(
                    _pathMapperSvc,
                    stateStoreSvc,
                    optionBuilderDelegate);

            testAction.ShouldNotThrow();

            delegateCalled.ShouldBeTrue();
        }

        [Fact]
        public void EnterMaintenance_ExpirationDate()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate);
            Action testAction = () => svc.EnterMaintanence(DateTime.Now.AddSeconds(5));

            testAction.ShouldNotThrow();
            MaintenanceState state = svc.GetState()
                .ShouldNotBeNull();
            state.IsMaintenanceOn.ShouldBeTrue();
            state.ExpirationDate.ShouldNotBeNull();

            TimeSpan delay = state.ExpirationDate.Value - DateTime.Now;
            Thread.Sleep((int)delay.TotalMilliseconds + 1000);
            svc.GetState().IsMaintenanceOn
                .ShouldBeFalse($"The maintenance mode didn't automatically end after the set {nameof(state.ExpirationDate)} date: {state.ExpirationDate}.");
        }

        [Fact]
        public void EnterMaintenance()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate); 
            Action testAction = () => svc.EnterMaintanence();

            testAction.ShouldNotThrow();

            MaintenanceState state = svc.GetState();
            state.IsMaintenanceOn.ShouldBeTrue();
            state.ExpirationDate.ShouldBeNull();
        }

        [Fact]
        public void EnterMaintenance_Twice()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate); 
            Action testAction = () => svc.EnterMaintanence();

            testAction.ShouldNotThrow();
            // the second call to EnterMaintenance should throw,
            // as the maintenance mode is alreary on
            testAction.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void EnterMaintenance_WithMiddlewareOptions()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate);
            Func<OptionCollection> testFunc = () =>
            {
                svc.EnterMaintanence(null, options =>
                {
                    options.BypassUser(testUserName);
                });

                return svc.GetState()?.MiddlewareOptions;
            };

            OptionCollection overridenMiddlewareOpts = testFunc
                .ShouldNotThrow();
            
            overridenMiddlewareOpts
                .ShouldNotBeNull()
                .Any<ISerializableOption>()
                .ShouldBeTrue();

            overridenMiddlewareOpts
                .GetSingleOrDefault<BypassUserNameOption>()
                .Value.ShouldBe(testUserName);
        }

        [Fact]
        public void EnterMaintenance_WithNoMiddlewareOptions_GetOptionsToOverride_Should_Return_Null()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate);
            Func<OptionCollection> testFunc = () =>
            {
                svc.EnterMaintanence();

                return svc.GetState()?.MiddlewareOptions;
            };

            testFunc
                .ShouldNotThrow()
                .ShouldBeNull();
        }

        [Fact]
        public void LeaveMaintenance()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate);
            Func<bool[]> testAction = () =>
            {
                svc.EnterMaintanence();
                bool isOn = svc.GetState().IsMaintenanceOn;
                svc.LeaveMaintanence();
                bool isStillOn = svc.GetState().IsMaintenanceOn;
                return new bool[] { isOn, isStillOn };
            };
            
            bool[] result = testAction.ShouldNotThrow();

            result[0].ShouldBeTrue();
            result[1].ShouldBeFalse();
        }
    }
}
