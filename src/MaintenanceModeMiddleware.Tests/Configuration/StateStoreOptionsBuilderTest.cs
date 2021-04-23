using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.StateStore;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using NSubstitute;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class StateStoreOptionsBuilderTest
    {
        [Fact]
        public void UseNoStateStore_WhenCalled_GetStateStoreShouldReturnNull()
        {
            StateStoreOptionsBuilder builder = new StateStoreOptionsBuilder();

            builder.UseNoStateStore();

            builder.GetStateStoreInstance()
                .ShouldBeNull();
            builder.GetStateStoreType()
                .ShouldBeNull();
        }

        [Fact]
        public void CustomStateStore_WithStateStoreInstance_GetStateStoreShouldEqualPassed()
        {
            StateStoreOptionsBuilder builder = new StateStoreOptionsBuilder();
            IStateStore storeSubstitute = Substitute.For<IStateStore>();

            builder.UseStateStore(storeSubstitute);

            builder.GetStateStoreInstance()
                .ShouldNotBeNull()
                .ShouldBeOfType(storeSubstitute.GetType());
        }

        [Fact]
        public void CustomStateStore_WithStateStoreType_GetStateStoreShouldEqualPassed()
        {
            StateStoreOptionsBuilder builder = new StateStoreOptionsBuilder();
            
            builder.UseStateStore<InMemoryStateStore>();

            builder.GetStateStoreType()
                .ShouldNotBeNull()
                .ShouldBe(typeof(InMemoryStateStore));
        }

        [Fact]
        public void CustomStateStore_WithNull_ShouldThrowArgumentNullException()
        {
            StateStoreOptionsBuilder builder = new StateStoreOptionsBuilder();
            IStateStore stateStore = null;

            Action testAction = () =>
            {
                builder.UseStateStore(stateStore);
            };

            testAction.ShouldThrow<ArgumentNullException>();
        }
    }
}
