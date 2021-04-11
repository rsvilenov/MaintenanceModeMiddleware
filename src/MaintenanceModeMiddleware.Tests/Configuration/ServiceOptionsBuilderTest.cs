using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.StateStore;
using NSubstitute;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class ServiceOptionsBuilderTest
    {
        [Fact]
        public void ServiceOptionsBuilder_NoStateStore()
        {
            ServiceOptionsBuilder builder = new ServiceOptionsBuilder();
            IStateStore stateStore = null;
            Action testAction = () =>
            {
                builder.UseNoStateStore();
                stateStore = builder.GetStateStore();
            };

            testAction.ShouldNotThrow();

            stateStore.ShouldBeNull();
        }

        [Fact]
        public void ServiceOptionsBuilder_DefaultStateStore()
        {
            ServiceOptionsBuilder builder = new ServiceOptionsBuilder();
            IStateStore stateStore = null;
            Action testAction = () =>
            {
                builder.UseDefaultStateStore();
                stateStore = builder.GetStateStore();
            };

            testAction.ShouldNotThrow();

            stateStore.ShouldNotBeNull();
            stateStore.ShouldBeOfType<FileStateStore>();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ServiceOptionsBuilder_CustomStateStore(bool passNull)
        {
            ServiceOptionsBuilder builder = new ServiceOptionsBuilder();
            IStateStore stateStore = null;
            IStateStore storeSubstitute = passNull 
                ? null 
                : Substitute.For<IStateStore>();

            Action testAction = () =>
            {
                builder.UseStateStore(storeSubstitute);
                stateStore = builder.GetStateStore();
            };

            if (passNull)
            {
                testAction.ShouldThrow<ArgumentNullException>();
            }
            else
            {
                testAction.ShouldNotThrow();

                stateStore.ShouldNotBeNull();
                stateStore.ShouldBe(storeSubstitute);
            }
        }
    }
}
