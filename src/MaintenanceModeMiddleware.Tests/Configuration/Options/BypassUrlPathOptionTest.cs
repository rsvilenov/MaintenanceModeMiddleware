using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using Xunit;


namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class BypassUrlPathOptionTest
    {
        [Theory]
        [InlineData("/path/page;Ordinal")]
        [InlineData("/patH/Page;OrdinalIgnoreCase")]
        [InlineData("/patH/Page;Ordinal")]
        public void LoadFromString_WithValidInput_ValueShouldNotBeNull(string input)
        {
            var option = new BypassUrlPathOption();
            
            option.LoadFromString(input);

            option.Value
                .ShouldNotBeNull();
        }

        [Theory]
        [InlineData("/path/page;Ordinal")]
        [InlineData("/patH/Page;OrdinalIgnoreCase")]
        [InlineData("/patH/Page;Ordinal")]
        public void LoadFromString_WithValidInput_StringValueShouldMatchInput(string input)
        {
            var option = new BypassUrlPathOption();
            
            option.LoadFromString(input);

            option.GetStringValue()
                .ShouldBe(input);
        }

        [Theory]
        [InlineData("/patH/Page;NonExistentComparison", typeof(ArgumentException))]
        [InlineData("/path/page", typeof(FormatException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void LoadFromString_WithInvalidOrNullInput_ShouldThrow(string input, Type expectedException)
        {
            var option = new BypassUrlPathOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldThrow(expectedException);
        }

        [Theory]
        [InlineData("/path/page;Ordinal", "/path/page", true)]
        [InlineData("/patH/Page;OrdinalIgnoreCase", "/path/page", true)]
        [InlineData("/patH/Page;Ordinal", "/path/page", false)]
        public void LoadFromString_WithComparison_ComparisonShouldBeTakenIntoAccount(string input, string resultPath, bool shouldBeEqual)
        {
            var option = new BypassUrlPathOption();
            
            option.LoadFromString(input);

            bool isEqual = option.Value.PathString.ToString()
                    .Equals(resultPath, option.Value.Comparison);
            isEqual.ShouldBe(shouldBeEqual);
        }

        [Theory]
        [InlineData("/path/page;Ordinal", StringComparison.Ordinal)]
        [InlineData("/patH/page;OrdinalIgnoreCase", StringComparison.OrdinalIgnoreCase)]
        public void LoadFromString_WithComparison_ComparisonPropInValueShouldMatchInput(string input, StringComparison comparison)
        {
            var option = new BypassUrlPathOption();
            
            option.LoadFromString(input);

            option.Value.Comparison.ShouldBe(comparison);
        }

    }
}
