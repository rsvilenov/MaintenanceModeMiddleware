using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using Xunit;


namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class BypassFileExtensionOptionTest
    {
        [Fact]
        public void LoadFromString_WithValidFilePath_ValueShouldMatchInput()
        {
            string input = "/test/file.txt";
            var option = new BypassFileExtensionOption();
            
            option.LoadFromString(input);

            option.Value
                .ShouldBe(input);
        }

        [Fact]
        public void LoadFromString_WithValidFilePath_StringValueShouldMatchInput()
        {
            string input = "/test/file.txt";
            var option = new BypassFileExtensionOption();
            
            option.LoadFromString(input);

            option.GetStringValue()
                .ShouldBe(input);
        }

        [Fact]
        public void LoadFromString_WithNullStringInput_ShouldThrowArgumentNullException()
        {
            string input = null;
            var option = new BypassFileExtensionOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldThrow<ArgumentNullException>();
        }
    }
}
