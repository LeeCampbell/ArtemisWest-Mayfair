using System;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository
{
    public interface IDailyCompoundedMortgageRepository
    {
        IObservable<bool> IsLoaded { get;}
        IObservable<decimal> MinimumRate { get; }
        IObservable<decimal> MaximumRate { get; }
        void Load();
        decimal GetMinimumMonthlyPayment(decimal principal, decimal term, decimal rate);
    }
}
