using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ArtemisWest.Mayfair.Infrastructure;
using ArtemisWest.PropertyInvestment.Calculator.Repository.Entities;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository
{
    public sealed class DailyCompoundedMortgageRepository : IDailyCompoundedMortgageRepository
    {
        private readonly ISchedulerProvider _schedulerProvider;
        private const decimal RateStep = 0.0005m;
        private const decimal PrincipalStep = 50000m;

        //Term --> Rate --> Principal
        private static readonly Dictionary<byte, Dictionary<decimal, Dictionary<decimal, decimal>>> _dataGroupsDictionary = new Dictionary<byte, Dictionary<decimal, Dictionary<decimal, decimal>>>();
        private const decimal MinimumPrincipal = 1000000;
        private const decimal MaximumPrincipal = 9950000;

        private static decimal _minimumRate;
        private static decimal _maximumRate;

        private static int _isLoading;
        private static readonly BehaviorSubject<bool> _isLoaded = new BehaviorSubject<bool>(false);
        private static readonly BehaviorSubject<decimal> _minimumRateSubject = new BehaviorSubject<decimal>(0m);
        private static readonly BehaviorSubject<decimal> _maximumRateSubject = new BehaviorSubject<decimal>(0m);

        public DailyCompoundedMortgageRepository(ISchedulerProvider schedulerProvider)
        {
            _schedulerProvider = schedulerProvider;
        }

        #region Implementation of IMortgageRepository

        public IObservable<bool> IsLoaded
        {
            get { return _isLoaded.AsObservable(); }
        }

        public IObservable<decimal> MinimumRate
        {
            get { return _minimumRateSubject.AsObservable(); }
        }

        public IObservable<decimal> MaximumRate
        {
            get { return _maximumRateSubject.AsObservable(); }
        }

        public void Load()
        {
            if (_isLoaded.Value) return;
            if (System.Threading.Interlocked.CompareExchange(ref _isLoading, 1, 0) != 0)
            {
                return;
            }

            _schedulerProvider.TaskPool.Schedule(() =>
                {
                    using (new Timer("Load"))
                    {
                        var dailyCompoundedPaidWeekly = new DailyCompoundedPaidWeeklyDataLoader();
                        dailyCompoundedPaidWeekly.MinimumPayments
                            .Subscribe(LoadDataSet, DataLoadCompleted);
                    }
                });
        }

        public decimal GetMinimumMonthlyPayment(decimal principal, decimal term, decimal rate)
        {
            if (rate < _minimumRate || rate > _maximumRate)
            {
                throw new ArgumentOutOfRangeException("rate", $"Rate is outside of the supported range of {_minimumRate} to {_maximumRate}");
            }

            byte t = term < 0.5m && term > 0
                         ? (byte)1
                         : Convert.ToByte(term);

            return MinPayment(t, principal, rate);
        }

        #endregion

        private static void DataLoadCompleted()
        {
            _isLoaded.OnNext(true);
            Debug.WriteLine("DailyCompoundedMortgageRepository is now Loaded");
        }

        private static void LoadDataSet(IEnumerable<Row> rowSet1)
        {
            var rowSet = rowSet1.ToList();
            var minimumRate = _minimumRate;
            var maximumRate = _maximumRate;
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
                if (row.Rate < minimumRate) minimumRate = row.Rate;
                if (row.Rate > maximumRate) maximumRate = row.Rate;
            }

            if (minimumRate < _minimumRate)
            {
                _minimumRate = minimumRate;
                _minimumRateSubject.OnNext(_minimumRate);
            }
            if (maximumRate > _maximumRate)
            {
                _maximumRate = maximumRate;
                _maximumRateSubject.OnNext(_maximumRate);
            }
        }

        private static decimal MinPayment(byte term, decimal principal, decimal rate)
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