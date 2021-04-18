using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using Xunit;


namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class BypassUserRoleOptionTest
    {
        [Fact]
        public void LoadFromString_WithValidRole_ValueShouldEqualInput()
        {
            const string userRole = "demoRole";
            var option = new BypassUserRoleOption();
            
            option.LoadFromString(userRole);

            option.Value
                .ShouldBe(userRole);
        }
        
        [Fact]
        public void LoadFromString_WithValidRole_StringValueShouldEqualInput()
        {
            const string userRole = "demoRole";
            var option = new BypassUserRoleOption();
            
            option.LoadFromString(userRole);

            option.GetStringValue()
                .ShouldBe(userRole);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentNullException))]
        public void LoadFromString_WithNullOrEmptyString_ShouldThrowArgumentNullException(string userRole, Type expectedException)
        {
            var option = new BypassUserRoleOption();
            Action testAction = () => option.LoadFromString(userRole);

            testAction.ShouldThrow(expectedException);
        }


    }
}
