using System;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository
{
    public interface IDailyCompoundedMortgageRepository
    {
        IObservable<bool> IsLoaded { get;}
        void Load();
        decimal GetMinimumMonthlyPayment(decimal principal, decimal term, decimal rate);
    }
}
