using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
//using ArtemisWest.PropertyInvestment.Calculator.Appender;

namespace ArtemisWest.PropertyInvestment.Calculator.Entities
{
    //public class Investment : IDailyBalance
    //{
    //    #region Fields
    //    private readonly Range<DateTime> _investmentPeriod;
    //    private readonly IDailyTransactionAppender _dailyTransactionAppender;
    //    #endregion

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="Investment"/> class.
    //    /// </summary>
    //    /// <param name="investmentPeriod">The period the investment is for.</param>
    //    /// <param name="initialValue">The initial value of the investment.</param>
    //    /// <param name="dailyTransactionAppender">The daily transaction appender.</param>
    //    public Investment(Range<DateTime> investmentPeriod, decimal initialValue, IDailyTransactionAppender dailyTransactionAppender)
    //    {
    //        _investmentPeriod = investmentPeriod;
    //        _dailyTransactionAppender = dailyTransactionAppender;
    //        Value = initialValue;
    //    }

    //    /// <summary>
    //    /// Gets the account balance.
    //    /// </summary>
    //    /// <value>The account balance.</value>
    //    public decimal Value { get; private set; }

    //    /// <summary>
    //    /// Gets period the investment is for.
    //    /// </summary>
    //    /// <value>The date range the investment is active.</value>
    //    public Range<DateTime> InvestmentPeriod
    //    {
    //        get { return _investmentPeriod; }
    //    }

    //    #region IDailyBalance Members
    //    /// <summary>
    //    /// Lists the daily balances.
    //    /// </summary>
    //    /// <returns>Returns a collection of daily positions.</returns>
    //    public IEnumerable<Position> ListDailyBalances()
    //    {
    //        var currentDate = InvestmentPeriod.Start;
    //        while (currentDate <= InvestmentPeriod.End)
    //        {
    //            var dailyPosition = new DateTransactionList(currentDate, Value, new Collection<Transaction>());
    //            _dailyTransactionAppender.AppendToDailyTransaction(dailyPosition);

    //            Value += dailyPosition.Transactions.Sum(t => t.Amount);

    //            yield return new Position(currentDate, Value);
    //            currentDate = currentDate.AddDays(1);
    //        }
    //    }
    //    #endregion

    //    public override string ToString()
    //    {
    //        return String.Format("InvestmentPeriod={0:d} to {1:d}; Value={2}", InvestmentPeriod.Start, InvestmentPeriod.End, Value);
    //    }
    //}
}