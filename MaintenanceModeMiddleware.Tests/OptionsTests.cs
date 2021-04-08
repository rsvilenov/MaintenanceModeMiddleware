using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using System.Text;
using Xunit;

namespace MaintenanceModeMiddleware.Tests
{
    public class OptionsTests
    {
        [Theory]
        [InlineData("True", true, null)]
        [InlineData("False", false, null)]
        [InlineData("Wrong", false, typeof(FormatException))]
        [InlineData(null, false, typeof(ArgumentNullException))]
        public void Test_BypassAllAuthenticatedUsersOption(string str, bool isSet, Type expectedException)
        {
            var option = new BypassAllAuthenticatedUsersOption();
            Action testAction = () => option.LoadFromString(str);

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();

                option.Value.ShouldBe(isSet);
                option.GetStringValue().ShouldBe(str);
            }
        }

        [Theory]
        [InlineData("/test/file.txt", null)]
        [InlineData(null, typeof(ArgumentNullException))]
        public void Test_BypassFileExtensionOption(string input, Type expectedException)
        {
            var option = new BypassFileExtensionOption();
            Action testAction = () => option.LoadFromString(input);

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();

                option.Value.ShouldBe(input);
                option.GetStringValue().ShouldBe(input);
            }
        }

        [Theory]
        [InlineData("/path/page;Ordinal", null)]
        [InlineData("/patH/Page;OrdinalIgnoreCase", null)]
        [InlineData("/patH/Page;Ordinal", null)]
        [InlineData("/path/page", typeof(FormatException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void Test_BypassUrlPathOption(string input, Type expectedException)
        {
            var option = new BypassUrlPathOption();
            Action testAction = () => option.LoadFromString(input);

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();

                option.Value.ShouldNotBeNull();
                option.GetStringValue().ShouldBe(input);
            }
        }

        [Theory]
        [InlineData("/path/page;Ordinal", "/path/page", true)]
        [InlineData("/patH/Page;OrdinalIgnoreCase", "/path/page", true)]
        [InlineData("/patH/Page;Ordinal", "/path/page", false)]
        public void Test_BypassUrlPathOption_Path(string input, string resultPath, bool shouldBeEqual)
        {
            var option = new BypassUrlPathOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldNotThrow();

            option.Value.ShouldNotBeNull();

            bool isEqual = option.Value.PathString.ToString()
                    .Equals(resultPath, option.Value.Comparison);

            if (shouldBeEqual)
            {
                isEqual.ShouldBeTrue();
            }
            else
            {
                isEqual.ShouldBeFalse();
            }
        }

        [Theory]
        [InlineData("/path/page;Ordinal", StringComparison.Ordinal, true)]
        [InlineData("/patH/page;OrdinalIgnoreCase", StringComparison.OrdinalIgnoreCase, true)]
        [InlineData("/patH/page;OrdinalIgnoreCase", StringComparison.Ordinal, false)]
        public void Test_BypassUrlPathOption_Comparison(string input, StringComparison comparison, bool shouldBeEqual)
        {
            var option = new BypassUrlPathOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldNotThrow();

            option.Value.ShouldNotBeNull();

            if (shouldBeEqual)
            {
                option.Value.Comparison.ShouldBe(comparison);
            }
            else
            {
                option.Value.Comparison.ShouldNotBe(comparison);
            }
        }

        [Theory]
        [InlineData("demoUser", null)]
        [InlineData(null, typeof(ArgumentNullException))]
        public void Test_BypassUserNameOption(string userName, Type expectedException)
        {
            var option = new BypassUserNameOption();
            Action testAction = () => option.LoadFromString(userName);
            
            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();

                option.Value
                    .ShouldNotBeNull()
                    .ShouldBe(userName);
                option.GetStringValue().ShouldBe(userName);
            }
        }

        [Theory]
        [InlineData("demoRole", null)]
        [InlineData(null, typeof(ArgumentNullException))]
        public void Test_BypassUserRoleOption(string userRole, Type expectedException)
        {
            var option = new BypassUserNameOption();
            Action testAction = () => option.LoadFromString(userRole);

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();

                option.Value
                    .ShouldNotBeNull()
                    .ShouldBe(userRole);
                option.GetStringValue().ShouldBe(userRole);
            }
        }

        [Theory]
        [InlineData("2300", null)]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("not an integer", typeof(FormatException))]
        [InlineData("2147483648" /* Int32.MaxValue + 1 */, typeof(OverflowException))]
        public void Test_Code503RetryIntervalOption(string interval, Type expectedException)
        {
            var option = new Code503RetryIntervalOption();
            Action testAction = () => option.LoadFromString(interval);

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();

                option.Value.ToString().ShouldBe(interval);
                option.GetStringValue().ShouldBe(interval);
            }
        }

        [Theory]
        [InlineData("ContentRootPath;file.txt", null)]
        [InlineData("ContentRootPath", typeof(FormatException))]
        [InlineData("RootPath_NotInEnum;File.TXT", typeof(ArgumentException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void Test_ResponseFileOption(string input, Type expectedException)
        {
            var option = new ResponseFileOption();
            Action testAction = () => option.LoadFromString(input);

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();
                option.Value.ShouldNotBeNull();
                option.GetStringValue().ShouldBe(input);
            }
        }

        [Theory]
        [InlineData("ContentRootPath;file.txt", "file.txt", true)]
        [InlineData("ContentRootPath;file.txt", "file2.txt", false)]
        [InlineData("ContentRootPath;File.TXT", "file.txt", false)]
        public void Test_ResponseFileOption_FilePath(string input, string path, bool shouldBeEqual)
        {
            var option = new ResponseFileOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldNotThrow();
            option.Value.ShouldNotBeNull();

            if (shouldBeEqual)
            {
                option.Value.FilePath.ShouldBe(path);
            }
            else
            {
                option.Value.FilePath.ShouldNotBe(path);
            }
        }

        [Theory]
        [InlineData("ContentRootPath;file.txt", PathBaseDirectory.ContentRootPath, true)]
        [InlineData("ContentRootPath;file.txt", PathBaseDirectory.WebRootPath, false)]
        public void Test_ResponseFileOption_BaseDir(string input, PathBaseDirectory baseDir, bool shouldBeEqual)
        {
            var option = new ResponseFileOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldNotThrow();
            option.Value.ShouldNotBeNull();

            if (shouldBeEqual)
            {
                option.Value.BaseDir.ShouldBe(baseDir);
            }
            else
            {
                option.Value.BaseDir.ShouldNotBe(baseDir);
            }
        }

        [Theory]
        [InlineData("Text;65001;maintenance mode", null)]
        [InlineData("Html;65002;maintenance mode", typeof(NotSupportedException) /* there is no encoding with code page 65002 */ )]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("string in wrong format", typeof(FormatException))]
        public void Test_ResponseOption(string input, Type expectedException)
        {
            var option = new ResponseOption();
            Action testAction = () => option.LoadFromString(input);

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();
                option.Value.ShouldNotBeNull();
                option.GetStringValue().ShouldBe(input);
            }
        }

        [Theory]
        [InlineData("Text;65001;maintenance mode", ContentType.Text, true)]
        [InlineData("Html;65001;maintenance mode", ContentType.Text, false)]
        [InlineData("Html;65001;<html><head></head><body>maintenance mode</body></html>", ContentType.Html, true)]
        public void Test_ResponseOption_ContentType(string input, ContentType contentType, bool shouldBeEqual)
        {
            var option = new ResponseOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldNotThrow();
            option.Value.ShouldNotBeNull();

            if (shouldBeEqual)
            {
                option.Value.ContentType.ShouldBe(contentType);
            }
            else
            {
                option.Value.ContentType.ShouldNotBe(contentType);
            }
        }

        [Theory]
        [InlineData("Text;65001;maintenance mode", 65001, true)]
        [InlineData("Text;65001;maintenance mode", 12000, false)]
        public void Test_ResponseOption_Encoding(string input, int codePage, bool shouldBeEqual)
        {
            var option = new ResponseOption();
            Action testAction = () => option.LoadFromString(input);

            Encoding encoding = Encoding.GetEncoding(codePage);

            testAction.ShouldNotThrow();
            option.Value.ShouldNotBeNull();

            if (shouldBeEqual)
            {
                option.Value.ContentEncoding.ShouldBe(encoding);
            }
            else
            {
                option.Value.ContentEncoding.ShouldNotBe(encoding);
            }
        }

        [Theory]
        [InlineData("Text;65001;maintenance mode", 65001, "maintenance mode", true)]
        [InlineData("Text;65001;maintenance mode", 65001, "not in maintenance mode", false)]
        public void Test_ResponseOption_Content(string input, int codePage, string content, bool shouldBeEqual)
        {
            var option = new ResponseOption();
            Action testAction = () => option.LoadFromString(input);

            Encoding encoding = Encoding.GetEncoding(codePage);

            testAction.ShouldNotThrow();
            option.Value.ShouldNotBeNull();

            if (shouldBeEqual)
            {
                option.Value.ContentBytes.ShouldBe(encoding.GetBytes(content));
            }
            else
            {
                option.Value.ContentBytes.ShouldNotBe(encoding.GetBytes(content));
            }
        }

        [Theory]
        [InlineData("True", true, null)]
        [InlineData("False", false, null)]
        [InlineData("Wrong", false, typeof(FormatException))]
        [InlineData(null, false, typeof(ArgumentNullException))]
        public void Test_UseDefaultResponseOption(string str, bool isSet, Type expectedException)
        {
            var option = new UseDefaultResponseOption();
            Action testAction = () => option.LoadFromString(str);

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();

                option.Value.ShouldBe(isSet);
                option.GetStringValue().ShouldBe(str);
            }
        }

        [Theory]
        [InlineData("True", true, null)]
        [InlineData("False", false, null)]
        [InlineData("Wrong", false, typeof(FormatException))]
        [InlineData(null, false, typeof(ArgumentNullException))]
        public void Test_UseNoDefaultValuesOption(string str, bool isSet, Type expectedException)
        {
            var option = new UseNoDefaultValuesOption();
            Action testAction = () => option.LoadFromString(str);

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();

                option.Value.ShouldBe(isSet);
                option.GetStringValue().ShouldBe(str);
            }
        }
    }
}
