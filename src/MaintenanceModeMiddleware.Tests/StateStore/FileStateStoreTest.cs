using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.StateStore;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.StateStore
{
    public class FileStateStoreTest
    {

        [Fact]
        public void Store_WithEmptyState_ShouldNotThrow()
        {
            FileStateStore store = GetStateStore();
            Action testAction = () 
                => store.SetState(new StorableMaintenanceState());

            testAction.ShouldNotThrow();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Store_WithStateState_StateShouldEqualInput(bool isOn)
        {
            FileStateStore store = GetStateStore();
            var testState = new StorableMaintenanceState
            {
                IsMaintenanceOn = isOn
            };

            store.SetState(testState);

            store.GetState().IsMaintenanceOn
                .ShouldBe(isOn);
        }

        [Fact]
        public void Store_WithStateStateWithDate_DateShouldEqualInput()
        {
            FileStateStore store = GetStateStore();
            var testState = new StorableMaintenanceState
            {
                ExpirationDate = DateTime.Now
            };

            store.SetState(testState);

            store.GetState().ExpirationDate
                .ShouldBe(testState.ExpirationDate);
        }

        [Fact]
        public void Store_WithStateStateWithOptions_OptionsShouldEqualInput()
        {
            FileStateStore store = GetStateStore();
            var testState = new StorableMaintenanceState();
            StorableOption testOption = new StorableOption
            {
                StringValue = "test",
                TypeName = "testType"
            };
            testState.MiddlewareOptions = new List<StorableOption>
            {
                testOption
            };

            store.SetState(testState);

            store.GetState().MiddlewareOptions
                .ShouldNotBeNull()
                .ShouldNotBeEmpty();
            StorableOption firstOption = store.GetState()
                .MiddlewareOptions.First();
            firstOption.StringValue
                .ShouldBe(testOption.StringValue);
            firstOption.TypeName
                .ShouldBe(testOption.TypeName);
        }

        [Fact]
        public void GetState_WhenFileIsEmpty_ShouldReturnNull()
        {
            string tempDir = null;
            FileStateStore store = GetStateStore((s) => { tempDir = s; });
            store.SetState(new StorableMaintenanceState());
            if (Directory.Exists(tempDir))
            {
                var fileToNullOut = Directory.GetFiles(tempDir, "*.json").FirstOrDefault();
                File.WriteAllText(fileToNullOut, "");
            }
            
            Func<StorableMaintenanceState> testFunc = () =>
            {
                return store.GetState();
            };

            testFunc
                .ShouldNotThrow()
                .ShouldBeNull();
        }

        [Fact]
        public void GetState_WhenFileDoesNotExist_ShouldReturnNull()
        {
            string tempDir = null;
            FileStateStore store = GetStateStore((s) => { tempDir = s; });
            store.SetState(new StorableMaintenanceState());

            if (Directory.Exists(tempDir))
            {
                var fileToDelete = Directory.GetFiles(tempDir, "*.json").FirstOrDefault();
                File.Delete(fileToDelete);
            }

            Func<StorableMaintenanceState> testFunc = () =>
            {
                return store.GetState();
            };

            testFunc
                .ShouldNotThrow()
                .ShouldBeNull();
        }

        [Fact]
        public void SetState_WhenDirectoryDoesNotExist_ShouldNotThrow()
        {
            FileStateStore store = GetStateStore();

            Action testAction = () =>
            {
                store.SetState(new StorableMaintenanceState());
            };

            testAction
                .ShouldNotThrow();
        }

        private FileStateStore GetStateStore(Action<string> onFullTempDirGenerated = null)
        {
            string tempDir = Path.GetTempPath();
            tempDir = SafeTempPath.Create(tempDir);

            var pathMapperSvc = FakePathMapperService.Create(tempDir);
            string fullTempPath = pathMapperSvc.GetPath(EnvDirectory.ContentRootPath);
            onFullTempDirGenerated?.Invoke(fullTempPath);

            FileStateStore store = new FileStateStore(pathMapperSvc);

            return store;
        }
    }
}
