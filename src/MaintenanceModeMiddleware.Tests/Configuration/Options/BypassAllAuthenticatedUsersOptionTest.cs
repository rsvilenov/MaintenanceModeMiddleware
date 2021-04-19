using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using Xunit;


namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class BypassAllAuthenticatedUsersOptionTest
    {
        [Theory]
        [InlineData("True", true)]
        [InlineData("False", false)]
        public void LoadFromString_WithValidInput_ValueShouldMatchInput(string inputStr, bool expectedValue)
        {
            var option = new BypassAllAuthenticatedUsersOption();
            
            option.LoadFromString(inputStr);

            option.Value
                .ShouldBe(expectedValue);
        }

        [Theory]
        [InlineData("True")]
        [InlineData("False")]
        public void LoadFromString_WithValidInput_StringValueShouldMatchInput(string inputStr)
        {
            var option = new BypassAllAuthenticatedUsersOption();
            
            option.LoadFromString(inputStr);

            option.GetStringValue()
                .ShouldBe(inputStr);
        }

        [Theory]
        [InlineData("Invalid", typeof(FormatException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void LoadFromString_WithinvalidOrNullInput_ShouldThrow(string inputStr, Type expectedExceptionType)
        {
            var option = new BypassAllAuthenticatedUsersOption();
            Action testAction = () => option.LoadFromString(inputStr);

            testAction.ShouldThrow(expectedExceptionType);
        }
    }
}
