using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ArtemisWest.PropertyInvestment.Calculator.Repository.Entities;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository
{
    public sealed class DailyCompoundedMortgageRepository : IDailyCompoundedMortgageRepository
    {
        private const decimal RateStep = 0.0005m;
        private const decimal PrincipalStep = 50000m;

        //Term --> Rate --> Principal
        private static readonly Dictionary<byte, Dictionary<decimal, Dictionary<decimal, decimal>>> _dataGroupsDictionary = new Dictionary<byte, Dictionary<decimal, Dictionary<decimal, decimal>>>();
        private static decimal _minimumPrincipal;
        private static decimal _maximumPrincipal;
        private static int _isLoading;
        private static readonly BehaviorSubject<bool> _isLoaded = new BehaviorSubject<bool>(false);

        public IObservable<bool> IsLoaded
        {
            get { return _isLoaded.AsObservable(); }
        }

        public void Load()
        {
            if(IsLoaded.First()) return;
            if(System.Threading.Interlocked.CompareExchange(ref _isLoading, 1, 0) != 0)
            {
                return;
            }

            Scheduler.TaskPool.Schedule(() =>
                                     {
                                         using (new Timer("Load"))
                                         {
                                             var dailyCompoundedPaidWeekly = new DailyCompoundedPaidWeekly();
                                             
                                             using (new Timer("LoadDataGroup"))
                                                LoadDataGroups(dailyCompoundedPaidWeekly);
                                             
                                             using (new Timer("ValidateData"))
                                                ValidateData(dailyCompoundedPaidWeekly);
                                             
                                             Debug.WriteLine("DailyCompoundedMortgageRepository is now Loaded");
                                             _isLoaded.OnNext(true);
                                         }
                                     });
        }

        private static void LoadDataGroups(DailyCompoundedPaidWeekly dailyCompoundedPaidWeekly)
        {
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
        }

        private static void ValidateData(DailyCompoundedPaidWeekly dailyCompoundedPaidWeekly)
        {
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
            _minimumPrincipal = principals.Min();
            _maximumPrincipal = principals.Max();
        }

        #region Implementation of IMortgageRepository

        public decimal GetMinimumMonthlyPayment(decimal principal, double term, decimal rate)
        {
            if (!IsLoaded.First())
                throw new InvalidOperationException("Load is not complete");

            byte t = term < 0.5 && term > 0
                         ? (byte)1
                         : Convert.ToByte(term);

            return MinPayment(t, principal, rate);
        }
        #endregion

        private static decimal MinPayment(byte term, decimal principal, decimal rate)
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