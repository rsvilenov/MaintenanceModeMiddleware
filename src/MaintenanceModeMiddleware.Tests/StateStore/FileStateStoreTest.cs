using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.StateStore;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using NSubstitute;
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

        [Theory]
        [InlineData("test1.json", null)]
        [InlineData("test2.json", EnvDirectory.ContentRootPath)]
        [InlineData("test3.json", EnvDirectory.WebRootPath)]
        public void SetStore_InVariousPaths_ShouldStoreAndRestoreCorrectData(string file, EnvDirectory? baseDir)
        {
            FileStateStore store = GetStateStore(SafeTempPath.Create(file), baseDir);
            var testState = new StorableMaintenanceState
            {
                IsMaintenanceOn = true,
                ExpirationDate = DateTime.Now
            };
            Func<StorableMaintenanceState> testFunc = () =>
            {
                store.SetState(testState);
                return store.GetState();
            };

            StorableMaintenanceState restoredState = testFunc.ShouldNotThrow();

            restoredState.ShouldNotBeNull();
            restoredState.ExpirationDate.ShouldBe(testState.ExpirationDate);
            restoredState.IsMaintenanceOn.ShouldBe(testState.IsMaintenanceOn);
        }

        [Theory]
        [InlineData("test2.json", EnvDirectory.ContentRootPath, EnvDirectory.ContentRootPath, true)]
        [InlineData("test2.json", EnvDirectory.WebRootPath, EnvDirectory.WebRootPath, true)]
        [InlineData("test2.json", EnvDirectory.ContentRootPath, EnvDirectory.WebRootPath, false)]
        public void SetStore_InVariousPathsWithMismatch_WhenMismatchShouldRestoreNull(string file, 
            EnvDirectory baseDirStore, 
            EnvDirectory baseDirRestore,
            bool shouldSucceed)
        {
            var testState = new StorableMaintenanceState();
            string tempDir = Path.GetTempPath();
            string prefixedFileName = SafeTempPath.Create(file); 
            string tempPath = null;

            // store
            FileStateStore storeWrite = GetStateStore(prefixedFileName, baseDirStore,
                (tf) => tempPath = tf,
                tempDir);
            Action testActionStore = () =>
            {
                storeWrite.SetState(testState);
            };

            testActionStore.ShouldNotThrow();

            // restore
            FileStateStore storeRead = GetStateStore(prefixedFileName, baseDirRestore, null, tempDir);
            Func<StorableMaintenanceState> testFuncRestore = () =>
            {
                return storeRead.GetState();
            };

            StorableMaintenanceState restoredState = testFuncRestore.ShouldNotThrow();

            if (shouldSucceed)
            {
                restoredState.ShouldNotBeNull();
            }
            else
            {
                restoredState.ShouldBeNull();
            }
        }

        [Fact]
        public void GetState_WhenFileIsEmpty_ShouldReturnNull()
        {
            string generatedFilePath = null;
            FileStateStore store = GetStateStore(SafeTempPath.Create("test_to_be_emptied.json"), null, (filePath) =>
            {
                generatedFilePath = filePath;
            });
            File.WriteAllText(generatedFilePath, "");

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
            string generatedFilePath = null;
            FileStateStore store = GetStateStore(SafeTempPath.Create("test_to_be_deleted.json"), null, (filePath) =>
            {
                generatedFilePath = filePath;
            });
            if (File.Exists(generatedFilePath))
            {
                File.Delete(generatedFilePath);
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
            string generatedFilePath = null;
            FileStateStore store = GetStateStore(SafeTempPath.Create("test_dir/test_to_be_deleted.json"), null, (filePath) =>
            {
                generatedFilePath = filePath;
            });

            Action testAction = () =>
            {
                store.SetState(new StorableMaintenanceState());
            };

            testAction
                .ShouldNotThrow();
        }

        private FileStateStore GetStateStore(string file = "test.json",
            EnvDirectory? baseDir = EnvDirectory.ContentRootPath,
            Action<string> onFileGenerated = null,
            string tempDir = null)
        {
            if (tempDir == null)
            {
                tempDir = Path.GetTempPath();
            }

            var pathMapperSvc = FakePathMapperService.Create(tempDir);

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider
                .GetService(typeof(IPathMapperService))
                .Returns(pathMapperSvc);

            FileDescriptor fileDescriptor = baseDir.HasValue
                ? new FileDescriptor(file, baseDir.Value)
                : new FileDescriptor(Path.Combine(tempDir, file));

            FileStateStore store = new FileStateStore(fileDescriptor);
            (store as IServiceConsumer).ServiceProvider = serviceProvider;

            onFileGenerated?.Invoke(Path.Combine(tempDir, file));

            return store;
        }
    }
}
