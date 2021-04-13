using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.State;
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

        [Theory]
        [InlineData("test1.json", null)]
        [InlineData("test2.json", PathBaseDirectory.ContentRootPath)]
        [InlineData("test3.json", PathBaseDirectory.WebRootPath)]
        public void Store_In_Various_Paths(string file, PathBaseDirectory? baseDir)
        {
            FileStateStore store = GetStateStore(SafeTempFileName.Create(file), baseDir);
            var testState = new MaintenanceState
            {
                IsMaintenanceOn = true,
                EndsOn = DateTime.Now
            };


            Func<MaintenanceState> testFunc = () =>
            {
                store.SetState(testState);
                return store.GetState();
            };

            MaintenanceState restoredState = testFunc.ShouldNotThrow();

            restoredState.ShouldNotBeNull();
            restoredState.EndsOn.ShouldBe(testState.EndsOn);
            restoredState.IsMaintenanceOn.ShouldBe(testState.IsMaintenanceOn);
        }

        [Theory]
        [InlineData("test2.json", PathBaseDirectory.ContentRootPath, PathBaseDirectory.ContentRootPath, true)]
        [InlineData("test2.json", PathBaseDirectory.WebRootPath, PathBaseDirectory.WebRootPath, true)]
        [InlineData("test2.json", PathBaseDirectory.ContentRootPath, PathBaseDirectory.WebRootPath, false)]
        public void Store_In_Various_Paths_Match_Mismatch(string file, 
            PathBaseDirectory baseDirStore, 
            PathBaseDirectory baseDirRestore,
            bool shouldSucceed)
        {
            var testState = new MaintenanceState();
            string tempDir = Path.GetTempPath();
            string prefixedFileName = SafeTempFileName.Create(file);
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

            Func<MaintenanceState> testFuncRestore = () =>
            {
                FileStateStore storeRead = GetStateStore(prefixedFileName, baseDirRestore, null, tempDir);
                return storeRead.GetState();
            };

            MaintenanceState restoredState = testFuncRestore.ShouldNotThrow();

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
            FileStateStore store = GetStateStore(SafeTempFileName.Create("test_to_be_emptied.json"), null, (filePath) =>
            {
                generatedFilePath = filePath;
            });

            Func<MaintenanceState> testFunc = () =>
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
            FileStateStore store = GetStateStore(SafeTempFileName.Create("test_to_be_deleted.json"), null, (filePath) =>
            {
                generatedFilePath = filePath;
            });

            Func<MaintenanceState> testFunc = () =>
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
            FileStateStore store = GetStateStore(SafeTempFileName.Create("test_dir/test_to_be_deleted.json"), null, (filePath) =>
            {
                generatedFilePath = filePath;
            });

            Action testAction = () =>
            {
                store.SetState(new MaintenanceState());
            };

            testAction
                .ShouldNotThrow();
        }

        [Fact]
        public void SetState_PathBaseDirectory_Invalid_Enum_Value()
        {
            string generatedFilePath = null;
            FileStateStore store = GetStateStore(SafeTempFileName.Create("invalid_basedir.json"),
                (PathBaseDirectory)(-1),
                (filePath) =>
            {
                generatedFilePath = filePath;
            });

            Action testAction = () =>
            {
                store.SetState(new MaintenanceState());
            };

            testAction
                .ShouldThrow<InvalidOperationException>();
        }

        private FileStateStore GetStateStore(string file = "test.json",
            PathBaseDirectory? baseDir = PathBaseDirectory.ContentRootPath,
            Action<string> onFileGenerated = null,
            string tempDir = null)
        {
            if (tempDir == null)
            {
                tempDir = Path.GetTempPath();
            }

            var webHostEnv = FakeWebHostEnvironment.Create(tempDir);

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider
                .GetService(typeof(IWebHostEnvironment))
                .Returns(webHostEnv);

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
