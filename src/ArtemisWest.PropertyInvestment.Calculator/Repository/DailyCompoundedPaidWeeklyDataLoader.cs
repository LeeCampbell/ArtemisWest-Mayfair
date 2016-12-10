using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ArtemisWest.PropertyInvestment.Calculator.Repository.Entities;
using SevenZip.Compression.LZMA;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository
{
    sealed class DailyCompoundedPaidWeeklyDataLoader
    {
        private static readonly string DataPrefix;
        private static readonly Assembly ExecutingAsssembly;

        //TODO: Split the files into Interest rate splits, not term. Ie load 0-8% as fast as possible (most standard rates)
        //TODO: in the ctor create the List<Row> and then AddRange as the rows come in. Will need to come in ordered. 
        //      -Or perhaps just OnNext the lot -
        //      OnNext the New Min-Max Interest Rates
        static DailyCompoundedPaidWeeklyDataLoader()
        {
            ExecutingAsssembly = Assembly.GetExecutingAssembly();
            DataPrefix = $"{ExecutingAsssembly.GetName().Name}.Data.DailyCompounded_PaidWeekly";
        }

        private static IEnumerable<Row> CsvFileToRows(string content)
        {
            return from line in content.Split
                       (
                            new[] { Environment.NewLine },
                            StringSplitOptions.RemoveEmptyEntries
                       ).Skip(1) //Skip the header row
                   select Row.New(line);
        }

        public IEnumerable<Row> MinimumPayments()
        {
            var resourceNames = ExecutingAsssembly.GetManifestResourceNames()
                        .Where(res => res.StartsWith(DataPrefix))
                        .OrderBy(i => i);

            var csvFiles = resourceNames
                .Select(ExecutingAsssembly.GetManifestResourceStream)
                .Select(ReadFully)
                .Select(DecompressToString);

            var pages = from file in csvFiles
                       select CsvFileToRows(file);

            return pages.SelectMany(page=>page);
        }

        private static byte[] ReadFully(Stream input)
        {
            using (new Timer("ReadFully"))
            {
                var buffer = new byte[16 * 1024];
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return ms.ToArray();
                }
            }
        }

        private static string DecompressToString(byte[] zipConents)
        {
            using (new Timer("DecompressToString"))
            {
                var binaryContents = SevenZipHelper.Decompress(zipConents);
                return new UTF8Encoding().GetString(binaryContents);
            }
        }
    }
}