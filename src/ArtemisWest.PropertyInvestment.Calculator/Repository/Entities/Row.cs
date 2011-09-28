﻿using System;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository.Entities
{
    [System.Diagnostics.DebuggerDisplay("Term={Term}; Principal={Principal}; Rate={Rate}; MinimumPayment={MinimumPayment}")]
    sealed class Row
    {
        public static readonly string CsvHeader = "Term,Principal,Rate,MinimumPayment";

        private readonly byte _term;
        private readonly decimal _principal;
        private readonly decimal _rate;
        private readonly decimal _minimumPayment;

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
                throw new ArgumentException(string.Format("csvLine is invalid : {0}", csvLine), ex);
            }
        }

        public Row(byte term, decimal principal, decimal rate, decimal minimumPayment)
        {
            _term = term;
            _principal = principal;
            _rate = rate;
            _minimumPayment = minimumPayment;
        }

        public byte Term
        {
            get { return _term; }
        }

        public decimal Principal
        {
            get { return _principal; }
        }

        public decimal Rate
        {
            get { return _rate; }
        }

        public decimal MinimumPayment
        {
            get { return _minimumPayment; }
        }

        public string ToCsv()
        {
            return string.Format("{0},{1},{2},{3}", Term, Principal, Rate, MinimumPayment);
        }
    }
}