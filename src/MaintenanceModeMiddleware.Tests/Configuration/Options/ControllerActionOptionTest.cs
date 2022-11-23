using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration.Options
{
    public class ControllerActionOptionTest
    {
        [Theory]
        [InlineData("MaintenanceArea")]
        [InlineData("")]
        public void LoadFromString_WithValidInput_ValueShouldEqualInput(string areaName)
        {
            string controllerName = "MaintenanceController";
            string actionName = "MaintenanceController";
            const string separator = ":";
            string str = $"{areaName}{separator}{controllerName}{separator}{actionName}";
            var option = new ControllerActionOption();

            option.LoadFromString(str);

            option.Value.AreaName.ShouldBe(areaName);
            option.Value.ControllerName.ShouldBe(controllerName);
            option.Value.ActionName.ShouldBe(actionName);

            option.GetStringValue().ShouldBe(str);
        }

        [Theory]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("sdfasdfd", typeof(IndexOutOfRangeException))]
        [InlineData("sdfasdfd:s", typeof(IndexOutOfRangeException))]
        public void LoadFromString_WithInvalidInput_ShouldThrow(string str, Type expectedException)
        {
            var option = new ControllerActionOption();
            Action testAction = () => option.LoadFromString(str);

            testAction.ShouldThrow(expectedException);
        }
    }
}
