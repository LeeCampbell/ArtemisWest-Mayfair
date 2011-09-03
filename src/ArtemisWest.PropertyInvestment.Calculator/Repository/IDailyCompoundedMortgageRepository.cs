namespace ArtemisWest.PropertyInvestment.Calculator.Repository
{
    public interface IDailyCompoundedMortgageRepository
    {
        decimal GetMinimumMonthlyPayment(decimal principal, double term, decimal rate);
    }
}
