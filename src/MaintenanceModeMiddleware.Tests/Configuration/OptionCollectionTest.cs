using MaintenanceModeMiddleware.Configuration;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace MaintenanceModeMiddleware.Tests.Configuration
{
    public class OptionCollectionTest
    {
        [Fact]
        public void ConstructEmpty()
        {
            Action testAction = () => new OptionCollection();

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void ConstructNotEmpty()
        {
            Action testAction = () => new OptionCollection(new List<ISerializableOption>
            {
                Substitute.For<ISerializableOption>()
            });

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void Add()
        {
            OptionCollection collection = new OptionCollection();
            ISerializableOption testOption = Substitute.For<ISerializableOption>();
            Action testAction = () => collection.Add(testOption);

            testAction.ShouldNotThrow();
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(2, typeof(InvalidOperationException))]
        public void GetSingleOrDefault(int numberOfSameEntries, Type expectedException)
        {
            OptionCollection collection = GetOptionCollection(numberOfSameEntries);
            Func<ISerializableOption> testFunc = () => collection.GetSingleOrDefault<ISerializableOption>();

            if (expectedException == null)
            {
                testFunc
                    .ShouldNotThrow()
                    .ShouldNotBeNull();
            }
            else
            {
                testFunc.ShouldThrow(expectedException);
            }
        }

        [Fact]
        public void GetAll()
        {
            OptionCollection collection = GetOptionCollection(2);
            Func<IEnumerable<IOption>> testFunc = () => collection.GetAll();

            testFunc
                .ShouldNotThrow()
                .ShouldNotBeNull()
                .ShouldNotBeEmpty();
        }

        [Fact]
        public void GetAllT()
        {
            OptionCollection collection = GetOptionCollection(2);
            ISerializableOption testOption = Substitute.For<ISerializableOption>();
            Func<IEnumerable<ISerializableOption>> testFunc = () => collection.GetAll<ISerializableOption>();

            testFunc
                .ShouldNotThrow()
                .ShouldNotBeNull()
                .ShouldNotBeEmpty();
        }



        [Fact]
        public void Clear()
        {
            OptionCollection collection = GetOptionCollection(2);
            Action testAction = () => collection.Clear();

            testAction.ShouldNotThrow();

            collection.GetAll<ISerializableOption>().ShouldBeEmpty();
        }

        [Fact]
        public void AddClearT()
        {
            OptionCollection collection = GetOptionCollection(2);
            Action testAction = () => collection.Clear<ISerializableOption>();

            testAction.ShouldNotThrow();

            collection.GetAll<ISerializableOption>().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Any(bool empty)
        {
            OptionCollection collection = GetOptionCollection(empty ? 0 : 1);
            Action testAction = () =>
            {
                if (empty)
                {
                    collection.Any<ISerializableOption>().ShouldBeFalse();
                }
                else
                {

                    collection.Any<ISerializableOption>().ShouldBeTrue();
                }
            };

            testAction.ShouldNotThrow();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Count(int numberOfEntries)
        {
            OptionCollection collection = GetOptionCollection(numberOfEntries);
            Action testAction = () =>
            {
                    collection.Count.ShouldBe(numberOfEntries);
            };

            testAction.ShouldNotThrow();
        }

        private OptionCollection GetOptionCollection(int numOfEntries = 1)
        {
            List<ISerializableOption> options = new List<ISerializableOption>();

            for (int i = 0; i < numOfEntries; i++)
            {
                options.Add(Substitute.For<ISerializableOption>());
            }

            return new OptionCollection(options);
        }
    }
}
