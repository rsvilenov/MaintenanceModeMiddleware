using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MaintenanceModeMiddleware.Tests
{
    public class MaintenanceMiddlewareInvocationTest
    {
        [Theory]
        [InlineData("/somePath", "/somePath", StringComparison.Ordinal, 200)]
        [InlineData("/somePath", "/SOMEPath", StringComparison.OrdinalIgnoreCase, 200)]
        [InlineData("/somePath", "/SOMEPath", StringComparison.Ordinal, 503)]
        [InlineData("/somePath", "/someOtherPath", StringComparison.Ordinal, 503)]
        public async void Invoke_With_BypassUrlPath(string requestPath,
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


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData("/somePath", new string[] { "/somePath", "/otherPath" }, StringComparison.Ordinal, 200)]
        [InlineData("/somePath", new string[] { "/SOMEPath", "/otherPath" }, StringComparison.OrdinalIgnoreCase, 200)]
        [InlineData("/somePath", new string[] { "/SOMEPath", "/otherPath" }, StringComparison.Ordinal, 503)]
        [InlineData("/somePath", new string[] { "/someOtherPath", "/stillOtherPath" }, StringComparison.Ordinal, 503)]
        public async void Invoke_With_BypassUrlPaths(string requestPath, string[] bypassPaths, StringComparison comparison, int expectedStatusCode)
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


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData("/somePath", "/someOverriden", "/somePath", 200)]
        [InlineData("/somePath", "/someOverriden", "/someOverriden", 503)]
        public async void Invoke_With_BypassUrlPath_SvcOverride(string requestPath,
            string bypassPath,
            string overridenPath,
            int expectedStatusCode)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.Request.Path = new PathString(requestPath);
                },
                (optionBuilder) =>
                {
                    optionBuilder.BypassUrlPath(bypassPath);
                },
                (optionsToOverrideFromSvc) =>
                {
                    // this is an option, passed as a parameter to
                    // the method EnterMaintanence() of MaintenanceControlService
                    optionsToOverrideFromSvc.BypassUrlPath(overridenPath);
                });


            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData(".txt", "/path/file.txt", 200)]
        [InlineData(".jpg", "/path/file.txt", 503)]
        public async void Invoke_With_BypassFileExtension(string extension, string requestPath, int expectedStatusCode)
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


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData(new string[] {".txt", ".jpg" }, "/path/file.txt", 200)]
        [InlineData(new string[] { ".png", ".jpg" }, "/path/file.txt", 503)]
        public async void Invoke_With_BypassFileExtensions(string[] extensions, string requestPath, int expectedStatusCode)
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


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData(true, 200)]
        [InlineData(false, 503)]
        public async void Invoke_With_BypassAllAuthenticatedUsers(bool isUserAuthenticated, int expectedStatusCode)
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


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData("testUser", "testUser", 200)]
        [InlineData("testUser", "otherUser", 503)]
        public async void Invoke_With_BypassUser(string requestUser, string bypassUser, int expectedStatusCode)
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


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData("firstUser", new string[] { "firstUser", "secondUser" }, 200)]
        [InlineData("secondUser", new string[] { "firstUser", "secondUser" }, 200)]
        [InlineData("otherUser", new string[] { "firstUser", "secondUser" }, 503)]
        public async void Invoke_With_BypassUsers(string requestUser, string[] bypassUsers, int expectedStatusCode)
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


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData("testRole", "testRole", 200)]
        [InlineData("testRole", "otherRole", 503)]
        public async void Invoke_With_BypassUserRole(string requestUserRole, string bypassUserRole, int expectedStatusCode)
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


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(expectedStatusCode);
        }

        [Theory]
        [InlineData("firstRole", new string[] { "firstRole", "secondRole" }, 200)]
        [InlineData("secondRole", new string[] { "firstRole", "secondRole" }, 200)]
        [InlineData("otherRole", new string[] { "firstRole", "secondRole" }, 503)]
        public async void Invoke_With_BypassUserRoles(string requestUserRole, string[] bypassUserRoles, int expectedStatusCode)
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


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(expectedStatusCode);
        }

        [Fact]
        public async void Invoke_With_UseDefaultResponse()
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    // UseDefaultResponseOption() is automatically set if no response option is specified
                    // and UseNoDefaultValues() is not called.
                });


            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(503);
            desk.CurrentHttpContext.Response.Headers
                .Any(h => h.Key == "Retry-After").ShouldBeTrue();
            desk.CurrentHttpContext.Response.ContentType
                .ShouldBe("text/html");
            GetResponseString(desk.CurrentHttpContext)
                .ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async void Invoke_With_UseResponse()
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UseResponse(Encoding.UTF8.GetBytes("test"), ContentType.Text, Encoding.UTF8);
                });


            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(503);
            desk.CurrentHttpContext.Response.Headers
                .Any(h => h.Key == "Retry-After")
                .ShouldBeTrue();
            desk.CurrentHttpContext.Response.ContentType
                .ShouldBe("text/plain");
            GetResponseString(desk.CurrentHttpContext)
                .ShouldBe("test");
        }

        [Fact]
        public async void Invoke_With_UseResponseFile()
        {
            string tempDir = Path.GetTempPath();
            File.WriteAllText(Path.Combine(tempDir, "test.txt"), "test");

            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UseResponseFile("test.txt", PathBaseDirectory.ContentRootPath);
                },
                null,
                tempDir);
            

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(503);
            desk.CurrentHttpContext.Response.Headers
                .Any(h => h.Key == "Retry-After")
                .ShouldBeTrue();
            desk.CurrentHttpContext.Response.ContentType
                .ShouldBe("text/plain");
            GetResponseString(desk.CurrentHttpContext)
                .ShouldBe("test");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Invoke_MaintenanceModeOn(bool isOn)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();

            IMaintenanceControlService svc = Substitute.For<IMaintenanceControlService>();
            svc.IsMaintenanceModeOn.Returns(isOn);

            bool isNextDelegateCalled = false;

            MaintenanceMiddleware middleware = new MaintenanceMiddleware(
                (hc) =>
                {
                    isNextDelegateCalled = true;
                    return Task.CompletedTask;
                },
                svc,
                null,
                (options) => { });


            Action testAction = async ()
                => await middleware.Invoke(httpContext);


            testAction.ShouldNotThrow();
            isNextDelegateCalled.ShouldBe(!isOn);
        }


        private string GetResponseString(HttpContext context)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                context.Response.Body.Position = 0;
                context.Response.Body.CopyTo(ms);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
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
            Action<MiddlewareOptionsBuilder> optionsSetup,
            Action<MiddlewareOptionsBuilder> optionsOverrideSetup = null,
            string tempDir = null)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            contextSetup(httpContext);

            bool isNextDelegateCalled = false;
            RequestDelegate nextDelegate = (HttpContext hc) =>
            {
                isNextDelegateCalled = true;
                return Task.CompletedTask;
            };

            IMaintenanceControlService svc = Substitute.For<IMaintenanceControlService, ICanOverrideMiddlewareOptions>();
            svc.IsMaintenanceModeOn.Returns(true);

            if (optionsOverrideSetup != null)
            {
                MiddlewareOptionsBuilder optionOverrideBuilder = BuildOptionsToOverride(optionsOverrideSetup);

                (svc as ICanOverrideMiddlewareOptions)
                    .GetOptionsToOverride()
                    .Returns(optionOverrideBuilder.Options);
            }

            if (tempDir == null)
            {
                tempDir = Path.GetTempPath();
            }

            IWebHostEnvironment webHostEnv = Substitute.For<IWebHostEnvironment>();
            webHostEnv.ContentRootPath.Returns(tempDir);
            webHostEnv.WebRootPath.Returns(tempDir);

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

        private MiddlewareOptionsBuilder BuildOptionsToOverride(Action<MiddlewareOptionsBuilder> optionsOverrideSetup)
        {
            MiddlewareOptionsBuilder optionOverrideBuilder = new MiddlewareOptionsBuilder();
            optionsOverrideSetup.Invoke(optionOverrideBuilder);

            if (optionOverrideBuilder.Options
                .GetSingleOrDefault<UseNoDefaultValuesOption>()
                ?.Value != true)
            {
                optionOverrideBuilder.FillEmptyOptionsWithDefault();
            }

            return optionOverrideBuilder;
        }
    }
}
