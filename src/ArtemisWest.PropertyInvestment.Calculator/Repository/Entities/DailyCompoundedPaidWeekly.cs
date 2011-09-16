using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SevenZip.Compression.LZMA;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository.Entities
{
    sealed class DailyCompoundedPaidWeekly
    {
        private readonly ReadOnlyCollection<Row> _data;

        public DailyCompoundedPaidWeekly()
        {
            using (new Timer("DailyCompoundedPaidWeekly.ctor()"))
            {
                var assembly = Assembly.GetExecutingAssembly();
                var dataPrefix = string.Format("{0}.Data.DailyCompounded_PaidWeekly", assembly.GetName().Name);

                var csvFiles = assembly.GetManifestResourceNames()
                    .Where(res => res.StartsWith(dataPrefix))
                    //.OrderBy(i => i)
                    .Select(assembly.GetManifestResourceStream)
                    .Select(ReadFully)
                    .Select(DecompressToString);

                var query = from csvContent in csvFiles
                            from line in Enumerable.Skip(csvContent.Split(
                                new[] { Environment.NewLine },
                                StringSplitOptions.RemoveEmptyEntries
                                                             ), 1)
                            //Skip the header row
                            select Row.New(line);

                //var query = from resourcePath in assembly.GetManifestResourceNames()
                //                .Where(res => res.StartsWith(dataPrefix))
                //                .OrderBy(i => i)
                //            let resource = assembly.GetManifestResourceStream(resourcePath)
                //            let byteArray = ReadFully(resource)
                //            let csvContent = DecompressToString(byteArray)
                //            from line in Enumerable.Skip(csvContent.Split(
                //                new[] { Environment.NewLine },
                //                StringSplitOptions.RemoveEmptyEntries
                //                                             ), 1)
                //            //Skip the header row
                //            select Row.New(line);

                IList<Row> rows;
                //using (new Timer("query.ToList()"))
                //{
                //    rows = query.ToList();    
                //}
                using (new Timer("query.ToArray()"))
                {
                    rows = query.ToArray();
                }
                
                _data = new ReadOnlyCollection<Row>(rows);
            }
        }

        public IEnumerable<Row> Data
        {
            get { return _data; }
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