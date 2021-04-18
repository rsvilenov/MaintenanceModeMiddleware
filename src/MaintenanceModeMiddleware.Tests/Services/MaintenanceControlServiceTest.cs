﻿using MaintenanceModeMiddleware.Configuration;
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
        public void Constructor_WithAllParams_OptionBuilderDelegateShouldBeCalled()
        {
            IStateStoreService stateStoreSvc = Substitute.For<IStateStoreService>();
            bool delegateCalled = false;
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) => delegateCalled = true;
            
            new MaintenanceControlService(
                    _pathMapperSvc,
                    stateStoreSvc,
                    optionBuilderDelegate);

            delegateCalled.ShouldBeTrue();
        }

        [Fact]
        public void EnterMaintenance_WithExpirationDate_ShouldSucceed()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate);
            
            svc.EnterMaintanence(DateTime.Now.AddSeconds(5));

            MaintenanceState state = svc.GetState()
                .ShouldNotBeNull();
            state.IsMaintenanceOn.ShouldBeTrue();
            state.ExpirationDate.ShouldNotBeNull();
        }

        [Fact]
        public void EnterMaintenance_WithExpirationDate_ShouldEndMaintenanceAutomatically()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate);
            
            svc.EnterMaintanence(DateTime.Now.AddSeconds(5));

            MaintenanceState state = svc.GetState();
            TimeSpan delay = state.ExpirationDate.Value - DateTime.Now;
            Thread.Sleep((int)delay.TotalMilliseconds + 1000);
            svc.GetState().IsMaintenanceOn
                .ShouldBeFalse($"The maintenance mode didn't automatically end after the set {nameof(state.ExpirationDate)} date: {state.ExpirationDate}.");
        }

        [Fact]
        public void EnterMaintenance_WithNoParams_ShouldSucceed()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate); 
            
            svc.EnterMaintanence();

            MaintenanceState state = svc.GetState();
            state.IsMaintenanceOn.ShouldBeTrue();
            state.ExpirationDate.ShouldBeNull();
        }

        [Fact]
        public void EnterMaintenance_WhenCalledTwice_SecondTimeShouldThrow()
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
        public void EnterMaintenance_WithMiddlewareOptions_ShouldSetMiddlewareOptionsToState()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate);

            svc.EnterMaintanence(null, options =>
            {
                options.BypassUser(testUserName);
            });

            
            OptionCollection overridenMiddlewareOpts = svc
                .GetState()?.MiddlewareOptions;
            overridenMiddlewareOpts
                .ShouldNotBeNull()
                .Any<ISerializableOption>()
                .ShouldBeTrue();
            overridenMiddlewareOpts
                .GetSingleOrDefault<BypassUserNameOption>()
                .Value.ShouldBe(testUserName);
        }

        [Fact]
        public void EnterMaintenance_WithNoMiddlewareOptions_GetOptionsToOverrideShouldReturnNull()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate);
                svc.EnterMaintanence();


            svc.GetState()
                ?.MiddlewareOptions
                .ShouldBeNull();
        }

        [Fact]
        public void LeaveMaintenance_WhenMaintenanceIsOn_ShouldTurnMaintenanceOff()
        {
            Action<ServiceOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoStateStore();
            };
            MaintenanceControlService svc = new MaintenanceControlService(
                _pathMapperSvc,
                FakeStateStoreService.Create(),
                optionBuilderDelegate);
            
            svc.EnterMaintanence();
            bool isOnAfterEnter = svc.GetState().IsMaintenanceOn;
            svc.LeaveMaintanence();
            bool isStillOnAfterLeave = svc.GetState().IsMaintenanceOn;
            
            isOnAfterEnter.ShouldBeTrue();
            isStillOnAfterLeave.ShouldBeFalse();
        }
    }
}
