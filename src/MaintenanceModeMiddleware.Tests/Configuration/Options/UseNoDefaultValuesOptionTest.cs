using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using Xunit;


namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class UseNoDefaultValuesOptionTest
    {
        [Theory]
        [InlineData("True", true)]
        [InlineData("False", false)]
        public void LoadFromString_WithValidInput_ValueShouldEqualInput(string str, bool isSet)
        {
            var option = new UseNoDefaultValuesOption();

            option.LoadFromString(str);

            option.Value.ShouldBe(isSet);
        }

        [Theory]
        [InlineData("True")]
        [InlineData("False")]
        public void LoadFromString_WithValidInput_StringValueShouldEqualInput(string str)
        {
            var option = new UseNoDefaultValuesOption();

            option.LoadFromString(str);

            option.GetStringValue().ShouldBe(str);
        }

        [Theory]
        [InlineData("Wrong", typeof(FormatException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void LoadFromString_WithInvalidInput_ShouldThrow(string str, Type expectedException)
        {
            var option = new UseNoDefaultValuesOption();
            Action testAction = () => option.LoadFromString(str);

            testAction.ShouldThrow(expectedException);
        }
    }
}
