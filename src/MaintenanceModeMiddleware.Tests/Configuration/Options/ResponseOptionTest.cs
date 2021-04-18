using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using System.Text;
using Xunit;


namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class ResponseOptionTest
    {

        [Fact]
        public void LoadFromString_WithValidInput_ValueShouldNotBeNull()
        {
            const string input = "Text;65001;5300;maintenance mode";
            var option = new ResponseOption();
            
            option.LoadFromString(input);

            option.Value
                .ShouldNotBeNull();
        }

        [Fact]
        public void LoadFromString_WithValidInput_StringValueShouldEqualInput()
        {
            const string input = "Text;65001;5300;maintenance mode";
            var option = new ResponseOption();
            
            option.LoadFromString(input);

            option.GetStringValue()
                .ShouldBe(input);
        }

        [Theory]
        [InlineData("Html;65002;5300;maintenance mode", typeof(NotSupportedException))] /* there is no encoding with code page 65002 */
        [InlineData("Music;65001;5300;maintenance mode", typeof(ArgumentException))] /* music is not a valid content type */
        [InlineData("Text;10v10s54;5300;maintenance mode", typeof(FormatException))] /* 10v10s54 is not a valid integer */
        [InlineData("Text;101054;5300;maintenance mode", typeof(ArgumentOutOfRangeException))] /* 101054 is not a valid code page */
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("string in wrong format", typeof(FormatException))]
        public void LoadFromString_WithInvalidInput_ShouldThrow(string input, Type expectedException)
        {
            var option = new ResponseOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldThrow(expectedException);
        }

        [Theory]
        [InlineData("Text;65001;5300;maintenance mode", ContentType.Text)]
        [InlineData("Json;65001;5300;maintenance mode", ContentType.Json)]
        [InlineData("Html;65001;5300;<html><head></head><body>maintenance mode</body></html>", ContentType.Html)]
        public void LoadFromString_WithVariousContentTypes_ContentTypeInValueShouldMatchInput(string input, ContentType contentType)
        {
            var option = new ResponseOption();
            
            option.LoadFromString(input);

            option.Value.ContentType
                .ShouldBe(contentType);
        }

        [Theory]
        [InlineData("Text;65001;5300;maintenance mode", 65001)]
        [InlineData("Text;20127;5300;maintenance mode", 20127)]
        public void LoadFromString_WithDifferentEncodings_ValueEncodingShouldMatchInput(string input, int codePage)
        {
            var option = new ResponseOption();
            
            option.LoadFromString(input);

            Encoding encoding = Encoding.GetEncoding(codePage);
            option.Value.ContentEncoding
                .ShouldBe(encoding);
        }

        [Theory]
        [InlineData("Text;65001;5300;maintenance mode", 65001, "maintenance mode")]
        [InlineData("Text;65001;5300;currently in maintenance mode", 65001, "currently in maintenance mode")]
        public void LoadFromString_WithVariousContent_ValueContentShouldMatchInput(string input, int codePage, string content)
        {
            var option = new ResponseOption();

            option.LoadFromString(input);

            Encoding encoding = Encoding.GetEncoding(codePage);
            option.Value.ContentBytes
                .ShouldBe(encoding.GetBytes(content));
        }

        [Fact]
        public void LoadFromString_WithValidRetryAfter_ValueRetryAfterShouldEqualInput()
        {
            const string input = "Text;65001;5300;maintenance mode";
            const int expectedValue = 5300;
            var option = new ResponseOption();
            
            option.LoadFromString(input);

            option.Value.Code503RetryInterval
                    .ShouldBe(expectedValue);
        }

        [Fact]
        public void LoadFromString_WithInvalidRetryAfter_ValueRetryAfterShouldEqualInput()
        {
            const string input = "Text;65001;abc;maintenance mode";
            var option = new ResponseOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldThrow<ArgumentException>();
        }
    }
}
