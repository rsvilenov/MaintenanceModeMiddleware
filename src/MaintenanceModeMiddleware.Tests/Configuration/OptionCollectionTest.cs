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
        public void DefaultConstructor_Call_ShouldNotThrow()
        {
            Action testAction = () => new OptionCollection();

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void ParametrizedConstructor_CallWithOptionList_ShouldNotThrow()
        {
            Action testAction = () => new OptionCollection(new List<ISerializableOption>
            {
                Substitute.For<ISerializableOption>()
            });

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void Add_WithAnOption_ShouldNotThrow()
        {
            OptionCollection collection = new OptionCollection();
            ISerializableOption testOption = Substitute.For<ISerializableOption>();
            Action testAction = () => collection.Add(testOption);

            testAction.ShouldNotThrow();
        }

        [Fact]
        public void GetSingleOrDefault_WithOnePresentEntry_ShouldSucceed()
        {
            const int numberOfSameEntries = 1;
            OptionCollection collection = GetOptionCollection(numberOfSameEntries);

            ISerializableOption option = collection.GetSingleOrDefault<ISerializableOption>();

            option
                .ShouldNotBeNull();
        }

        [Fact]
        public void GetSingleOrDefault_WithTwoPresentEntriesOfAType_ShouldThrowInvalidOperationException()
        {
            const int numberOfSameEntries = 2;
            OptionCollection collection = GetOptionCollection(numberOfSameEntries);
            Func<ISerializableOption> testFunc = () => collection.GetSingleOrDefault<ISerializableOption>();

            testFunc.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void GetAll_Call_ShouldSucceed()
        {
            OptionCollection collection = GetOptionCollection(2);
            
            IEnumerable<IOption> options = collection.GetAll();

            options
                .ShouldNotBeNull()
                .ShouldNotBeEmpty();
        }

        [Fact]
        public void GetAllT_Call_ShouldSucceed()
        {
            OptionCollection collection = GetOptionCollection(2);
            
            IEnumerable<IOption> options = collection.GetAll<ISerializableOption>();

            options
                .ShouldNotBeNull()
                .ShouldNotBeEmpty();
        }



        [Fact]
        public void Clear_Call_ShouldSucceed()
        {
            OptionCollection collection = GetOptionCollection(2);
            
            collection.Clear();

            collection.GetAll<ISerializableOption>()
                .ShouldBeEmpty();
        }

        [Fact]
        public void AddClearT_Call_ShouldSucceed()
        {
            OptionCollection collection = GetOptionCollection(2);
            
            collection.Clear<ISerializableOption>();

            collection.GetAll<ISerializableOption>()
                .ShouldBeEmpty();
        }

        [Fact]
        public void Any_WhenEntriesAreNotPresent_ShouldReturnFalse()
        {
            OptionCollection collection = GetOptionCollection(0);

            bool result = collection.Any<ISerializableOption>();

            result
                .ShouldBeFalse();
        }

        [Fact]
        public void Any_WhenEntriesArePresent_ShouldReturnTrue()
        {
            OptionCollection collection = GetOptionCollection(1);
            
            bool result = collection.Any<ISerializableOption>();

            result
                .ShouldBeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Count_Call_ShouldReturnCorrectCount(int numberOfEntries)
        {
            OptionCollection collection = GetOptionCollection(numberOfEntries);
            
            int count = collection.Count;

            count.ShouldBe(numberOfEntries);
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
