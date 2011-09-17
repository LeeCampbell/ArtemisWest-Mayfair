using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ArtemisWest.PropertyInvestment.Calculator.Repository;
using ArtemisWest.PropertyInvestment.Calculator.Repository.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

//TODO: Add test to actual test some known data. Ensure there is a fair range that it must hit ie: enough to pay, but not so much that it would pay the next bracket.
namespace ArtemisWest.PropertyInvestment.Calculator.Tests
{
    [TestClass]
    public class DailyCompoundedPaidWeeklyDataLoaderFixture
    {
        private static readonly List<Row> _data;
        static DailyCompoundedPaidWeeklyDataLoaderFixture()
        {
            var sut = new DailyCompoundedPaidWeeklyDataLoader();
            var data = sut.MinimumPayments.ToEnumerable();
            _data = new List<Row>(data);    //Big slow blocking call.
        }

        [TestMethod]
        public void Should_return_terms_from_1_to_30()
        {
            var actual = _data.Select(row => row.Term)
                .OrderBy(i => i)
                .Distinct()
                .ToList();
            var expected = Enumerable.Range(1, 30)
                .Select(i=>(byte)i)
                .ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Should_return_principals_from_1M_to_9M950Thousand()
        {
            var actual = _data.Select(row => row.Principal)
                .OrderBy(i => i)
                .Distinct()
                .ToList();
            var expected = Generate(1000000, 9950000, 50000)
                .Select(Convert.ToDecimal)
                .ToList();

            Assert.AreEqual(expected.Count, actual.Count);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Should_return_Rates_to_25pc_in_50ths()
        {
            var actual = _data.Select(row => row.Rate)
                .OrderBy(i => i)
                .Distinct()
                .ToList();
            var expected = Generate(0.0005m, 0.25m, 0.0005m)
                .ToList();

            Assert.AreEqual(expected.Count, actual.Count);
            CollectionAssert.AreEqual(expected, actual);
        }

        private static IEnumerable<int> Generate(int start, int max, int step)
        {
            for (int i = start; i <= max; i+=step)
            {
                yield return i;
            }
        }

        private static IEnumerable<decimal> Generate(decimal start, decimal max, decimal step)
        {
            for (decimal i = start; i <= max; i += step)
            {
                yield return i;
            }
        }
    }
}
