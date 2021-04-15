using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using Shouldly;
using System;
using System.Text;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class OptionsStringSerializationTest
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
        [InlineData("/patH/Page;NonExistentComparison", typeof(ArgumentException))]
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
        [InlineData("", typeof(ArgumentNullException))]
        public void Test_BypassUserRoleOption(string userRole, Type expectedException)
        {
            var option = new BypassUserRoleOption();
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
        [InlineData("ContentRootPath;file.txt;5300", null)]
        [InlineData("ContentRootPath", typeof(FormatException))]
        [InlineData("RootPath_NotInEnum;File.TXT;5300", typeof(ArgumentException))]
        [InlineData(null, typeof(ArgumentNullException))]
        public void Test_ResponseFileOption(string input, Type expectedException)
        {
            var option = new ResponseFromFileOption();
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
        [InlineData("ContentRootPath;file.txt;5300", "file.txt", true)]
        [InlineData("ContentRootPath;file.txt;5300", "file2.txt", false)]
        [InlineData("ContentRootPath;File.TXT;5300", "file.txt", false)]
        public void Test_ResponseFileOption_FilePath(string input, string path, bool shouldBeEqual)
        {
            var option = new ResponseFromFileOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldNotThrow();
            option.Value.ShouldNotBeNull();

            if (shouldBeEqual)
            {
                option.Value.File.Path.ShouldBe(path);
            }
            else
            {
                option.Value.File.Path.ShouldNotBe(path);
            }
        }

        [Theory]
        [InlineData("ContentRootPath;file.txt;5300", PathBaseDirectory.ContentRootPath, true)]
        [InlineData("ContentRootPath;file.txt;5300", PathBaseDirectory.WebRootPath, false)]
        public void Test_ResponseFileOption_BaseDir(string input, PathBaseDirectory baseDir, bool shouldBeEqual)
        {
            var option = new ResponseFromFileOption();
            Action testAction = () => option.LoadFromString(input);

            testAction.ShouldNotThrow();
            option.Value.ShouldNotBeNull();

            if (shouldBeEqual)
            {
                option.Value.File.BaseDir.ShouldBe(baseDir);
            }
            else
            {
                option.Value.File.BaseDir.ShouldNotBe(baseDir);
            }
        }

        [Fact]
        public void Test_ResponseFileOption_ParamConstructor()
        {
            string filePath = "some.txt";
            PathBaseDirectory baseDir = PathBaseDirectory.ContentRootPath;
            int code503RetryInterval = 2000;

            Func<ResponseFromFileOption> testFunc = () 
                => new ResponseFromFileOption(filePath, baseDir, code503RetryInterval);

            ResponseFromFileOption opt = testFunc.ShouldNotThrow();

            opt.Value.ShouldNotBeNull();
            opt.Value.File.Path.ShouldBe(filePath);
            opt.Value.File.BaseDir.ShouldBe(baseDir);
            opt.Value.Code503RetryInterval.ShouldBe(code503RetryInterval);
        }

        [Theory]
        [InlineData("Text;65001;5300;maintenance mode", null)]
        [InlineData("Html;65002;5300;maintenance mode", typeof(NotSupportedException))] /* there is no encoding with code page 65002 */
        [InlineData("Music;65001;5300;maintenance mode", typeof(ArgumentException))] /* music is not a valid content type */
        [InlineData("Text;10v10s54;5300;maintenance mode", typeof(FormatException))] /* 10v10s54 is not a valid integer */
        [InlineData("Text;101054;5300;maintenance mode", typeof(ArgumentOutOfRangeException))] /* 101054 is not a valid code page */
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
        [InlineData("Text;65001;5300;maintenance mode", ContentType.Text, true)]
        [InlineData("Html;65001;5300;maintenance mode", ContentType.Text, false)]
        [InlineData("Html;65001;5300;<html><head></head><body>maintenance mode</body></html>", ContentType.Html, true)]
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
        [InlineData("Text;65001;5300;maintenance mode", 65001, true)]
        [InlineData("Text;65001;5300;maintenance mode", 12000, false)]
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
        [InlineData("Text;65001;5300;maintenance mode", 65001, "maintenance mode", true)]
        [InlineData("Text;65001;5300;maintenance mode", 65001, "not in maintenance mode", false)]
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
        [InlineData("Text;65001;5300;maintenance mode", 5300, null)]
        [InlineData("Text;65001;abc;maintenance mode", 5300, typeof(ArgumentException))]
        public void Test_ResponseOption_RetryAfter(string input, int retryAfter, Type expectedExceptionType)
        {
            var option = new ResponseOption();
            Action testAction = () => option.LoadFromString(input);

            if (expectedExceptionType != null)
            {
                testAction.ShouldThrow(expectedExceptionType);
            }
            else
            {
                testAction
                    .ShouldNotThrow();
                option.Value.Code503RetryInterval
                    .ShouldBe(retryAfter);
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
