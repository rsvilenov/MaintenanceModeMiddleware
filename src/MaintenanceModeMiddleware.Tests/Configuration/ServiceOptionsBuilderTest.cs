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
        public void UseNoStateStore_WhenCalled_GetStateStoreShouldReturnNull()
        {
            ServiceOptionsBuilder builder = new ServiceOptionsBuilder();

            builder.UseNoStateStore();

            builder.GetStateStore()
                .ShouldBeNull();
        }

        [Fact]
        public void UseDefaultStateStore_WhenCalled_GetStateStoreShouldBeDefault()
        {
            ServiceOptionsBuilder builder = new ServiceOptionsBuilder();

            builder.UseDefaultStateStore();

            builder.GetStateStore()
                .ShouldNotBeNull()
                .ShouldBeOfType<FileStateStore>();
        }

        [Fact]
        public void CustomStateStore_WithStateStore_GetStateStoreShouldEqualPassed()
        {
            ServiceOptionsBuilder builder = new ServiceOptionsBuilder();
            IStateStore storeSubstitute = Substitute.For<IStateStore>();

            builder.UseStateStore(storeSubstitute);

            builder.GetStateStore()
                .ShouldNotBeNull()
                .ShouldBeOfType(storeSubstitute.GetType());
        }

        [Fact]
        public void CustomStateStore_WithNull_ShouldThrowArgumentNullException()
        {
            ServiceOptionsBuilder builder = new ServiceOptionsBuilder();
            IStateStore stateStore = null;

            Action testAction = () =>
            {
                builder.UseStateStore(stateStore);
            };

            testAction.ShouldThrow<ArgumentNullException>();
        }
    }
}
