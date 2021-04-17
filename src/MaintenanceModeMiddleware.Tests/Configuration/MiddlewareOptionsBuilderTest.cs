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
        private readonly IPathMapperService _pathMapperSvc = FakePathMapperService.Create();

        [Fact]
        public void Constructor()
        {
            MiddlewareOptionsBuilder builder = null;
            Action testAction = () => builder = new MiddlewareOptionsBuilder(_pathMapperSvc);

            testAction.ShouldNotThrow();

            builder.GetOptions()
                .ShouldNotBeNull();
        }

        [Theory]
        [InlineData(null, default(EnvDirectory), false, typeof(ArgumentNullException))]
        [InlineData("", default(EnvDirectory), false, typeof(ArgumentNullException))]
        [InlineData("test" /* file name without an extension */, default(EnvDirectory), true, typeof(ArgumentException))]
        [InlineData("test.txt", EnvDirectory.ContentRootPath, true, null)]
        [InlineData("test.txt", EnvDirectory.WebRootPath, true, null)]
        public void UseResponseFile(string relativePath, 
            EnvDirectory baseDir, 
            bool createFile,
            Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Func<ResponseFromFileOption> testFunc = () =>
            {
                if (createFile)
                {
                    string filePath = Path.Combine(_pathMapperSvc.GetPath(baseDir), relativePath);
                    File.Create(filePath);
                }

                builder.UseResponseFromFile(relativePath, baseDir);
                return builder.GetOptions().GetSingleOrDefault<ResponseFromFileOption>();
            };

            if (expectedException == null)
            {
                ResponseFromFileOption option = testFunc
                    .ShouldNotThrow()
                    .ShouldNotBeNull();

                option.Value.ShouldNotBeNull();
                option.Value.File.Path.ShouldBe(relativePath);
                option.Value.File.BaseDir.ShouldBe(baseDir);
            }
            else
            {
                testFunc.ShouldThrow(expectedException);
            }
        }

        [Theory]
        [InlineData("", ContentType.Text, 65001, typeof(ArgumentNullException))]
        [InlineData(null, ContentType.Text, 65001, typeof(ArgumentNullException))]
        [InlineData("maintenance mode", ContentType.Text, 65001, null)]
        [InlineData("<p>maintenance mode</p>", ContentType.Html, 65001, null)]
        public void UseResponse(string response, 
            ContentType contentType, 
            int codePage,
            Type expectedException)
        {
            Encoding encoding = Encoding.GetEncoding(codePage);
            Func<ResponseOption>[] testFunctions = new Func<ResponseOption>[]
            {
                () =>
                {
                    var builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
                    builder.UseResponse(response, contentType, encoding);
                    return builder.GetOptions().GetSingleOrDefault<ResponseOption>();
                },
                () =>
                {
                    var builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
                    builder.UseResponse(string.IsNullOrEmpty(response) 
                            ? null 
                            : encoding.GetBytes(response),
                        contentType, 
                        encoding);

                    return builder.GetOptions().GetSingleOrDefault<ResponseOption>();
                }
            };

            if (expectedException == null)
            {
                foreach (Func<ResponseOption> func in testFunctions)
                {
                    var option = func
                        .ShouldNotThrow()
                        .ShouldNotBeNull();

                    option.Value.ShouldNotBeNull();
                    option.Value.ContentBytes.ShouldBe(encoding.GetBytes(response));
                    option.Value.ContentType.ShouldBe(contentType);
                    option.Value.ContentEncoding.ShouldBe(encoding);
                }
                
            }
            else
            {
                foreach (Func<ResponseOption> func in testFunctions)
                {
                    func.ShouldThrow(expectedException);
                }
            }
        }

        [Fact]
        public void UseDefaultResponse()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Action testAction = () =>
            {
                // ensure that the UseDefaultResponse option is used
                builder.UseNoDefaultValues();
                builder.UseDefaultResponse();
            };

            testAction.ShouldNotThrow();

            builder.GetOptions()
                .GetSingleOrDefault<UseDefaultResponseOption>()
                .ShouldNotBeNull();
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData("userName1", null)]
        public void BypassUser(string userName, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Func<BypassUserNameOption> testFunc = () =>
            {
                builder.BypassUser(userName);
                return builder.GetOptions().GetSingleOrDefault<BypassUserNameOption>();
            };

            if (expectedException != null)
            {
                testFunc.ShouldThrow(expectedException);
                return;
            }

            BypassUserNameOption option = testFunc.ShouldNotThrow();

            option
                .ShouldNotBeNull()
                .Value.ShouldBe(userName);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData(new string[] { }, typeof(ArgumentException))]
        [InlineData(new string[] { "userName1", "userName2" }, null)]
        public void BypassUsers(string[] userNames, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Func<IEnumerable<BypassUserNameOption>> testFunc = () =>
            {
                builder.BypassUsers(userNames);
                return builder.GetOptions().GetAll<BypassUserNameOption>();
            };

            if (expectedException != null)
            {
                testFunc.ShouldThrow(expectedException);
                return;
            }

            IEnumerable<BypassUserNameOption> options = testFunc.ShouldNotThrow();

            options
                .ShouldNotBeNull()
                .ShouldNotBeEmpty();
            options
                .Count()
                .ShouldBe(userNames.Count());
            userNames
                .ShouldContain(options.First().Value);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData("role1", null)]
        public void BypassUserRole(string userRole, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Func<BypassUserRoleOption> testFunc = () =>
            {
                builder.BypassUserRole(userRole);
                return builder.GetOptions().GetSingleOrDefault<BypassUserRoleOption>();
            };

            if (expectedException != null)
            {
                testFunc.ShouldThrow(expectedException);
                return;
            }

            BypassUserRoleOption option = testFunc.ShouldNotThrow();

            option
                .ShouldNotBeNull()
                .Value.ShouldBe(userRole);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData(new string[] { }, typeof(ArgumentException))]
        [InlineData(new string[] { "role1", "role2" }, null)]
        public void BypassUserRoles(string[] userRoles, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Func<IEnumerable<BypassUserRoleOption>> testFunc = () =>
            {
                builder.BypassUserRoles(userRoles);
                return builder.GetOptions().GetAll<BypassUserRoleOption>();
            };

            if (expectedException != null)
            {
                testFunc.ShouldThrow(expectedException);
                return;
            }

            IEnumerable<BypassUserRoleOption> options = testFunc.ShouldNotThrow();

            options
                .ShouldNotBeNull()
                .ShouldNotBeEmpty();
            options
                .Count()
                .ShouldBe(userRoles.Count());
            userRoles
                .ShouldContain(options.First().Value);
        }

        [Fact]
        public void BypassAllAuthenticatedUsers()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Action testAction = () => builder.BypassAllAuthenticatedUsers();

            testAction.ShouldNotThrow();

            builder.GetOptions()
                .GetSingleOrDefault<BypassAllAuthenticatedUsersOption>()
                .ShouldNotBeNull();
        }

        [Fact]
        public void BypassUrlPath()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Func<BypassUrlPathOption> testFunc = () =>
            {
                builder.BypassUrlPath(new PathString(), StringComparison.Ordinal);
                return builder.GetOptions().GetSingleOrDefault<BypassUrlPathOption>();
            };


            testFunc
                .ShouldNotThrow()
                .ShouldNotBeNull();
        }

        [Theory]
        [MemberData(nameof(GetBypassUrlPathsMembers))]
        public void BypassUrlPaths(PathString[] pathStrings, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
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
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData("txt", null)]
        [InlineData(".txt", null)]
        public void BypassFileExtension(string extension, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Func<BypassFileExtensionOption> testFunc = () =>
            {
                builder.BypassFileExtension(extension);
                return builder.GetOptions().GetSingleOrDefault<BypassFileExtensionOption>();
            };

            if (expectedException != null)
            {
                testFunc.ShouldThrow(expectedException);
            }
            else
            {
                testFunc
                    .ShouldNotThrow()
                    .ShouldNotBeNull()
                    // the method BypassFileExtension removes the dots of the extensions
                    .Value.ShouldBe(extension.StartsWith('.') 
                        ? extension.Substring(1) 
                        : extension);
            }
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData(new string[] { }, typeof(ArgumentException))]
        [InlineData(new string[] { "txt" }, null)]
        [InlineData(new string[] { "txt", "html" }, null)]
        public void BypassFileExtensions(string[] extensions, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Func<IEnumerable<BypassFileExtensionOption>> testFunc = () =>
            {
                builder.BypassFileExtensions(extensions);
                return builder.GetOptions().GetAll<BypassFileExtensionOption>();
            };

            if (expectedException != null)
            {
                testFunc.ShouldThrow(expectedException);
            }
            else
            {
                IEnumerable<BypassFileExtensionOption> options = testFunc
                    .ShouldNotThrow()
                    .ShouldNotBeNull();
                options.ShouldNotBeEmpty();

                if (extensions != null)
                {
                    options.Count()
                        .ShouldBe(extensions.Length);
                    extensions
                        .ShouldContain(options.First().Value);
                }
            }
        }

        [Fact]
        public void UseNoDefaultValues()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Action testAction = () => builder.UseNoDefaultValues();

            testAction.ShouldNotThrow();

            Action assertAction = () => builder.GetOptions();
            assertAction.ShouldThrow<ArgumentException>()
                .Message.ShouldStartWith("No response was specified.");
        }

        [Fact]
        public void FillEmptyOptionsWithDefault()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);

            builder.GetOptions().ShouldNotBeNull()
                .GetAll().ShouldNotBeEmpty();
        }

        [Fact]
        public void GetOptionsTwice_ShouldNotThrow()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Action testAction = () => 
            {
                builder.GetOptions();
                builder.GetOptions();
            };

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void When_DuplicateResponseOption_GetOption_Should_Throw()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder(_pathMapperSvc);
            Action testAction = () =>
            {
                builder.UseNoDefaultValues();
                builder.UseDefaultResponse();
                builder.UseResponse(Encoding.UTF8.GetBytes("test"), ContentType.Text, Encoding.UTF8);
                builder.GetOptions();
            };

            testAction.ShouldThrow<ArgumentException>()
                .Message.ShouldStartWith("More than one response");

        }
    }
}
