using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.State;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
        public async void Invoke_WithBypassUrlPath_WhenMatchStatusCodeShouldBeAppropriate(string requestPath,
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
        public async void Invoke_WithBypassUrlPaths_WhenMatchStatusCodeShouldBeAppropriate(string requestPath, string[] bypassPaths, StringComparison comparison, int expectedStatusCode)
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
        public async void Invoke_WithBypassUrlPathAndSvcOverride_WhenMatchStatusCodeShouldBeAppropriate(string requestPath,
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
                    // the method EnterMaintenance() of MaintenanceControlService
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
        public async void Invoke_WithBypassFileExtension_WhenMatchStatusCodeShouldBeAppropriate(string extension, string requestPath, int expectedStatusCode)
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
        public async void Invoke_WithBypassFileExtensions_WhenMatchStatusCodeShouldBeAppropriate(string[] extensions, string requestPath, int expectedStatusCode)
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
        public async void Invoke_WithBypassAllAuthenticatedUsers_WhenMatchStatusCodeShouldBeAppropriate(bool isUserAuthenticated, int expectedStatusCode)
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
        public async void Invoke_WithBypassUser_WhenMatchStatusCodeShouldBeAppropriate(string requestUser, string bypassUser, int expectedStatusCode)
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
        public async void Invoke_WithBypassUsers_StatusCodeShouldBeAppropriate(string requestUser, string[] bypassUsers, int expectedStatusCode)
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
        public async void Invoke_WithBypassUserRole_StatusCodeShouldBeAppropriate(string requestUserRole, string bypassUserRole, int expectedStatusCode)
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
        public async void Invoke_WithBypassUserRoles_StatusCodeShouldBeAppropriate(string requestUserRole, string[] bypassUserRoles, int expectedStatusCode)
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
        public async void Invoke_WithUseDefaultResponse_ResponseShouldBeAppropriate()
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
                .ShouldContain(h => h.Key == "Retry-After");
            desk.CurrentHttpContext.Response.ContentType
                .ShouldBe("text/html");
            GetResponseString(desk.CurrentHttpContext)
                .ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async void Invoke_WithUseResponse_ResponseShouldBeAppropriate()
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UseResponse(Encoding.UTF8.GetBytes("test"), ResponseContentType.Text, Encoding.UTF8);
                });


            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(503);
            desk.CurrentHttpContext.Response.Headers
                .ShouldContain(h => h.Key == "Retry-After");
            desk.CurrentHttpContext.Response.ContentType
                .ShouldBe("text/plain");
            GetResponseString(desk.CurrentHttpContext)
                .ShouldBe("test");
        }

        [Theory]
        [InlineData("test.txt", "text/plain", EnvDirectory.ContentRootPath)]
        [InlineData("test.html", "text/html", EnvDirectory.ContentRootPath)]
        [InlineData("test.json", "application/json", EnvDirectory.ContentRootPath)]
        [InlineData("test.txt", "text/plain", EnvDirectory.WebRootPath)]
        public async void Invoke_WithUseResponseFile_ResponseShouldBeAppropriate(string fileName, string expectedContentType, EnvDirectory baseDir)
        {
            string tempDir = Path.GetTempPath();
            string rootDir;
            if (baseDir == EnvDirectory.ContentRootPath)
            {
                rootDir = Path.Combine(tempDir, "contentRoot");
            }
            else
            {
                rootDir = Path.Combine(tempDir, "wwwRoot");
            }
            string safeTempFileName = SafeTempPath.Create(fileName);
            File.WriteAllText(Path.Combine(rootDir, safeTempFileName), "test");

            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UseResponseFromFile(safeTempFileName, baseDir);
                },
                null,
                tempDir);

            
            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext); 

            
            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(503);
            desk.CurrentHttpContext.Response.Headers
                .ShouldContain(h => h.Key == "Retry-After");
            desk.CurrentHttpContext.Response.ContentType
                .ShouldBe(expectedContentType);
            GetResponseString(desk.CurrentHttpContext)
                .ShouldBe("test");
        }

        [Fact]
        public async void Invoke_WithUseRedirect_ResponseShouldBeAppropriate()
        {
            string testUriPath = "http://test.com/test";
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UseRedirect(testUriPath);
                });


            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);


            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(302);
            desk.CurrentHttpContext.Response.Headers
                .ShouldContain(h => h.Key == "Location");
            desk.CurrentHttpContext.Response.Headers["Location"]
                .ToString()
                .ShouldBe(testUriPath);
        }

        [Fact]
        public async void Invoke_WithUsePathRedirect_RedirectResponseShouldBeAppropriate()
        {
            string testUriPath = "/test";
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UsePathRedirect(testUriPath);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(302);
            desk.CurrentHttpContext.Response.Headers
                .ShouldContain(h => h.Key == "Location");
            desk.CurrentHttpContext.Response.Headers["Location"]
                .ToString()
                .ShouldBe(testUriPath);
        }

        [Fact]
        public async void Invoke_WithUsePathRedirect_ResponseAfterRedirectShouldBeAppropriate()
        {
            const string testUriPath = "/test";
            const int defaultStatusCode = 200;
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UsePathRedirect(testUriPath);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Request.Path = testUriPath;
            desk.CurrentHttpContext.Response.StatusCode = defaultStatusCode;

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(503);
            desk.CurrentHttpContext.Request.Path.ToString()
                .ShouldBe(testUriPath);
            desk.CurrentHttpContext.Response.Headers
                .ShouldContain(h => h.Key == "Retry-After");
            desk.CurrentHttpContext.Response.Headers["Retry-After"]
                .ToString()
                .ShouldBe(DefaultValues.DEFAULT_503_RETRY_INTERVAL.ToString());
        }

        [Fact]
        public async void Invoke_WithUsePathRedirect_WithPreserveStatusCode_ResponseStatusCodeShouldBeDefault()
        {
            const string testUriPath = "/test";
            const int defaultStatusCode = 200;
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UsePathRedirect(testUriPath, 
                        redirectOptions => redirectOptions.PreserveStatusCode());
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Request.Path = testUriPath;
            desk.CurrentHttpContext.Response.StatusCode = defaultStatusCode;

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(defaultStatusCode);
            desk.CurrentHttpContext.Response.Headers
                .ShouldNotContain(h => h.Key == "Retry-After");
        }

        [Fact]
        public async void Invoke_WithUsePathRedirect_WhenPathDoesNotMatchRedirectLocation_ShouldRedirect()
        {
            const string testUriPath = "/test";
            const int defaultStatusCode = 200;
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UsePathRedirect(testUriPath);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Request.Path = "/";
            desk.CurrentHttpContext.Response.StatusCode = defaultStatusCode;

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(302);
            desk.CurrentHttpContext.Response.Headers
                .ShouldContainKey("Location"); 
            desk.CurrentHttpContext.Response.Headers["Location"].ToString()
                 .ShouldBe(testUriPath);
        }
        
        [Fact]
        public async void Invoke_WithUsePathRedirect_SetReturnUrlInCookie_ShouldSetReturnUrlInCookie()
        {
            const string testUriPath = "/test";
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.Request.Path = "/returnPath";
                },
                (optionBuilder) =>
                {
                    optionBuilder.UsePathRedirect(testUriPath, 
                        redirectOptions => redirectOptions.SetReturnPathInCookie());
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.Headers
                .ShouldContainKey("Set-Cookie");
            desk.CurrentHttpContext.Response.Headers["Set-Cookie"]
                .ShouldContain(cookie => cookie.Contains("%2FreturnPath"));
        }

        [Fact]
        public async void Invoke_WithUsePathRedirect_WithNoCookieOption_ShouldNotSetCookie()
        {
            const string testUriPath = "/test";
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.Request.Path = "/returnPath";
                },
                (optionBuilder) =>
                {
                    optionBuilder.UsePathRedirect(testUriPath);
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.Headers
                .ShouldNotContainKey("Set-Cookie");
        }

        [Fact]
        public async void Invoke_WithUsePathRedirect_WithCustomCookieOptions_ShouldNotSetCookieOptins()
        {
            const string testUriPath = "/test";
            string customCookieDomain = "mytestdomain.com";
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.Request.Path = "/returnPath";
                },
                (optionBuilder) =>
                {
                    var cookieOptions = new CookieOptions
                    {
                        Domain = customCookieDomain
                    };

                    optionBuilder.UsePathRedirect(testUriPath, 
                        redirectOptions => redirectOptions.SetReturnPathInCookie(cookieOptions: cookieOptions));
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.Headers
                .ShouldContainKey("Set-Cookie");
            desk.CurrentHttpContext.Response.Headers["Set-Cookie"]
                .ShouldContain(cookie => cookie.Contains($"domain={customCookieDomain}"));
        }

        [Fact]
        public async void Invoke_WithUsePathRedirect_WithPassReturnPathAsParameter_ShouldPassReurnPath()
        {
            const string testUriPath = "/test";
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.Request.Path = "/returnPath";
                },
                (optionBuilder) =>
                {
                    optionBuilder.UsePathRedirect(testUriPath,
                        redirectOptions => redirectOptions.PassReturnPathAsParameter());
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.Headers
                .ShouldContainKey("Location");
            desk.CurrentHttpContext.Response.Headers["Location"]
                .ToString()
                .ShouldEndWith("maintenanceReturnPath=/returnPath");
        }

        [Fact]
        public async void Invoke_WithUsePathRedirect_WithCustomReturnPath_ShouldSetCustomReurnPath()
        {
            const string testUriPath = "/test";
            const string customReturnPath = "/testCustomReturnPath";
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.Request.Path = "/returnPath";
                },
                (optionBuilder) =>
                {
                    optionBuilder.UsePathRedirect(testUriPath,
                        redirectOptions => redirectOptions
                            .PassReturnPathAsParameter()
                            .SetReturnPathInCookie()
                            .SetCustomReturnPath(customReturnPath));
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.Headers
                .ShouldContainKey("Location");
            desk.CurrentHttpContext.Response.Headers["Location"]
                .ToString()
                .ShouldEndWith($"maintenanceReturnPath={customReturnPath}");

            string encodedPath = $"%2F{customReturnPath.TrimStart('/')}";
            desk.CurrentHttpContext.Response.Headers
                .ShouldContainKey("Set-Cookie");
            desk.CurrentHttpContext.Response.Headers["Set-Cookie"]
                .ShouldContain(cookie => cookie.Contains(encodedPath));
        }

        [Fact]
        public async void Invoke_WithUsePathRedirect_WithCustomReturnPathParameterName_ShouldSetCustoParameterName()
        {
            const string testUriPath = "/test";
            const string customReturnPathParameterName = "myReturnPath";
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                    httpContext.Request.Path = "/returnPath";
                },
                (optionBuilder) =>
                {
                    optionBuilder.UsePathRedirect(testUriPath,
                        redirectOptions => redirectOptions
                            .PassReturnPathAsParameter(customReturnPathParameterName)
                            .SetReturnPathInCookie(customReturnPathParameterName));
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.Headers
                .ShouldContainKey("Location");
            desk.CurrentHttpContext.Response.Headers["Location"]
                .ToString()
                .ShouldEndWith($"{customReturnPathParameterName}=/returnPath");

            string encodedPath = $"%2FreturnPath";
            desk.CurrentHttpContext.Response.Headers
                .ShouldContainKey("Set-Cookie");
            desk.CurrentHttpContext.Response.Headers["Set-Cookie"]
                .ShouldContain(cookie => cookie.Contains(customReturnPathParameterName)
                    && cookie.Contains(encodedPath));
        }

        [Fact]
        public async void Invoke_WithUsePathRedirect_And_PreserveStatusCode_ResponseShouldBeAppropriate()
        {
            const string testUriPath = "/test";
            const int defaultStatusCode = 200;
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UsePathRedirect(testUriPath, redirectOptionsBuilder =>
                        redirectOptionsBuilder.PreserveStatusCode());
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Request.Path = testUriPath;
            desk.CurrentHttpContext.Response.StatusCode = defaultStatusCode;

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(defaultStatusCode);
            desk.CurrentHttpContext.Request.Path.ToString()
                .ShouldBe(testUriPath);
            desk.CurrentHttpContext.Response.Headers
                .ShouldNotContain(h => h.Key == "Retry-After");
        }

        [Fact]
        public async void Invoke_WithUsePathRedirect_And_Use503CodeRetryInterval_ResponseShouldBeAppropriate()
        {
            const int retryInterval = 1234;
            const string testUriPath = "/test";
            const int defaultStatusCode = 200;
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UsePathRedirect(testUriPath, redirectOptionsBuilder =>
                        redirectOptionsBuilder.Use503CodeRetryInterval(retryInterval));
                });

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Request.Path = testUriPath;
            desk.CurrentHttpContext.Response.StatusCode = defaultStatusCode;

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            desk.CurrentHttpContext.Response.StatusCode
                .ShouldBe(503);
            desk.CurrentHttpContext.Request.Path.ToString()
                .ShouldBe(testUriPath);
            desk.CurrentHttpContext.Response.Headers
                .ShouldContain(h => h.Key == "Retry-After");
            desk.CurrentHttpContext.Response.Headers["Retry-After"]
                .ToString()
                .ShouldBe(retryInterval.ToString());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void Invoke_WithUseControllerAction_ControllerAndActionNamesShouldBeSet(bool isRouteDataPrefilled)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UseControllerAction<TestController>(nameof(TestController.Get));
                });

            if (isRouteDataPrefilled)
            {
                var preMiddlewareRouteData = desk.CurrentHttpContext.GetRouteData();
                preMiddlewareRouteData.Values.Add("controller", "someController");
                preMiddlewareRouteData.Values.Add("action", "someAction");
            }

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            var routeData = desk.CurrentHttpContext.GetRouteData();
            routeData.Values.Keys.ShouldContain(k => k == "controller");
            routeData.Values.Keys.ShouldContain(k => k == "action");
            routeData.Values.Keys.ShouldNotContain(k => k == "area");
            routeData.Values["Controller"].ShouldBe(nameof(TestController).Replace("Controller", ""));
            routeData.Values["Action"].ShouldBe(nameof(TestController.Get));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void Invoke_WithUseControllerAction_WithControllerWithArea_ControllerAndActionAndAreaNamesShouldBeSet(bool isRouteDataPrefilled)
        {
            MiddlewareTestDesk desk = GetTestDesk(
                (httpContext) =>
                {
                },
                (optionBuilder) =>
                {
                    optionBuilder.UseControllerAction<TestControllerWithAreaAttribute>(nameof(TestController.Get));
                });

            if (isRouteDataPrefilled)
            {
                var preMiddlewareRouteData = desk.CurrentHttpContext.GetRouteData();
                preMiddlewareRouteData.Values.Add("controller", "someController");
                preMiddlewareRouteData.Values.Add("action", "someAction");
                preMiddlewareRouteData.Values.Add("area", "someArea");
            }

            await desk.MiddlewareInstance
                .Invoke(desk.CurrentHttpContext);

            var routeData = desk.CurrentHttpContext.GetRouteData();
            routeData.Values.Keys.ShouldContain(k => k == "controller");
            routeData.Values.Keys.ShouldContain(k => k == "action");
            routeData.Values.Keys.ShouldContain(k => k == "area");
            routeData.Values["controller"].ShouldBe(nameof(TestControllerWithAreaAttribute));
            routeData.Values["action"].ShouldBe(nameof(TestControllerWithAreaAttribute.Get));
            routeData.Values["area"].ShouldBe(TestControllerWithAreaAttribute.AreaName);
        }

        [Fact]
        public void Invoke_WhenMaintenanceModeIsOn_ShouldNotThrow()
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();

            IMaintenanceControlService controlSvc = Substitute.For<IMaintenanceControlService>();
            IMaintenanceOptionsService optionsSvc = Substitute.For<IMaintenanceOptionsService>();
            IDirectoryMapperService dirMapperSvc = Substitute.For<IDirectoryMapperService>();
            controlSvc.GetState().Returns(new MaintenanceState
            {
                IsMaintenanceOn = true
            });
            optionsSvc.GetOptions().Returns(new MiddlewareOptionsBuilder(dirMapperSvc).GetOptions());

            MaintenanceMiddleware middleware = new MaintenanceMiddleware(
                (hc) =>
                {
                    return Task.CompletedTask;
                },
                controlSvc,
                optionsSvc);


            Action testAction = async ()
                => await middleware.Invoke(httpContext);


            testAction.ShouldNotThrow();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void Invoke_WithMaintenanceStateSet_NextDelegateShouldBeCalledOnlyWhenMaintenanceIsOn(bool isOn)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            bool isNextDelegateCalled = false;

            IMaintenanceControlService controlSvc = Substitute.For<IMaintenanceControlService>();
            IMaintenanceOptionsService optionsSvc = Substitute.For<IMaintenanceOptionsService>();
            IDirectoryMapperService dirMapperSvc = Substitute.For<IDirectoryMapperService>();
            controlSvc.GetState().Returns(new MaintenanceState
            {
                IsMaintenanceOn = isOn
            });
            optionsSvc.GetOptions().Returns(new MiddlewareOptionsBuilder(dirMapperSvc).GetOptions());

            MaintenanceMiddleware middleware = new MaintenanceMiddleware(
                (hc) =>
                {
                    isNextDelegateCalled = true;
                    return Task.CompletedTask;
                },
                controlSvc,
                optionsSvc);


            await middleware.Invoke(httpContext);

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
            Action<IMiddlewareOptionsBuilder> optionsSetup,
            Action<IMiddlewareOptionsBuilder> optionsOverrideSetup = null,
            string tempDir = null)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            contextSetup(httpContext);

            bool isNextDelegateCalled = false;
            RequestDelegate nextDelegate = async (HttpContext hc) =>
            {
                isNextDelegateCalled = true;
                await Task.CompletedTask;
            };

            if (tempDir == null)
            {
                tempDir = Path.GetTempPath();
            }

            IDirectoryMapperService dirMapperSvc = FakeDirectoryMapperService.Create(tempDir);
            
            MiddlewareOptionsBuilder optionBuilder = new MiddlewareOptionsBuilder(dirMapperSvc);
            optionsSetup.Invoke(optionBuilder);
            OptionCollection middlewareOptions = optionBuilder.GetOptions();
            
            if (optionsOverrideSetup != null)
            {
                MiddlewareOptionsBuilder optionOverrideBuilder = new MiddlewareOptionsBuilder(dirMapperSvc);
                optionsOverrideSetup.Invoke(optionOverrideBuilder);
                middlewareOptions = optionOverrideBuilder.GetOptions();
            }

            IMaintenanceControlService ctrlSvc = Substitute.For<IMaintenanceControlService>();
            IMaintenanceOptionsService optionsSvc = new MaintenanceOptionsService();
            optionsSvc.SetStartupOptions(middlewareOptions);
            ctrlSvc.GetState().Returns(new MaintenanceState(null, isMaintenanceOn: true, middlewareOptions));


            MaintenanceMiddleware middleware = new MaintenanceMiddleware(
                nextDelegate, ctrlSvc, optionsSvc);

            return new MiddlewareTestDesk
            {
                CurrentHttpContext = httpContext,
                IsNextDelegateCalled = isNextDelegateCalled,
                MiddlewareInstance = middleware
            };
        }
    }
}
