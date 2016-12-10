using System;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository.Entities
{
    [System.Diagnostics.DebuggerDisplay("Term={Term}; Principal={Principal}; Rate={Rate}; MinimumPayment={MinimumPayment}")]
    internal sealed class Row
    {
        public static readonly string CsvHeader = "Term,Principal,Rate,MinimumPayment";

        public static Row New(string csvLine)
        {
            var columns = csvLine.Split(',');
            try
            {
                return new Row(
                    byte.Parse(columns[0]),
                    decimal.Parse(columns[1]),
                    decimal.Parse(columns[2]),
                    decimal.Parse(columns[3])
                    );
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"csvLine is invalid : {csvLine}", ex);
            }
        }

        public Row(byte term, decimal principal, decimal rate, decimal minimumPayment)
        {
            Term = term;
            Principal = principal;
            Rate = rate;
            MinimumPayment = minimumPayment;
        }

        public byte Term { get; }

        public decimal Principal { get; }

        public decimal Rate { get; }

        public decimal MinimumPayment { get; }

        public string ToCsv()
        {
            return $"{Term},{Principal},{Rate},{MinimumPayment}";
        }
    }
}