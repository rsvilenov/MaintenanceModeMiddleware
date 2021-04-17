using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.StateStore;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Hosting;
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
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        public void Store(bool isOn, bool storeDate, bool storeOptions)
        {
            FileStateStore store = GetStateStore();
            var testState = new StorableMaintenanceState
            {
                IsMaintenanceOn = isOn
            };

            if (storeDate)
            {
                testState.ExpirationDate = DateTime.Now;
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

            Func<StorableMaintenanceState> testFunc = () =>
            {
                store.SetState(testState);
                return store.GetState();
            };

            StorableMaintenanceState restoredState = testFunc.ShouldNotThrow();

            restoredState.ShouldNotBeNull();
            restoredState.ExpirationDate.ShouldBe(testState.ExpirationDate);
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

        [Theory]
        [InlineData("test1.json", null)]
        [InlineData("test2.json", EnvDirectory.ContentRootPath)]
        [InlineData("test3.json", EnvDirectory.WebRootPath)]
        public void Store_In_Various_Paths(string file, EnvDirectory? baseDir)
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
        public void Store_In_Various_Paths_Match_Mismatch(string file, 
            EnvDirectory baseDirStore, 
            EnvDirectory baseDirRestore,
            bool shouldSucceed)
        {
            var testState = new StorableMaintenanceState();
            string tempDir = Path.GetTempPath();
            string prefixedFileName = SafeTempPath.Create(file);
            Func<string> testFuncStore = () =>
            {
                string tempPath = null;
                FileStateStore storeWrite = GetStateStore(prefixedFileName, baseDirStore, 
                    (tf) => tempPath = tf, 
                    tempDir);
                storeWrite.SetState(testState);
                return tempPath;
            };

            string tempFilePath = testFuncStore.ShouldNotThrow();

            Func<StorableMaintenanceState> testFuncRestore = () =>
            {
                FileStateStore storeRead = GetStateStore(prefixedFileName, baseDirRestore, null, tempDir);
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
        public void GetState_File_Is_Empty()
        {
            string generatedFilePath = null;
            FileStateStore store = GetStateStore(SafeTempPath.Create("test_to_be_emptied.json"), null, (filePath) =>
            {
                generatedFilePath = filePath;
            });

            Func<StorableMaintenanceState> testFunc = () =>
            {
                File.WriteAllText(generatedFilePath, "");
                return store.GetState();
            };

            testFunc
                .ShouldNotThrow()
                .ShouldBeNull();
        }

        [Fact]
        public void GetState_File_Does_Not_Exist()
        {
            string generatedFilePath = null;
            FileStateStore store = GetStateStore(SafeTempPath.Create("test_to_be_deleted.json"), null, (filePath) =>
            {
                generatedFilePath = filePath;
            });

            Func<StorableMaintenanceState> testFunc = () =>
            {
                if (File.Exists(generatedFilePath))
                {
                    File.Delete(generatedFilePath);
                }

                return store.GetState();
            };

            testFunc
                .ShouldNotThrow()
                .ShouldBeNull();
        }

        [Fact]
        public void SetState_Directory_Does_Not_Exist()
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
