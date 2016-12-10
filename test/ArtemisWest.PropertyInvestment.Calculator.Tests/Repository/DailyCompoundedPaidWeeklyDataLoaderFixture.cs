using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArtemisWest.PropertyInvestment.Calculator.Repository;
using ArtemisWest.PropertyInvestment.Calculator.Repository.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SevenZip.Compression.LZMA;

//TODO: Add test to actual test some known data. Ensure there is a fair range that it must hit ie: enough to pay, but not so much that it would pay the next bracket.
namespace ArtemisWest.PropertyInvestment.Calculator.Tests.Repository
{
    [TestClass]
    public class DailyCompoundedPaidWeeklyDataLoaderFixture
    {
        private static readonly List<Row> _data;
        static DailyCompoundedPaidWeeklyDataLoaderFixture()
        {
            var sut = new DailyCompoundedPaidWeeklyDataLoader();
            var data = sut.MinimumPayments();
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
                .Select(i => (byte)i)
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

        [TestMethod, Ignore]
        public void Not_a_test_xxx_Generate_the_new_data_structure()
        {
            //TODO: Split the files into Interest rate splits, not term. Ie load 0-8% as fast as possible (most standard rates)
            //TODO: in the ctor create the List<Row> and then AddRange as the rows come in. Will need to come in ordered. 
            //      -Or perhaps just OnNext the lot -
            //      OnNext the New Min-Max Interest Rates
            Console.WriteLine("Total Count : {0}", _data.Count);
            var groupedByRate = _data.GroupBy(row => Math.Round(row.Rate, 2, MidpointRounding.AwayFromZero));
            Console.WriteLine("Group Count : {0}", groupedByRate.Count());
            foreach (var grp in groupedByRate)
            {
                Console.WriteLine("Group {0} Count : {1}", grp.Key, grp.Count());
                SaveGroupToFile(grp);
            }

        }
        private static void SaveGroupToFile(IGrouping<decimal, Row> grouping)
        {
            var minRate = grouping.Min(row => row.Rate);
            var maxRate = grouping.Max(row => row.Rate);//or grouping.Key
            var fileName = string.Format("DailyCompounded_PaidWeekly_Principal1-10m_Rate{0}-{1}.csv",
                minRate,
                maxRate);
            
            var csvLines = from row in grouping
                    orderby row.Rate, row.Principal, row.Term
                    select row.ToCsv();
            var rows = Enumerable.Repeat(Row.CsvHeader, 1)
                                 .Concat(csvLines);

            File.WriteAllLines(fileName, rows);

            //var binaryContents = SevenZipHelper.Decompress(zipConents);
            var bytes = File.ReadAllBytes(fileName);
            var compressed = SevenZipHelper.Compress(bytes);
            File.WriteAllBytes(fileName +".lzma", compressed);
            
        }

        private static IEnumerable<int> Generate(int start, int max, int step)
        {
            for (int i = start; i <= max; i += step)
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
