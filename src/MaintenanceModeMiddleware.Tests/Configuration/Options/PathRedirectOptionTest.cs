using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class PathRedirectOptionTest
    {
        private const string PARTS_SEPARATOR = "[::]";

        [Fact]
        public void LoadFromString_WithValidInput_ValueShouldEqualInput()
        {
            uint interval = 5000;
            bool set503StatusCode = true;
            string path = $"/{Guid.NewGuid()}";
            var serializedOption = $"{path}{PARTS_SEPARATOR}{interval}{PARTS_SEPARATOR}{set503StatusCode}";
            var option = new PathRedirectOption();

            option.LoadFromString(serializedOption);

            option.Value.Path.ToString().ShouldBe(path);
            option.Value.Code503RetryInterval.ShouldBe(interval);
            option.Value.Set503StatusCode.ShouldBe(set503StatusCode);
        }

        [Fact]
        public void LoadFromString_WithValidInput_StringValueShouldEqualInput()
        {
            int interval = 5000;
            bool set503StatusCode = true;
            string path = $"/{Guid.NewGuid()}";
            var serializedOption = $"{path}{PARTS_SEPARATOR}{interval}{PARTS_SEPARATOR}{set503StatusCode}";
            var option = new PathRedirectOption();

            option.LoadFromString(serializedOption);

            option.GetStringValue().ShouldBe(serializedOption);
        }

        [Theory]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("2013.05.29_14:33:41", typeof(ArgumentException))]
        public void LoadFromString_WithInvalidInput_ShouldThrow(string str, Type expectedException)
        {
            var option = new PathRedirectOption();
            Action testAction = () => option.LoadFromString(str);

            testAction.ShouldThrow(expectedException);
        }
    }
}
