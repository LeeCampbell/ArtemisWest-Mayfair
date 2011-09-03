using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ArtemisWest.PropertyInvestment.Calculator.Repository.Entities;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository
{
    public sealed class DailyCompoundedMortgageRepository : IDailyCompoundedMortgageRepository
    {
        private const decimal RateStep = 0.0005m;
        private const decimal PrincipalStep = 50000m;
        private readonly Dictionary<byte, Dictionary<decimal, Dictionary<decimal, decimal>>> _dataGroupsDictionary;
        private decimal _minimumPrincipal;
        private decimal _maximumPrincipal;
        private bool _isLoading;
        private bool _isLoaded;

        public DailyCompoundedMortgageRepository()
        {
            //Term --> Rate-->Principal
            _dataGroupsDictionary = new Dictionary<byte, Dictionary<decimal, Dictionary<decimal, decimal>>>();
        }

        public IObservable<Unit> Load()
        {
            return Observable.Create<Unit>(
                o =>
                {
                    if (_isLoading || _isLoaded)
                        throw new InvalidOperationException("Load can only be called once");
                    _isLoading = true;

                    var dailyCompoundedPaidWeekly = new DailyCompoundedPaidWeekly();
                    foreach (var row in dailyCompoundedPaidWeekly.Data)
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

                    //Validate expectations about data
                    _dataGroupsDictionary.Keys
                        .OrderBy(term => term)
                        .Aggregate(0,
                            (last, current) =>
                            {
                                if (current - last != 1)
                                {
                                    throw new InvalidDataException(
                                        string.Format(
                                            "Expecting all Term values to step by 1 year. Values {0} and {1} do not meet this expectation.",
                                            last, current));
                                }
                                return current;
                            });

                    dailyCompoundedPaidWeekly.Data
                        .Select(row => row.Rate)
                        .Distinct()
                        .OrderBy(rate => rate)
                        .Aggregate(0.0m,
                            (last, current) =>
                            {
                                if (current - last != RateStep)
                                {
                                    throw new InvalidDataException(
                                        string.Format(
                                            "Expecting all interest rates to step by {2}. Values {0} and {1} do not meet this expectation.",
                                            last, current, RateStep));
                                }
                                return current;
                            });

                    var principals = dailyCompoundedPaidWeekly.Data
                        .Select(row => row.Principal)
                        .Distinct()
                        .OrderBy(principal => principal);
                    _minimumPrincipal = principals.Min();
                    _maximumPrincipal = principals.Max();

                    principals
                        .Aggregate(1000000m - PrincipalStep,
                            (last, current) =>
                            {
                                if (current - last != PrincipalStep)
                                {
                                    throw new InvalidDataException(
                                        string.Format(
                                            "Expecting all interest rates to step by {2}. Values {0} and {1} do not meet this expectation.",
                                            last, current, PrincipalStep));
                                }
                                return current;
                            });
                    _isLoaded = true;
                    o.OnCompleted();
                    Debug.WriteLine("DailyCompoundedMortgageRepository is now Loaded");
                    return Disposable.Empty;
                });
        }

        #region Implementation of IMortgageRepository

        public decimal GetMinimumMonthlyPayment(decimal principal, double term, decimal rate)
        {
            if (!_isLoaded)
                throw new InvalidOperationException("Load is not complete");

            byte t = term < 0.5 && term > 0
                         ? (byte)1
                         : Convert.ToByte(term);

            return MinPayment(t, principal, rate);
        }
        #endregion

        private decimal MinPayment(byte term, decimal principal, decimal rate)
        {
            if (principal == 0m || rate == 0m)
                return 0m;

            var normailizedRate = RoundUp(rate, RateStep);

            var principalMultiplier = 1.0m;
            var normailizedPrincipal = RoundUp(principal, PrincipalStep);
            while (normailizedPrincipal < _minimumPrincipal)
            {
                normailizedPrincipal = normailizedPrincipal * 10.0m;
                principalMultiplier = principalMultiplier * 10.0m;
            }
            while (normailizedPrincipal > _maximumPrincipal)
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
                throw new ArgumentOutOfRangeException(string.Format("Could not find data for arguments MinPayment({0}, {1}, {2})", term, principal, rate), ex);
            }
        }

        public static decimal RoundUp(decimal value, decimal range)
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