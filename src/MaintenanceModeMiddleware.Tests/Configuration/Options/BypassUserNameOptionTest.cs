using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using Xunit;


namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class BypassUserNameOptionTest
    {
        [Fact]
        public void LoadFromString_WithValidUser_ValueShouldEqualInput()
        {
            const string userName = "demoUser";
            var option = new BypassUserNameOption();
            
            option.LoadFromString(userName);

            option.Value
                .ShouldBe(userName);
        }

        [Fact]
        public void LoadFromString_WithValidUser_StringValueShouldEqualInput()
        {
            const string userName = "demoUser";
            var option = new BypassUserNameOption();
            
            option.LoadFromString(userName);

            option.GetStringValue()
                .ShouldBe(userName);
        }

        [Fact]
        public void LoadFromString_WithNull_ShouldThrowArgumentNullException()
        {
            string userName = null;
            var option = new BypassUserNameOption();
            Action testAction = () => option.LoadFromString(userName);

            testAction.ShouldThrow<ArgumentNullException>();
        }

    }
}