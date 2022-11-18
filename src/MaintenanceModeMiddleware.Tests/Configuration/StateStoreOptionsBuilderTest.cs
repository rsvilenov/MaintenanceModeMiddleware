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
        private readonly StateStoreOptionsBuilder _builder;

        public StateStoreOptionsBuilderTest()
        {
            _builder = new StateStoreOptionsBuilder();
        }

        [Fact]
        public void UseNoStateStore_WhenCalled_GetStateStoreShouldReturnNull()
        {
            // Act
            _builder.UseNoStateStore();

            // Assert
            _builder.GetStateStoreInstance()
                .ShouldBeNull();
            _builder.GetStateStoreType()
                .ShouldBeNull();
        }

        [Fact]
        public void CustomStateStore_WithStateStoreInstance_GetStateStoreShouldEqualPassed()
        {
            // Arrange
            IStateStore storeSubstitute = Substitute.For<IStateStore>();

            // Act
            _builder.UseStateStore(storeSubstitute);

            // Assert
            _builder.GetStateStoreInstance()
                .ShouldNotBeNull()
                .ShouldBeOfType(storeSubstitute.GetType());
        }

        [Fact]
        public void CustomStateStore_WithStateStoreType_GetStateStoreShouldEqualPassed()
        {
            // Act
            _builder.UseStateStore<InMemoryStateStore>();

            // Assert
            _builder.GetStateStoreType()
                .ShouldNotBeNull()
                .ShouldBe(typeof(InMemoryStateStore));
        }

        [Fact]
        public void CustomStateStore_WithNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IStateStore stateStore = null;

            Action testAction = () =>
            {
                _builder.UseStateStore(stateStore);
            };

            // Act & Assert
            testAction.ShouldThrow<ArgumentNullException>();
        }
    }
}
