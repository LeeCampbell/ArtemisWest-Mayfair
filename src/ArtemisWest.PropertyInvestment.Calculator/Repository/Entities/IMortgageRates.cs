namespace ArtemisWest.PropertyInvestment.Calculator.Repository.Entities
{
    public interface IMortgageRates
    {
        bool IsRateValid(decimal annualInterestRate);
        decimal GetMinimumMonthlyPayment(decimal principal, decimal term, decimal rate);
    }
}