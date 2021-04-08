using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.StateStore;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using System.Linq;

namespace MaintenanceModeMiddleware.Tests
{
    public class FileStateStoreTests
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        public void Store(bool isOn, bool storeDate, bool storeOptions)
        {
            FileStateStore store = GetStateStore();
            var testState = new MaintenanceState
            {
                IsMaintenanceOn = isOn
            };

            if (storeDate)
            {
                testState.EndsOn = DateTime.Now;
            }

            if (storeOptions)
            {
                testState.MiddlewareOptions = new List<StorableOption>
                {
                    new StorableOption
                    {
                        StringValue = "test",
                        TypeName = "testType"
                    }
                };
            }

            Func<MaintenanceState> testFunc = () =>
            {
                store.SetState(testState);
                return store.GetState();
            };

            MaintenanceState restoredState = testFunc.ShouldNotThrow();

            restoredState.ShouldNotBeNull();
            restoredState.EndsOn.ShouldBe(testState.EndsOn);
            restoredState.IsMaintenanceOn.ShouldBe(testState.IsMaintenanceOn);

            if (storeOptions)
            {
                restoredState.MiddlewareOptions
                    .ShouldNotBeNull()
                    .ShouldNotBeEmpty();

                StorableOption firstOption = restoredState.MiddlewareOptions.First();
                firstOption.StringValue.ShouldBe("test");
                firstOption.TypeName.ShouldBe("testType");
            }
        }

        private FileStateStore GetStateStore()
        {
            string tempPath = Path.GetTempPath();

            var webHostEnv = Substitute.For<IWebHostEnvironment>();
            webHostEnv.WebRootPath = tempPath;
            webHostEnv.ContentRootPath = tempPath;

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider
                .GetService(typeof(IWebHostEnvironment))
                .Returns(webHostEnv);

            FileStateStore store = new FileStateStore(new FileDescriptor("test.json", PathBaseDirectory.ContentRootPath));
            (store as IServiceConsumer).ServiceProvider = serviceProvider;

            return store;
        }
    }
}
