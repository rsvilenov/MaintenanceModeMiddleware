using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class PathRedirectOptionsBuilderTest : StatusCodeOptionsBuilderTestBase
    {
        public PathRedirectOptionsBuilderTest()
            : base(new PathRedirectOptionsBuilder())
        { }

        [Fact]
        public void WhenCalledWithPassReturnPathAsParameter_SetReturnUrlInUrlParameterShouldBeTrue()
        {
            PathRedirectOptionsBuilder builder = new PathRedirectOptionsBuilder();
            builder.PassReturnPathAsParameter();

            ReturnUrlData data = builder.GetReturnUrlData();

            data.SetReturnUrlInUrlParameter.ShouldBeTrue();
        }

        [Fact]
        public void WhenCalledWithPassReturnPathAsParameter_ReturnUrlParameterNameShouldBeDefault()
        {
            const string defaultReturnParameterName = "maintenanceReturnPath";
            PathRedirectOptionsBuilder builder = new PathRedirectOptionsBuilder();
            builder.PassReturnPathAsParameter();

            ReturnUrlData data = builder.GetReturnUrlData();

            data.ReturnUrlParameterName.ShouldBe(defaultReturnParameterName);
        }

        [Fact]
        public void WhenCalledWithPassReturnPathAsParameter_WithName_ReturnUrlParameterNameShouldBeAsExpected()
        {
            const string returnParameterName = "path123";
            PathRedirectOptionsBuilder builder = new PathRedirectOptionsBuilder();
            builder.PassReturnPathAsParameter(returnParameterName);

            ReturnUrlData data = builder.GetReturnUrlData();

            data.ReturnUrlParameterName.ShouldBe(returnParameterName);
        }

        [Fact]
        public void WhenCalledWithSetReturnPathInCookier_SetReturnUrlInCookieShouldBeTrue()
        {
            PathRedirectOptionsBuilder builder = new PathRedirectOptionsBuilder();
            builder.SetReturnPathInCookie();

            ReturnUrlData data = builder.GetReturnUrlData();

            data.SetReturnUrlInCookie.ShouldBeTrue();
        }


        [Fact]
        public void WhenCalledWithSetReturnPathInCookie_ReturnUrlCookiePrefixShouldBeDefault()
        {
            const string defaultReturnUrlCookiePrefix = "maintenanceReturnPath";
            PathRedirectOptionsBuilder builder = new PathRedirectOptionsBuilder();
            builder.SetReturnPathInCookie();

            ReturnUrlData data = builder.GetReturnUrlData();

            data.ReturnUrlCookiePrefix.ShouldBe(defaultReturnUrlCookiePrefix);
        }

        [Fact]
        public void WhenCalledWithSetReturnPathInCookie_WithPrefix_ReturnUrlCookiePrefixShouldBeAsExpected()
        {
            const string returnUrlCookiePrefix = "prefix123";
            PathRedirectOptionsBuilder builder = new PathRedirectOptionsBuilder();
            builder.SetReturnPathInCookie(returnUrlCookiePrefix);

            ReturnUrlData data = builder.GetReturnUrlData();

            data.ReturnUrlCookiePrefix.ShouldBe(returnUrlCookiePrefix);
        }

        [Fact]
        public void WhenCalledWithPassReturnPathAsCookie_WithoutCookieOptions_ReturnUrlCookieOptionsShouldBeNull()
        {
            PathRedirectOptionsBuilder builder = new PathRedirectOptionsBuilder();
            builder.SetReturnPathInCookie(cookiePrefix: "test");

            ReturnUrlData data = builder.GetReturnUrlData();

            data.ReturnUrlCookieOptions.ShouldBeNull();
        }

        [Fact]
        public void WhenCalledWithPassReturnPathAsCookie_WithCookieOptions_ReturnUrlCookieOptionsShouldNotBeNull()
        {
            PathRedirectOptionsBuilder builder = new PathRedirectOptionsBuilder();
            builder.SetReturnPathInCookie(cookiePrefix: "test", cookieOptions: new CookieOptions());

            ReturnUrlData data = builder.GetReturnUrlData();

            data.ReturnUrlCookieOptions.ShouldNotBeNull();
        }

        [Fact]
        public void WhenCalledWithSetCustomReturnPath_CustomReturnPathShouldBeAsExpected()
        {
            PathString customReturnPath = "/test/path";
            PathRedirectOptionsBuilder builder = new PathRedirectOptionsBuilder();
            builder.SetCustomReturnPath(customReturnPath);

            ReturnUrlData data = builder.GetReturnUrlData();

            data.CustomReturnPath.ShouldBe(customReturnPath);
        }
    }
}
