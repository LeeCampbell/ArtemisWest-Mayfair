using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            var assembly = Assembly.GetExecutingAssembly();
            var dataPrefix = string.Format("{0}.Data.DailyCompounded_PaidWeekly", assembly.GetName().Name);

            var query = from resourcePath in assembly.GetManifestResourceNames()
                            .Where(res => res.StartsWith(dataPrefix))
                            .OrderBy(i => i)
                        let resource = assembly.GetManifestResourceStream(resourcePath)
                        let byteArray = ReadFully(resource)
                        let csvContent = DecompressToString(byteArray)
                        from line in Enumerable.Skip(csvContent.Split(
                                new[] { Environment.NewLine },
                                StringSplitOptions.RemoveEmptyEntries
                                               ), 1) //Skip the header row
                        select Row.New(line);

            _data = new ReadOnlyCollection<Row>(query.ToList());
        }

        public IEnumerable<Row> Data
        {
            get { return _data; }
        }

        private static byte[] ReadFully(Stream input)
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

        private static string DecompressToString(byte[] zipConents)
        {
            var binaryContents = SevenZipHelper.Decompress(zipConents);
            return new UTF8Encoding().GetString(binaryContents);
        }
    }
}