using System;
using ArtemisWest.PropertyInvestment.Calculator.Repository.Entities;

namespace ArtemisWest.PropertyInvestment.Calculator.Repository
{
    public interface IDailyCompoundedMortgageRepository
    {
        IObservable<IMortgageRates> Load();
    }
}
