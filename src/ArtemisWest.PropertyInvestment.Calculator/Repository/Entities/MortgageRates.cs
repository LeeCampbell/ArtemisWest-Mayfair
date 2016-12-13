using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository.Entities
{
    internal sealed class MortgageRates : IMortgageRates
    {
        private const decimal RateStep = 0.0005m;
        private const decimal PrincipalStep = 50000m;
        private const decimal MinimumPrincipal = 1000000;
        private const decimal MaximumPrincipal = 9950000;

        //Term --> Rate --> Principal
        private readonly Dictionary<byte, Dictionary<decimal, Dictionary<decimal, decimal>>> _dataGroupsDictionary = new Dictionary<byte, Dictionary<decimal, Dictionary<decimal, decimal>>>();
        private readonly Range<decimal> _range;

        public MortgageRates(IEnumerable<Row> rowSet1)
        {
            var rowSet = rowSet1.ToList();
            foreach (var row in rowSet)
            {
                if (!_dataGroupsDictionary.ContainsKey(row.Term))
                {
                    _dataGroupsDictionary[row.Term] = new Dictionary<decimal, Dictionary<decimal, decimal>>();
                }
                if (!_dataGroupsDictionary[row.Term].ContainsKey(row.Rate))
                {
                    _dataGroupsDictionary[row.Term][row.Rate] = new Dictionary<decimal, decimal>();
                }
                _dataGroupsDictionary[row.Term][row.Rate][row.Principal] = row.MinimumPayment;
            }

            _range = new Range<decimal>(rowSet.Min(r => r.Rate), rowSet.Max(r => r.Rate));
        }

        public bool IsRateValid(decimal annualInterestRate)
        {
            return _range.Contains(annualInterestRate);
        }


        public decimal GetMinimumMonthlyPayment(decimal principal, decimal term, decimal rate)
        {
            if (!_range.Contains(rate))
            {
                throw new ArgumentOutOfRangeException("rate", $"Rate is outside of the supported range of {_range.Start} to {_range.End}");
            }

            byte t = 0 < term && term < 0.5m
                ? (byte)1
                : Convert.ToByte(term);

            return MinPayment(t, principal, rate);
        }

        private decimal MinPayment(byte term, decimal principal, decimal rate)
        {
            if (principal == 0m || rate == 0m)
                return 0m;

            var normailizedRate = RoundUp(rate, RateStep);

            var principalMultiplier = 1.0m;
            var normailizedPrincipal = RoundUp(principal, PrincipalStep);
            while (normailizedPrincipal < MinimumPrincipal)
            {
                normailizedPrincipal = normailizedPrincipal * 10.0m;
                principalMultiplier = principalMultiplier * 10.0m;
            }
            while (normailizedPrincipal > MaximumPrincipal)
            {
                normailizedPrincipal = normailizedPrincipal / 10.0m;
                principalMultiplier = principalMultiplier / 10.0m;
            }

            try
            {
                var normailizedMinPayment = _dataGroupsDictionary[term][normailizedRate][normailizedPrincipal];
                return normailizedMinPayment / principalMultiplier;
            }
            catch (KeyNotFoundException ex)
            {
                throw new ArgumentOutOfRangeException($"Could not find data for arguments MinPayment({term}, {principal}, {rate})", ex);
            }
        }

        private static decimal RoundUp(decimal value, decimal range)
        {
            var modulus = value % range;
            if (modulus != 0)
            {
                var dividend = (int)(value / range);
                return (dividend + 1.0m) * range;
            }
            return value;
        }
    }
}