using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Services;
using NSubstitute;
using Shouldly;
using System;
using Xunit;


namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class DefaultResponseOptionTest
    {
        [Theory]
        [InlineData("True", true)]
        [InlineData("False", false)]
        public void LoadFromString_WithValidInput_ValueShouldEqualInput(string str, bool isSet)
        {
            var option = new DefaultResponseOption(Substitute.For<IDirectoryMapperService>());

            option.LoadFromString(str);

            option.Value.ShouldBe(isSet);
        }

        [Theory]
        [InlineData("True")]
        [InlineData("False")]
        public void LoadFromString_WithValidInput_StringValueShouldEqualInput(string str)
        {
            var option = new DefaultResponseOption(Substitute.For<IDirectoryMapperService>());

            option.LoadFromString(str);

            option.GetStringValue().ShouldBe(str);
        }

        [Theory]
        [InlineData("Wrong", typeof(FormatException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void LoadFromString_WithInvalidInput_ShouldThrow(string str, Type expectedException)
        {
            var option = new DefaultResponseOption(Substitute.For<IDirectoryMapperService>());
            Action testAction = () => option.LoadFromString(str);

            testAction.ShouldThrow(expectedException);
        }
    }
}
