using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using MaintenanceModeMiddleware.Services;
using MaintenanceModeMiddleware.Tests.HelperTypes;
using Microsoft.AspNetCore.Http;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class MiddlewareOptionsBuilderTest
    {
        private readonly IDirectoryMapperService _dirMapperSvc = FakeDirectoryMapperService.Create();

        [Fact]
        public void Constructor_Default_ShouldAddDefaultOptions()
        {
            MiddlewareOptionsBuilder builder = null;
            
            builder = new MiddlewareOptionsBuilder(_dirMapperSvc);

            builder.GetOptions()
                .ShouldNotBeNull();
        }

        [Theory]
        [InlineData("test.txt", EnvDirectory.ContentRootPath)]
        [InlineData("test.txt", EnvDirectory.WebRootPath)]
        public void UseResponseFile_WithValidData_GetOptionsValueShouldEqualInput(string relativePath,
            EnvDirectory baseDir)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            string filePath = Path.Combine(_dirMapperSvc.GetAbsolutePath(baseDir), relativePath);
            File.Create(filePath);

            builder.UseResponseFromFile(relativePath, baseDir);

            ResponseFromFileOption option = builder
                 .GetOptions()
                 .GetSingleOrDefault<ResponseFromFileOption>();
            option.Value.ShouldNotBeNull();
            option.Value.File.Path.ShouldBe(relativePath);
            option.Value.File.BaseDir.ShouldBe(baseDir);
        }

        [Theory]
        [InlineData(null, default(EnvDirectory), false, typeof(ArgumentNullException))]
        [InlineData("", default(EnvDirectory), false, typeof(ArgumentNullException))]
        [InlineData("test" /* file name without an extension */, default(EnvDirectory), true, typeof(ArgumentException))]
        public void UseResponseFile_WithInvalidData_ShouldThrow(string relativePath,
            EnvDirectory baseDir,
            bool createFile,
            Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            if (createFile)
            {
                string filePath = Path.Combine(_dirMapperSvc.GetAbsolutePath(baseDir), relativePath);
                File.Create(filePath);
            }
            Action testAction = () =>
            {
                builder.UseResponseFromFile(relativePath, baseDir);
            };

            testAction.ShouldThrow(expectedException);
        }

        [Theory]
        [InlineData("maintenance mode", ResponseContentType.Text, 65001)]
        [InlineData("<p>maintenance mode</p>", ResponseContentType.Html, 65001)]
        public void UseResponseStringOverload_WithValidData_ValueShouldEqualInput(string response,
            ResponseContentType contentType,
            int codePage)
        {
            Encoding encoding = Encoding.GetEncoding(codePage);
            var builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            
            
            builder.UseResponse(response, contentType, encoding);


            var option = builder
                .GetOptions()
                .GetSingleOrDefault<ResponseOption>();
            option.Value.ShouldNotBeNull();
            option.Value.ContentBytes.ShouldBe(encoding.GetBytes(response));
            option.Value.ContentType.ShouldBe(contentType);
            option.Value.ContentEncoding.ShouldBe(encoding);
        }

        [Theory]
        [InlineData("maintenance mode", ResponseContentType.Text, 65001)]
        [InlineData("<p>maintenance mode</p>", ResponseContentType.Html, 65001)]
        public void UseResponseBytesOverload_WithValidData_ValueShouldEqualInput(string response,
            ResponseContentType contentType,
            int codePage)
        {
            Encoding encoding = Encoding.GetEncoding(codePage);
            var builder = new MiddlewareOptionsBuilder(_dirMapperSvc);


            builder.UseResponse(encoding.GetBytes(response), contentType, encoding);


            var option = builder
                .GetOptions()
                .GetSingleOrDefault<ResponseOption>();
            option.Value.ShouldNotBeNull();
            option.Value.ContentBytes.ShouldBe(encoding.GetBytes(response));
            option.Value.ContentType.ShouldBe(contentType);
            option.Value.ContentEncoding.ShouldBe(encoding);
        }

        [Theory]
        [InlineData("", ResponseContentType.Text, 65001, typeof(ArgumentNullException))]
        [InlineData(null, ResponseContentType.Text, 65001, typeof(ArgumentNullException))]
        public void UseResponseStringOverload_WithInvalidData_ShouldThrow(string response,
            ResponseContentType contentType,
            int codePage,
            Type expectedException)
        {
            Encoding encoding = Encoding.GetEncoding(codePage);
            var builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            Action testAction = () =>
            {
                builder.UseResponse(response, contentType, encoding);
            };


            testAction.ShouldThrow(expectedException);
        }

        [Theory]
        [InlineData("", ResponseContentType.Text, 65001, typeof(ArgumentNullException))]
        [InlineData(null, ResponseContentType.Text, 65001, typeof(ArgumentNullException))]
        public void UseResponseBytesOverload_WithInvalidData_ShouldThrow(string response,
            ResponseContentType contentType,
            int codePage,
            Type expectedException)
        {
            Encoding encoding = Encoding.GetEncoding(codePage);
            var builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            Action testAction = () =>
            {
                builder.UseResponse(string.IsNullOrEmpty(response)
                            ? null
                            : encoding.GetBytes(response),
                        contentType,
                        encoding);
            };


            testAction.ShouldThrow(expectedException);
        }

        [Fact]
        public void UseDefaultResponse_WhenCalled_ShouldSucceed()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
                // ensure that the UseDefaultResponse option is used
            builder.UseNoDefaultValues();

            builder.UseDefaultResponse();
            
            builder.GetOptions()
                .GetSingleOrDefault<DefaultResponseOption>()
                .ShouldNotBeNull();
        }

        [Fact]
        public void UseRedirect_WithValidUriPath_ValueShouldEqualInput()
        {
            string uriPath = "/test";
            var builder = new MiddlewareOptionsBuilder(_dirMapperSvc);


            builder.UseRedirect(uriPath);


            var option = builder
                .GetOptions()
                .GetSingleOrDefault<IRedirectInitializer>();
            option.RedirectLocation.ShouldBe(uriPath);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData("2013.05.29_14:33:41", typeof(ArgumentException))]
        public void UseRedirect_WithInvalidUriPath_ShouldThrow(string uriPath, Type expectedException)
        {
            var builder = new MiddlewareOptionsBuilder(_dirMapperSvc);

            Action testAction = () =>
            {
                builder.UseRedirect(uriPath);
            };

            testAction.ShouldThrow(expectedException);
        }

        [Fact]
        public void BypassUser_WithUser_ShouldSucceed()
        {
            const string userName = "userName1";
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            
            builder.BypassUser(userName);

            builder
                .GetOptions()
                .GetSingleOrDefault<BypassUserNameOption>()
                .ShouldNotBeNull()
                .Value.ShouldBe(userName);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentNullException))]
        public void BypassUser_WithNullOrEmptyUsername_ShouldThrow(string userName, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            Action testAction = () =>
            {
                builder.BypassUser(userName);
            };


            testAction.ShouldThrow(expectedException);
        }

        [Fact]
        public void BypassUsers_WithValidUsers_ShouldSucceed()
        {
            string[] userNames = new string[] { "userName1", "userName2" };
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            

            builder.BypassUsers(userNames);

            IEnumerable<BypassUserNameOption> options = builder
                .GetOptions()
                .GetAll<BypassUserNameOption>();
            options
                .ShouldNotBeNull()
                .Count()
                .ShouldBe(userNames.Count());
            userNames
                .ShouldContain(options.First().Value);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData(new string[] { }, typeof(ArgumentException))]
        public void BypassUsers_WithNullOrEmptyArray_ShouldThrow(string[] userNames, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            Action testAction = () =>
            {
                builder.BypassUsers(userNames);
            };

            testAction.ShouldThrow(expectedException);
        }

        [Fact]
        public void BypassUserRole_WithRole_ValueShouldBeEqualToTheInput()
        {
            const string userRole = "role1";
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            
            builder.BypassUserRole(userRole);

            BypassUserRoleOption option = builder.GetOptions()
                .GetSingleOrDefault<BypassUserRoleOption>();
            option
                .ShouldNotBeNull()
                .Value.ShouldBe(userRole);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentNullException))]
        public void BypassUserRole_WithNullOrEmptyRole_ShouldThrow(string userRole, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            Action testAction = () =>
            {
                builder.BypassUserRole(userRole);
            };

            testAction.ShouldThrow(expectedException);
        }

        [Fact]
        public void BypassUserRoles_WithRoles_ShouldSucceed()
        {
            string[] userRoles = new string[] { "role1", "role2" };
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            
            builder.BypassUserRoles(userRoles);

            IEnumerable<BypassUserRoleOption> options = builder.GetOptions()
                .GetAll<BypassUserRoleOption>();
            options
                .ShouldNotBeNull()
                .Count()
                .ShouldBe(userRoles.Count());
            userRoles
                .ShouldContain(options.First().Value);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData(new string[] { }, typeof(ArgumentException))]
        public void BypassUserRoles_WithNullOrEmptyArray_ShouldThrow(string[] userRoles, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            Action testAction = () =>
            {
                builder.BypassUserRoles(userRoles);
            };


            testAction.ShouldThrow(expectedException);
        }

        [Fact]
        public void BypassAllAuthenticatedUsers_Default_ShouldSucceed()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            
            builder.BypassAllAuthenticatedUsers();

            builder.GetOptions()
                .GetSingleOrDefault<BypassAllAuthenticatedUsersOption>()
                .ShouldNotBeNull();
        }

        [Fact]
        public void BypassUrlPath_WithEmptyPath_ShouldThrowArgumentException()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            Action testAction = () =>
            {
                builder.BypassUrlPath(new PathString(), StringComparison.Ordinal);;
            };

            testAction.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void BypassUrlPath_WithNonEmptyPath_ValueShouldEqualInput()
        {
            const string urlPath = "/path";
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            
            builder.BypassUrlPath(new PathString(urlPath), StringComparison.Ordinal);
               
            builder.GetOptions()
                .GetSingleOrDefault<BypassUrlPathOption>()
                .ShouldNotBeNull()
                .Value.PathString.Value
                .ShouldBe(urlPath);
        }

        [Theory]
        [MemberData(nameof(GetBypassUrlPathsMembers))]
        public void BypassUrlPaths(PathString[] pathStrings, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            Func<IEnumerable<BypassUrlPathOption>> testFunc = () =>
            {
                builder.BypassUrlPaths(pathStrings, StringComparison.Ordinal);
                return builder.GetOptions().GetAll<BypassUrlPathOption>();
            };

            if (expectedException != null)
            {
                testFunc.ShouldThrow(expectedException);
            }
            else
            {
                IEnumerable<BypassUrlPathOption> options = testFunc
                    .ShouldNotThrow()
                    .ShouldNotBeNull();

                if (pathStrings != null)
                {
                    options
                        .Count()
                        .ShouldBe(pathStrings.Length);
                }
               
            }
        }

        public static IEnumerable<object[]> GetBypassUrlPathsMembers()
        {
            yield return new object[]
            {
                null,
                typeof(ArgumentNullException)
            };

            yield return new object[]
            {
                new PathString[] { },
                typeof(ArgumentException)
            };

            yield return new object[]
            {
                new PathString[] { "/some/path"},
                null
            };

            yield return new object[]
            {
                new PathString[] { "/some/path", "/some/other/path"},
                null
            };
        }

        [Theory]
        [InlineData("txt")]
        [InlineData(".txt")]
        public void BypassFileExtension_WithValidExtension_ValueShouldEqualInput(string extension)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            
            builder.BypassFileExtension(extension);
                

            builder.GetOptions()
                .GetSingleOrDefault<BypassFileExtensionOption>()
                .ShouldNotBeNull()
                // the method BypassFileExtension removes the dots of the extensions
                .Value.ShouldBe(extension.StartsWith('.')
                    ? extension.Substring(1)
                    : extension);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentNullException))]
        public void BypassFileExtension_WithNullOrEmptyInput_ShouldThrow(string extension, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            Action testAction = () =>
            {
                builder.BypassFileExtension(extension);
            };


            testAction.ShouldThrow(expectedException);
        }

        [Fact]
        public void BypassFileExtensions_WithValidExtensions_ShouldSucceed()
        {
            string[] extensions = new string[] { "txt", "html" };
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);

            builder.BypassFileExtensions(extensions);

            IEnumerable<BypassFileExtensionOption> options = 
                builder.GetOptions().GetAll<BypassFileExtensionOption>();
            options.ShouldNotBeEmpty();
            options.Count()
                .ShouldBe(extensions.Length);
            extensions
                .ShouldContain(options.First().Value);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData(new string[] { }, typeof(ArgumentException))]
        public void BypassFileExtensions_WithNullOrEmptyArray_ShouldThrow(string[] extensions, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            Action testAction = () =>
            {
                builder.BypassFileExtensions(extensions);
            };

            testAction.ShouldThrow(expectedException);
        }

        [Fact]
        public void UseNoDefaultValues_Default_GetOptionsShouldThrow()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            builder.UseNoDefaultValues();
            Action assertAction = () => builder.GetOptions();

            assertAction.ShouldThrow<ArgumentException>()
                .Message.ShouldStartWith("No response or redirect was specified.");
        }

        [Fact]
        public void FillEmptyOptionsWithDefault_Default_ShouldAddOptions()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);

            builder.GetOptions().ShouldNotBeNull()
                .GetAll().ShouldNotBeEmpty();
        }

        [Fact]
        public void MiddlewareOptionsBuilder_WhenGetOptionsIsCalledTwice_ShouldNotThrow()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            Action testAction = () => 
            {
                builder.GetOptions();
                builder.GetOptions();
            };

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void MiddlewareOptionsBuilder_WhenDuplicateResponseOptionIsSet_GetOptionShouldThrow()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc); 
            builder.UseNoDefaultValues();
            builder.UseDefaultResponse();
            builder.UseResponse(Encoding.UTF8.GetBytes("test"), ResponseContentType.Text, Encoding.UTF8);

            Action testAction = () =>
            {
                builder.GetOptions();
            };

            testAction.ShouldThrow<ArgumentException>()
                .Message.ShouldStartWith("More than one response");

        }

        [Fact]
        public void MiddlewareOptionsBuilder_WhenBothResponseAndRedirectOptionIsSet_GetOptionShouldThrow()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_dirMapperSvc);
            builder.UseDefaultResponse();
            builder.UseRedirect("/test");

            Action testAction = () =>
            {
                builder.GetOptions();
            };

            testAction.ShouldThrow<ArgumentException>()
                .Message.ShouldStartWith("Both a response and a redirect were specified.");

        }
    }
}
