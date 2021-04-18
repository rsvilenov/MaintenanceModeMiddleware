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
        [InlineData(ContentType.Html, "text/html")]
        [InlineData(ContentType.Text, "text/plain")]
        [InlineData(ContentType.Json, "application/json")]
        public void GetContentTypeString_WithValidEnumValue_ShouldReturnValidContentType(ContentType contentType, string contentTypeString)
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
            ContentType invalidEnumValue = (ContentType)(-1);
            MaintenanceResponse response = new MaintenanceResponse
            {
                ContentType = invalidEnumValue
            };

            Func<string> testFunc = () => response.GetContentTypeString();

            testFunc.ShouldThrow(typeof(InvalidOperationException));
        }
    }
}
