﻿using MaintenanceModeMiddleware.Configuration;
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
            Action testAction = () => new OptionCollection(new List<IOption>
            {
                Substitute.For<IOption>()
            });

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void Add()
        {
            OptionCollection collection = new OptionCollection();
            IOption testOption = Substitute.For<IOption>();
            Action testAction = () => collection.Add(testOption);

            testAction.ShouldNotThrow();
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(2, typeof(InvalidOperationException))]
        public void GetSingleOrDefault(int numberOfSameEntries, Type expectedException)
        {
            OptionCollection collection = GetOptionCollection(numberOfSameEntries);
            Func<IOption> testFunc = () => collection.GetSingleOrDefault<IOption>();

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
            IOption testOption = Substitute.For<IOption>();
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
            IOption testOption = Substitute.For<IOption>();
            Func<IEnumerable<IOption>> testFunc = () => collection.GetAll<IOption>();

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

            collection.GetAll<IOption>().ShouldBeEmpty();
        }

        [Fact]
        public void AddClearT()
        {
            OptionCollection collection = GetOptionCollection(2);
            Action testAction = () => collection.Clear<IOption>();

            testAction.ShouldNotThrow();

            collection.GetAll<IOption>().ShouldBeEmpty();
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
                    collection.Any<IOption>().ShouldBeFalse();
                }
                else
                {

                    collection.Any<IOption>().ShouldBeTrue();
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
                options.Add(Substitute.For<IOption>());
            }

            return new OptionCollection(options);
        }
    }
}