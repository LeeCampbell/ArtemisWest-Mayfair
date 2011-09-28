using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using ArtemisWest.PropertyInvestment.Calculator.Repository.Entities;
using SevenZip.Compression.LZMA;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository
{
    sealed class DailyCompoundedPaidWeeklyDataLoader
    {
        private readonly IObservable<IEnumerable<Row>> _minimumPayments;

        //TODO: Split the files into Interest rate splits, not term. Ie load 0-8% as fast as possible (most standard rates)
        //TODO: in the ctor create the List<Row> and then AddRange as the rows come in. Will need to come in ordered. 
        //      -Or perhaps just OnNext the lot -
        //      OnNext the New Min-Max Interest Rates


        public DailyCompoundedPaidWeeklyDataLoader()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var dataPrefix = string.Format("{0}.Data.DailyCompounded_PaidWeekly", assembly.GetName().Name);

            _minimumPayments = Observable.Create<IEnumerable<Row>>(
                o =>
                {
                    var resourceNames = assembly.GetManifestResourceNames()
                        .Where(res => res.StartsWith(dataPrefix))
                        .OrderBy(i => i);

                    var csvFiles = resourceNames
                        .Select(assembly.GetManifestResourceStream)
                        .Select(ReadFully)
                        .Select(DecompressToString);

                    var rows = from file in csvFiles
                               select CsvFileToRows(file);

                    return rows
                        .ToObservable()
                        .Subscribe(o);
                });
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

        public IObservable<IEnumerable<Row>> MinimumPayments
        {
            get { return _minimumPayments; }
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