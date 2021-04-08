using MaintenanceModeMiddleware.Configuration;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace MaintenanceModeMiddleware.Tests
{
    public class OptionCollectionTests
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
            Action testAction = () => new OptionCollection(new List<IOption>
            {
                new TestOption()
            });

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void Add()
        {
            OptionCollection collection = new OptionCollection();
            IOption testOption = new TestOption();
            Action testAction = () => collection.Add(testOption);

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void Get()
        {
            OptionCollection collection = GetOptionCollection(2);
            Action testAction = () =>
            {
                collection.Get<TestOption>().ShouldNotBeNull();
            };

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void GetAll()
        {
            OptionCollection collection = GetOptionCollection(2);

            IOption testOption = new TestOption();
                Action testAction = () =>
                {
                    collection.GetAll<TestOption>().ShouldNotBeNull();
                    collection.GetAll<TestOption>().ShouldNotBeEmpty();
                    collection.GetAll().ShouldNotBeNull();
                    collection.GetAll().ShouldNotBeEmpty();
                };

            testAction.ShouldNotThrow();
        }

        

        [Fact]
        public void Clear()
        {
            OptionCollection collection = GetOptionCollection(2);
            Action testAction = () => collection.Clear();

            testAction.ShouldNotThrow();

            collection.GetAll<TestOption>().ShouldBeEmpty();
        }

        [Fact]
        public void AddClearT()
        {
            OptionCollection collection = GetOptionCollection(2);
            Action testAction = () => collection.Clear<IOption>();

            testAction.ShouldNotThrow();

            collection.GetAll<TestOption>().ShouldBeEmpty();
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
                    collection.Any<TestOption>().ShouldBeFalse();
                }
                else
                {

                    collection.Any<TestOption>().ShouldBeTrue();
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
            List<IOption> options = new List<IOption>();

            for (int i = 0; i < numOfEntries; i++)
            {
                options.Add(new TestOption());
            }

            return new OptionCollection(options);
        }

        private class TestOption : IOption
        {
            public bool IsDefault => throw new NotImplementedException();
            public string TypeName => throw new NotImplementedException();

            public string GetStringValue()
            {
                throw new NotImplementedException();
            }

            public void LoadFromString(string str)
            {
                throw new NotImplementedException();
            }
        }

    }
}
