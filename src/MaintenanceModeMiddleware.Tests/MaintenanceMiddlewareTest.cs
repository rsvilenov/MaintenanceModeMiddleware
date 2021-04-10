using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace MaintenanceModeMiddleware.Tests
{
    public class MaintenanceMiddlewareTest
    {
        private readonly string _tempDir;
        public MaintenanceMiddlewareTest()
        {
            _tempDir = Path.GetTempPath();
        }

        [Fact]
        public void Constructor_DefaultOptions()
        {
            bool isBuilderCalled = false;
            Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                isBuilderCalled = true;
            };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    null,
                    null,
                    optionBuilderDelegate);

            testAction.ShouldNotThrow();
            isBuilderCalled.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true, true, null, null)]
        [InlineData(false, true, typeof(ArgumentException), "No response was specified")]
        [InlineData(true, false, typeof(ArgumentException), "No value was specified for 503")]
        public void Constructor_MandatoryOptions(
            bool responseSpecified,
            bool retryIntervalSpecified,
            Type expectedException,
            string expectedExMsgStart)
        {
            Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseNoDefaultValues();

                if (responseSpecified)
                {
                    options.UseDefaultResponse();
                }

                if (retryIntervalSpecified)
                {
                    options.Set503RetryAfterInterval(1000);
                }
            };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    null,
                    null,
                    optionBuilderDelegate);

            if (expectedException == null)
            {
                testAction.ShouldNotThrow();
            }
            else
            {
                testAction.ShouldThrow(expectedException)
                    .Message.ShouldStartWith(expectedExMsgStart);
            }
        }

        const string testFileName = "test.txt";
        [Theory]
        [InlineData(testFileName, null, null)]
        [InlineData("nonexistent.txt", typeof(ArgumentException), "Could not find file")]
        public void Constructor_ResponseFileOption(
            string filePath,
            Type expectedException,
            string expectedExMsgStart)
        {
            IWebHostEnvironment webHostEnv = Substitute.For<IWebHostEnvironment>();
            webHostEnv.ContentRootPath.Returns(_tempDir);
            File.Create(Path.Combine(webHostEnv.ContentRootPath, testFileName))
                .Dispose();

            Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) =>
            {
                options.UseResponseFile(filePath, PathBaseDirectory.ContentRootPath);
                // prevent other exceptions due to missing required options
                options.FillEmptyOptionsWithDefault();
            };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    null,
                    webHostEnv,
                    optionBuilderDelegate);

            if (expectedException == null)
            {
                testAction.ShouldNotThrow();
            }
            else
            {
                testAction.ShouldThrow(expectedException)
                    .Message.ShouldStartWith(expectedExMsgStart);
            }
        }

        [Fact]
        public void Constructor_RestoreSvcOptions()
        {
            IMaintenanceControlService svc = Substitute.For<IMaintenanceControlService, ICanRestoreState>();

            Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) => { };

            Action testAction = () =>
                new MaintenanceMiddleware(null,
                    svc,
                    null,
                    optionBuilderDelegate);

            testAction.ShouldNotThrow();

            (svc as ICanRestoreState).Received(1)
                                     .RestoreState();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Invke_MaintenanceModeOn(bool isOn)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();

            bool isNextDelegateCalled = false;
            HttpContext ctx = Substitute.For<HttpContext>();
            RequestDelegate nextDelegate = (HttpContext hc) =>
            {
                isNextDelegateCalled = true;
                return Task.CompletedTask;
            };

            Action<MiddlewareOptionsBuilder> optionBuilderDelegate = (options) => { };

            IMaintenanceControlService svc = Substitute.For<IMaintenanceControlService>();
            svc.IsMaintenanceModeOn.Returns(isOn);

            MaintenanceMiddleware middleware = new MaintenanceMiddleware(
                nextDelegate,
                svc,

                null,
                optionBuilderDelegate);

            Action testAction = async ()
                => await middleware.Invoke(httpContext);


            testAction.ShouldNotThrow();
            isNextDelegateCalled.ShouldBe(!isOn);
        }

        [Theory]
        [InlineData("/somePath", "/somePath", StringComparison.Ordinal, 200)]
        [InlineData("/somePath", "/SOMEPath", StringComparison.OrdinalIgnoreCase, 200)]
        [InlineData("/somePath", "/SOMEPath", StringComparison.Ordinal, 503)]
        [InlineData("/somePath", "/someOtherPath", StringComparison.Ordinal, 503)]
        public async void BypassUrlPathOption(string requestPath,
            string bypassPath,
            StringComparison comparison,
            int expectedStatusCode)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.Request.Path = new PathString(requestPath);
                },
                (optionBuilder) =>
                {
                    optionBuilder.BypassUrlPath(bypassPath, comparison);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode.ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData("/somePath", new string[] { "/somePath", "/otherPath" }, StringComparison.Ordinal, 200)]
        [InlineData("/somePath", new string[] { "/SOMEPath", "/otherPath" }, StringComparison.OrdinalIgnoreCase, 200)]
        [InlineData("/somePath", new string[] { "/SOMEPath", "/otherPath" }, StringComparison.Ordinal, 503)]
        [InlineData("/somePath", new string[] { "/someOtherPath", "/stillOtherPath" }, StringComparison.Ordinal, 503)]
        public async void BypassUrlPathsOption(string requestPath, string[] bypassPaths, StringComparison comparison, int expectedStatusCode)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.Request.Path = new PathString(requestPath);
                },
                (optionBuilder) =>
                {
                    var bypassPathStrings = bypassPaths
                        .Select(s => new PathString(s));
                    optionBuilder.BypassUrlPaths(bypassPathStrings, comparison);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode.ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData(".txt", "/path/file.txt", 200)]
        [InlineData(".jpg", "/path/file.txt", 503)]
        public async void BypassFileExtensionOption(string extension, string requestPath, int expectedStatusCode)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.Request.Path = new PathString(requestPath);
                },
                (optionBuilder) =>
                {
                    optionBuilder.BypassFileExtension(extension);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode.ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData(new string[] {".txt", ".jpg" }, "/path/file.txt", 200)]
        [InlineData(new string[] { ".png", ".jpg" }, "/path/file.txt", 503)]
        public async void BypassFileExtensionsOption(string[] extensions, string requestPath, int expectedStatusCode)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.Request.Path = new PathString(requestPath);
                },
                (optionBuilder) =>
                {
                    optionBuilder.BypassFileExtensions(extensions);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode.ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData(true, 200)]
        [InlineData(false, 503)]
        public async void BypassAllAuthenticatedUsersOption(bool isUserAuthenticated, int expectedStatusCode)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.User = MockUser(null, null, isUserAuthenticated);
                },
                (optionBuilder) =>
                {
                    optionBuilder.BypassAllAuthenticatedUsers();
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode.ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData("testUser", "testUser", 200)]
        [InlineData("testUser", "otherUser", 503)]
        public async void BypassUserNameOption(string requestUser, string bypassUser, int expectedStatusCode)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.User = MockUser(requestUser, null, true);
                },
                (optionBuilder) =>
                {
                    optionBuilder.BypassUser(bypassUser);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode.ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData("firstUser", new string[] { "firstUser", "secondUser" }, 200)]
        [InlineData("secondUser", new string[] { "firstUser", "secondUser" }, 200)]
        [InlineData("otherUser", new string[] { "firstUser", "secondUser" }, 503)]
        public async void BypassUserNamesOption(string requestUser, string[] bypassUsers, int expectedStatusCode)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.User = MockUser(requestUser, null, true);
                },
                (optionBuilder) =>
                {
                    optionBuilder.BypassUsers(bypassUsers);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode.ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData("testRole", "testRole", 200)]
        [InlineData("testRole", "otherRole", 503)]
        public async void BypassUserRoleOption(string requestUserRole, string bypassUserRole, int expectedStatusCode)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.User = MockUser(null, requestUserRole, true);
                },
                (optionBuilder) =>
                {
                    optionBuilder.BypassUserRole(bypassUserRole);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode.ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData("firstRole", new string[] { "firstRole", "secondRole" }, 200)]
        [InlineData("secondRole", new string[] { "firstRole", "secondRole" }, 200)]
        [InlineData("otherRole", new string[] { "firstRole", "secondRole" }, 503)]
        public async void BypassUserRolesOption(string requestUserRole, string[] bypassUserRoles, int expectedStatusCode)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.User = MockUser(null, requestUserRole, true);
                },
                (optionBuilder) =>
                {
                    optionBuilder.BypassUserRoles(bypassUserRoles);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode.ShouldBe(expectedStatusCode);
        }

        private ClaimsPrincipal MockUser(string userName, string userRole, bool isAuthenticated)
        {
            var identity = Substitute.For<ClaimsIdentity>();
            identity.Name.Returns(userName);
            identity.IsAuthenticated.Returns(isAuthenticated);

            var claimsPrincipal = Substitute.For<ClaimsPrincipal>();
            claimsPrincipal.IsInRole(Arg.Is(userRole)).Returns(true);
            claimsPrincipal.Identity.Returns(identity);

            return claimsPrincipal;
        }

        private MiddlewareTestDesk GetTestDesk(
            Action<HttpContext> contextSetup,
            Action<MiddlewareOptionsBuilder> optionsSetup
            )
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            contextSetup(httpContext);

            bool isNextDelegateCalled = false;
            RequestDelegate nextDelegate = (HttpContext hc) =>
            {
                isNextDelegateCalled = true;
                return Task.CompletedTask;
            };

            IMaintenanceControlService svc = Substitute.For<IMaintenanceControlService>();
            svc.IsMaintenanceModeOn.Returns(true);

            IWebHostEnvironment webHostEnv = Substitute.For<IWebHostEnvironment>();
            webHostEnv.ContentRootPath.Returns(_tempDir);
            webHostEnv.WebRootPath.Returns(_tempDir);

            MaintenanceMiddleware middleware = new MaintenanceMiddleware(
                nextDelegate,
                svc,
                webHostEnv,
                optionsSetup);

            return new MiddlewareTestDesk
            {
                CurrentHttpContext = httpContext,
                IsNextDelegateCalled = isNextDelegateCalled,
                MiddlewareInstance = middleware
            };
        }
    }
}
