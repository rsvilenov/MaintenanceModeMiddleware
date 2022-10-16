using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class UseRedirectOptionTest
    {
        [Fact]
        public void LoadFromString_WithValidInput_ValueShouldEqualInput()
        {
            var urlPath = $"/{Guid.NewGuid()}";
            var option = new UseRedirectOption();

            option.LoadFromString(urlPath);

            option.Value.ToString().ShouldBe(urlPath);
        }

        [Fact]
        public void LoadFromString_WithValidInput_StringValueShouldEqualInput()
        {
            var urlPath = $"/{Guid.NewGuid()}";
            var option = new UseRedirectOption();

            option.LoadFromString(urlPath);

            option.GetStringValue().ShouldBe(urlPath);
        }

        [Theory]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("2013.05.29_14:33:41", typeof(ArgumentException))]
        public void LoadFromString_WithInvalidInput_ShouldThrow(string str, Type expectedException)
        {
            var option = new UseRedirectOption();
            Action testAction = () => option.LoadFromString(str);

            testAction.ShouldThrow(expectedException);
        }
    }
}
