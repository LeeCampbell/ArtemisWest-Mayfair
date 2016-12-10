using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using ArtemisWest.PropertyInvestment.Calculator.Repository.Entities;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository
{
    public sealed class DailyCompoundedMortgageRepository : IDailyCompoundedMortgageRepository
    {
        private readonly SingleAssignmentDisposable _cacheConnection = new SingleAssignmentDisposable();
        private readonly IObservable<MortgageRates> _loadRates;

        public DailyCompoundedMortgageRepository()
        {
            _loadRates = Observable.Create<MortgageRates>(o =>
                {
                    var dailyCompoundedPaidWeekly = new DailyCompoundedPaidWeeklyDataLoader();
                    var paymentRates = dailyCompoundedPaidWeekly.MinimumPayments();
                    o.OnNext(new MortgageRates(paymentRates));
                    return Disposable.Empty;
                })
                .Replay(1)
                .LazyConnect(_cacheConnection);
        }

        public IObservable<IMortgageRates> Load()
        {
            return _loadRates;
        }
    }

    
    public static class ObservableExtensions
    {
        public static IObservable<T> LazyConnect<T>(this IConnectableObservable<T> source, SingleAssignmentDisposable connection)
        {
            int isConnected = 0;
            return Observable.Create<T>(
                o =>
                {
                    var subscription = source.Subscribe(o);
                    var isDisconnected = Interlocked.CompareExchange(ref isConnected, 1, 0) == 0;
                    if (isDisconnected)
                        connection.Disposable = source.Connect();
                    return subscription;
                });
        }
    }
}