using ArtemisWest.PropertyInvestment.Calculator.Entities;

namespace ArtemisWest.PropertyInvestment.Calculator.Appender
{
    /// <summary>
    /// Provides a method to append transactions to a daily position.
    /// </summary>
    public interface IDailyTransactionAppender
    {
        /// <summary>
        /// Appends transactions to a given transaction list.
        /// </summary>
        /// <param name="dateTransactionList">The list of transaction to append to.</param>
        /// <remarks>
        /// Implementations of this interface will probably inspect the <see cref="DateTransactionList.Date"/> 
        /// and/or <see cref="DateTransactionList.InitialValue"/> to apply any conditional logic.
        /// </remarks>
        void AppendToDailyTransaction(DateTransactionList dateTransactionList);
    }
}