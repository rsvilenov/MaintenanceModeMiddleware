using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration.Data
{
    public class MaintenanceResponseTest
    {
        [Theory]
        [InlineData(ResponseContentType.Html, "text/html")]
        [InlineData(ResponseContentType.Text, "text/plain")]
        [InlineData(ResponseContentType.Json, "application/json")]
        public void GetContentTypeString_WithValidEnumValue_ShouldReturnValidContentType(ResponseContentType contentType, string contentTypeString)
        {
            MaintenanceResponse response = new MaintenanceResponse
            {
                ContentType = contentType
            };

            string returned = response.GetContentTypeString();

            returned
                .ShouldBe(contentTypeString);
        }

        [Fact]
        public void GetContentTypeString_WithInalidEnumValue_ShouldThrow()
        {
            ResponseContentType invalidEnumValue = (ResponseContentType)(-1);
            MaintenanceResponse response = new MaintenanceResponse
            {
                ContentType = invalidEnumValue
            };

            Func<string> testFunc = () => response.GetContentTypeString();

            testFunc.ShouldThrow(typeof(InvalidOperationException));
        }
    }
}
