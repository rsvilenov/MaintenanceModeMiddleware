using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class RedirectOptionTest
    {
        [Fact]
        public void LoadFromString_WithValidInput_ValueShouldEqualInput()
        {
            string str = "http://test.com/test";
            var option = new RedirectOption();

            option.LoadFromString(str);

            option.Value.ShouldBe(str);
        }

        [Fact]
        public void LoadFromString_WithValidInput_StringValueShouldEqualInput()
        {
            string str = "http://test.com/test"; 
            var option = new RedirectOption();

            option.LoadFromString(str);

            option.GetStringValue().ShouldBe(str);
        }

        [Theory]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("2013.05.29_14:33:41", typeof(ArgumentException))]
        public void LoadFromString_WithInvalidInput_ShouldThrow(string str, Type expectedException)
        {
            var option = new RedirectOption();
            Action testAction = () => option.LoadFromString(str);

            testAction.ShouldThrow(expectedException);
        }
    }
}
