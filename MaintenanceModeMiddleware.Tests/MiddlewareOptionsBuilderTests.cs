using MaintenanceModeMiddleware.Configuration.Builders;
using MaintenanceModeMiddleware.Configuration.Enums;
using MaintenanceModeMiddleware.Configuration.Options;
using Microsoft.AspNetCore.Http;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MaintenanceModeMiddleware.Tests
{
    public class MiddlewareOptionsBuilderTests
    {
        [Fact]
        public void Constructor()
        {
            MiddlewareOptionsBuilder builder = null;
            Action testAction = () => builder = new MiddlewareOptionsBuilder();

            testAction.ShouldNotThrow();

            builder.Options.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(null, default(PathBaseDirectory), typeof(ArgumentNullException))]
        [InlineData("", default(PathBaseDirectory), typeof(ArgumentNullException))]
        [InlineData("test" /* file name without an extension */, default(PathBaseDirectory), typeof(ArgumentException))]
        [InlineData("test.txt", PathBaseDirectory.ContentRootPath, null)]
        [InlineData("test.txt", PathBaseDirectory.WebRootPath, null)]
        public void UseResponseFile(string relativePath, PathBaseDirectory baseDir, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Func<ResponseFileOption> testFunc = () =>
            {
                builder.UseResponseFile(relativePath, baseDir);
                return builder.Options.GetSingleOrDefault<ResponseFileOption>();
            };

            if (expectedException == null)
            {
                ResponseFileOption option = testFunc
                    .ShouldNotThrow()
                    .ShouldNotBeNull();

                option.Value.ShouldNotBeNull();
                option.Value.FilePath.ShouldBe(relativePath);
                option.Value.BaseDir.ShouldBe(baseDir);
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
                    var builder = new MiddlewareOptionsBuilder();
                    builder.UseResponse(response, contentType, encoding);
                    return builder.Options.GetSingleOrDefault<ResponseOption>();
                },
                () =>
                {
                    var builder = new MiddlewareOptionsBuilder();
                    builder.UseResponse(string.IsNullOrEmpty(response) 
                            ? null 
                            : encoding.GetBytes(response),
                        contentType, 
                        encoding);

                    return builder.Options.GetSingleOrDefault<ResponseOption>();
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
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Action testAction = () => builder.UseDefaultResponse();

            testAction.ShouldNotThrow();

            builder.Options
                .GetSingleOrDefault<UseDefaultResponseOption>()
                .ShouldNotBeNull();
        }

        [Theory]
        [InlineData(true, false, false, null)]
        [InlineData(true, true, false, typeof(InvalidOperationException))]
        [InlineData(true, false, true, typeof(InvalidOperationException))]
        [InlineData(false, true, true, typeof(InvalidOperationException))]
        public void ResponseAlreadySpecifiedAssertion(bool useDefaultResponse,
            bool useResponseFile,
            bool useResponse,
            Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Action testAction = () =>
            {
                if (useDefaultResponse)
                {
                    builder.UseDefaultResponse();
                }

                if (useResponseFile)
                {
                    builder.UseResponseFile("test.txt", PathBaseDirectory.ContentRootPath);
                }

                if (useResponse)
                {
                    builder.UseResponse("test", ContentType.Text, Encoding.UTF8);
                }
            };

            if (expectedException != null)
            {
                testAction.ShouldThrow(expectedException);
            }
            else
            {
                testAction.ShouldNotThrow();
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5001)]
        public void Set503RetryAfterInterval(int interval)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Func<Code503RetryIntervalOption> testFunc = () =>
            {
                builder.Set503RetryAfterInterval(interval);
                return builder.Options.GetSingleOrDefault<Code503RetryIntervalOption>();
            };

            Code503RetryIntervalOption option = testFunc.ShouldNotThrow();

            option
                .ShouldNotBeNull()
                .Value.ShouldBe(interval);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentNullException))]
        [InlineData("userName1", null)]
        public void BypassUser(string userName, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Func<BypassUserNameOption> testFunc = () =>
            {
                builder.BypassUser(userName);
                return builder.Options.GetSingleOrDefault<BypassUserNameOption>();
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
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Func<IEnumerable<BypassUserNameOption>> testFunc = () =>
            {
                builder.BypassUsers(userNames);
                return builder.Options.GetAll<BypassUserNameOption>();
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
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Func<BypassUserRoleOption> testFunc = () =>
            {
                builder.BypassUserRole(userRole);
                return builder.Options.GetSingleOrDefault<BypassUserRoleOption>();
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
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Func<IEnumerable<BypassUserRoleOption>> testFunc = () =>
            {
                builder.BypassUserRoles(userRoles);
                return builder.Options.GetAll<BypassUserRoleOption>();
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
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Action testAction = () => builder.BypassAllAuthenticatedUsers();

            testAction.ShouldNotThrow();

            builder.Options
                .GetSingleOrDefault<BypassAllAuthenticatedUsersOption>()
                .ShouldNotBeNull();
        }

        [Fact]
        public void BypassUrlPath()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Func<BypassUrlPathOption> testFunc = () =>
            {
                builder.BypassUrlPath(new PathString(), StringComparison.Ordinal);
                return builder.Options.GetSingleOrDefault<BypassUrlPathOption>();
            };


            testFunc
                .ShouldNotThrow()
                .ShouldNotBeNull();
        }

        [Theory]
        [MemberData(nameof(GetBypassUrlPathsMembers))]
        public void BypassUrlPaths(PathString[] pathStrings, Type expectedException)
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Func<IEnumerable<BypassUrlPathOption>> testFunc = () =>
            {
                builder.BypassUrlPaths(pathStrings, StringComparison.Ordinal);
                return builder.Options.GetAll<BypassUrlPathOption>();
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
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Func<BypassFileExtensionOption> testFunc = () =>
            {
                builder.BypassFileExtension(extension);
                return builder.Options.GetSingleOrDefault<BypassFileExtensionOption>();
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
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Func<IEnumerable<BypassFileExtensionOption>> testFunc = () =>
            {
                builder.BypassFileExtensions(extensions);
                return builder.Options.GetAll<BypassFileExtensionOption>();
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
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Action testAction = () => builder.UseNoDefaultValues();

            testAction.ShouldNotThrow();

            builder.Options
                .GetSingleOrDefault<UseNoDefaultValuesOption>()
                .ShouldNotBeNull();
        }

        [Fact]
        public void FillEmptyOptionsWithDefault()
        {
            MiddlewareOptionsBuilder builder = new MiddlewareOptionsBuilder();
            Action testAction = () => builder.FillEmptyOptionsWithDefault();

            testAction.ShouldNotThrow();

            builder.Options.ShouldNotBeNull()
                .GetAll().ShouldNotBeEmpty();
        }
    }
}
