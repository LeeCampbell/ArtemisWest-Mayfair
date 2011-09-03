using System;
using System.Globalization;
using ArtemisWest.PropertyInvestment.Calculator.Entities;

namespace ArtemisWest.PropertyInvestment.Calculator.Appender
{
    /// <summary>
    /// This implementation of <see cref="IDailyTransactionAppender"/> appends daily transactions 
    /// that reflect a yearly growth spread evenly across the year. 
    /// </summary>
    /// <remarks>
    /// This appender will model the simulated effect that growth would have. This can be useful
    /// for example in calculations such as captial growth. Captial Growth is generally thought of
    /// as an annual increase. This appender allows that gradual increase to be modeled without 
    /// having sharp jumps in value at yearly intervals.
    /// <example>
    /// For example if an asset was to grow 10% per-annum over 1 year, this appender will
    /// model that gradual increase so that after one calendar yeat the asset will be 10%
    /// larger than its original value. 
    /// </example>
    /// If the desired effect is to show a sharp change in value at yearly intervals then the
    /// <see cref="DailyCompoundedInterest"/> appender is the correct appender to use. Provide 
    /// the <see cref="DailyCompoundedInterest"/> instance with a predicate that only evaluates to
    /// true on a yearly basis.
    /// 
    /// <para>
    /// Also note that this class would not produce the same output as a compound interest appender
    /// such as <see cref="DailyCompoundedInterest"/>, even if it was provided with a predicate to 
    /// append daily. Where a growth rate of 10% for this appender will always result in a value 
    /// 10% greater after one year (ie initialValue * 1.10); a compounding interest 
    /// calculation is (initialValue * (1+ interest/partsPerYear)^partsPerYear) so for 10% per annum
    /// interest compounded and accured daily the calculation would be 
    /// (initialValue * (1 + 0.10/365)^365) which can be simplified to approximately 
    /// (initialValue * 1.10516). Comparing the two formulae you can see that the daily compounded 
    /// version returns approximately 0.5% more: (initialValue * 1.10) vs. (initialValue * 1.10516)
    /// </para>
    /// </remarks>
    /// <seealso cref="DailyCompoundedInterest"/>
    internal class YearlyGrowthCalculatedDaily : IDailyTransactionAppender
    {
        private readonly decimal _annualGrowthRate;
        private readonly decimal _dailyRate;
        private readonly decimal _dailyRateLeapYear;
        private readonly Predicate<DateTime> _isInRange;

        /// <summary>
        /// Creates a new instance of <see cref="YearlyGrowthCalculatedDaily"/>.
        /// </summary>
        /// <param name="annualGrowthRate">The grow rate that occurs yearly.</param>
        /// <remarks>
        /// A value of 3 for the <paramref name="annualGrowthRate"/> would cause growth of 300%. 
        /// Applied to an initial value of 100, the value after 1 year would be 400. For growth of
        /// 10% then a value of 0.1 should be provided for <paramref name="annualGrowthRate"/>.
        /// </remarks>
        public YearlyGrowthCalculatedDaily(decimal annualGrowthRate)
            : this(annualGrowthRate, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="YearlyGrowthCalculatedDaily"/>.
        /// </summary>
        /// <param name="annualGrowthRate">The grow rate that occurs yearly.</param>
        /// <param name="isInRange">Predicate that specifies for dates the Appender should be invoked.</param>
        /// <remarks>
        /// A value of 3 for the <paramref name="annualGrowthRate"/> would cause growth of 300%. 
        /// Applied to an initial value of 100, the value after 1 year would be 400. For growth of
        /// 10% then a value of 0.1 should be provided for <paramref name="annualGrowthRate"/>.
        /// </remarks>
        public YearlyGrowthCalculatedDaily(decimal annualGrowthRate, Predicate<DateTime> isInRange)
        {
            _annualGrowthRate = annualGrowthRate;
            _isInRange = isInRange ?? AlwaysTrue;

            var dailyRate = Math.Pow(Convert.ToDouble(1m + _annualGrowthRate), (1.0 / 365d));
            _dailyRate = ((IConvertible)dailyRate).ToDecimal(CultureInfo.CurrentCulture);
            var dailyRateLeapYear = Math.Pow(Convert.ToDouble(1m + _annualGrowthRate), (1.0 / 366d));
            _dailyRateLeapYear = ((IConvertible)dailyRateLeapYear).ToDecimal(CultureInfo.CurrentCulture); ;
        }

        #region IDailyTransactionAppender Members

        /// <summary>
        /// Appends transactions to a given transaction list.
        /// </summary>
        /// <param name="dateTransactionList">The list of transaction to append to.</param>
        public void AppendToDailyTransaction(DateTransactionList dateTransactionList)
        {
            if (_isInRange(dateTransactionList.Date))
            {
                var interest = CalculateInterest(dateTransactionList.InitialValue, dateTransactionList.Date);
                dateTransactionList.Transactions.Add(new Transaction(interest, "Yearly interest calculated daily"));
            }
        }

        #endregion

        private decimal CalculateInterest(decimal balance, DateTime date)
        {
            var dailyRate = GetDailyRate(date);
            return balance * (dailyRate - 1);
        }

        private decimal GetDailyRate(DateTime date)
        {
            if (date.DaysInYear() == 366)
            {
                return _dailyRateLeapYear;
            }
            return _dailyRate;
        }

        private static bool AlwaysTrue(DateTime ignored)
        {
            return true;
        }
    }
}
