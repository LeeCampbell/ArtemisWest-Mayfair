using System;
using ArtemisWest.PropertyInvestment.Calculator.Entities;

namespace ArtemisWest.PropertyInvestment.Calculator.Appender
{
    /// <summary>
    /// This appender calculates and accrues interest daily and charges it to the 
    /// <see cref="DateTransactionList"/> based on a predicate.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Compounding interest daily is a very common way for bank accounts (loans, and 
    /// savings/chequeing) to accrue interest. What is mainly variable is when the interest is 
    /// debited or credited (depending on if the account is a loan or savings).
    /// </para>
    /// By providing a predicate to the appender, the logic for when to charge the accrued interest
    /// is abstracted.
    /// <example>
    /// In this example the interest would be charged on the 1st day of every month.
    /// <code>
    /// <![CDATA[
    /// var rate = 0.05m;
    /// var monthlyInterest = new DailyCompoundedInterest(rate, d => d.Day == 1);
    /// ]]>
    /// </code>
    /// In this example the interest would be charged yearly on the first of January.
    /// <code>
    /// <![CDATA[
    /// var rate = 0.05m;
    /// var yearlyInterest = new DailyCompoundedInterest(rate, d => d.DayOfYear == 1);
    /// ]]></code>
    /// </example>
    /// </remarks>
    /// <seealso cref="YearlyGrowthCalculatedDaily"/>
    internal sealed class DailyCompoundedInterest : IDailyTransactionAppender
    {
        private readonly Predicate<DateTransactionList> _shouldAccrue;
        private readonly decimal _annualInterestRate;
        private decimal _accumulator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyCompoundedInterest"/> class.
        /// </summary>
        /// <param name="annualInterestRate">The annual interest rate.</param>
        /// <param name="isAccuralDate">The is accural date.</param>
        public DailyCompoundedInterest(decimal annualInterestRate, Predicate<DateTime> isAccuralDate)
        {
            Guard.ArgumentNotNull(isAccuralDate, "isAccuralDate");
            _annualInterestRate = annualInterestRate;
            _shouldAccrue = (tranList) => isAccuralDate(tranList.Date);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyCompoundedInterest"/> class.
        /// </summary>
        /// <param name="annualInterestRate">The annual interest rate.</param>
        /// <param name="shouldAccrue">Predicate to indicate if the accumlated interest should appear as a transaction.</param>
        public DailyCompoundedInterest(decimal annualInterestRate, Predicate<DateTransactionList> shouldAccrue)
        {
            Guard.ArgumentNotNull(shouldAccrue, "shouldAccrue");
            _annualInterestRate = annualInterestRate;
            _shouldAccrue = shouldAccrue;
        }

        /// <summary>
        /// Gets the annual interest rate.
        /// </summary>
        /// <value>The annual interest rate.</value>
        public decimal AnnualInterestRate
        {
            get { return _annualInterestRate; }
        }

        #region IDailyTransactionAppender Members

        /// <summary>
        /// Appends transactions to a given transaction list.
        /// </summary>
        /// <param name="dateTransactionList">The list of transaction to append to.</param>
        public void AppendToDailyTransaction(DateTransactionList dateTransactionList)
        {
            var balance = dateTransactionList.CurrentBalance();
            _accumulator += CalculateInterest(balance, dateTransactionList.Date);
            if (_shouldAccrue(dateTransactionList))
            {
                dateTransactionList.Transactions.Add(new Transaction(_accumulator, "Interest"));
                _accumulator = 0;
            }
        }
        #endregion

        private decimal CalculateInterest(decimal balance, DateTime date)
        {
            return (AnnualInterestRate * balance) * (1M / date.DaysInYear());
        }
    }
}