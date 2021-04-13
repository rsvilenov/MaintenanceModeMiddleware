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
        [InlineData(ContentType.Html, "text/html", null)]
        [InlineData(ContentType.Text, "text/plain", null)]
        [InlineData(ContentType.Json, "application/json", null)]
        [InlineData((ContentType)(-1), "application/json", typeof(InvalidOperationException))]
        public void GetContentTypeString(ContentType contentType, string contentTypeString, Type expectedExceptionType)
        {
            MaintenanceResponse response = new MaintenanceResponse
            {
                ContentType = contentType
            };

            Func<string> testFunc = () => response.GetContentTypeString();

            if (expectedExceptionType == null)
            {
                testFunc.ShouldNotThrow()
                    .ShouldBe(contentTypeString);
            }
            else
            {
                testFunc.ShouldThrow(expectedExceptionType);
            }
        }
    }
}
