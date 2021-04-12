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
            FileStateStore store = GetStateStore(file, baseDir);
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

        [Fact]
        public void GetState_File_Is_Empty()
        {
            string generatedFilePath = null;
            FileStateStore store = GetStateStore("test_to_be_emptied.json", null, (filePath) =>
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
            FileStateStore store = GetStateStore("test_to_be_deleted.json", null, (filePath) =>
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
            FileStateStore store = GetStateStore("test_dir/test_to_be_deleted.json", null, (filePath) =>
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
            FileStateStore store = GetStateStore("invalid_basedir.json",
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
            Action<string> onFileGenerated = null)
        {
            string tempPath = Path.GetTempPath();

            var webHostEnv = Substitute.For<IWebHostEnvironment>();
            webHostEnv.WebRootPath = tempPath;
            webHostEnv.ContentRootPath = tempPath;

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider
                .GetService(typeof(IWebHostEnvironment))
                .Returns(webHostEnv);

            string randFilePrefix = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            FileDescriptor fileDescriptor = baseDir.HasValue
                ? new FileDescriptor($"{randFilePrefix}{file}", baseDir.Value)
                : new FileDescriptor(Path.Combine(tempPath, $"{randFilePrefix}{file}"));

            FileStateStore store = new FileStateStore(fileDescriptor);
            (store as IServiceConsumer).ServiceProvider = serviceProvider;

            onFileGenerated?.Invoke(Path.Combine(tempPath, $"{randFilePrefix}{file}"));

            return store;
        }
    }
}
