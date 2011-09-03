using System;
using ArtemisWest.PropertyInvestment.Calculator.Entities;

namespace ArtemisWest.PropertyInvestment.Calculator.Appender
{
    /// <summary>
    /// An implementation of the <see cref="IDailyTransactionAppender"/> that appends a fixed 
    /// transaction amount when the provided predicate evaluates to true.
    /// </summary>
    internal class FixedScheduledTransaction : IDailyTransactionAppender
    {
        private readonly decimal _scheduledPaymentAmount;
        private readonly string _paymentDescription;
        //private readonly Predicate<DateTime> _shouldAccrue;
        private readonly Predicate<DateTransactionList> _shouldAccrue;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedScheduledTransaction"/> class.
        /// </summary>
        /// <param name="scheduledPaymentAmount">The scheduled payment amount.</param>
        /// <param name="isTransactionDate"></param>
        public FixedScheduledTransaction(decimal scheduledPaymentAmount, Predicate<DateTime> isTransactionDate)
            : this(scheduledPaymentAmount, isTransactionDate, "Scheduled transaction")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedScheduledTransaction"/> class.
        /// </summary>
        /// <param name="scheduledPaymentAmount">The scheduled payment amount.</param>
        /// <param name="isTransactionDate"></param>
        /// <param name="paymentDescription">The payment description.</param>
        public FixedScheduledTransaction(decimal scheduledPaymentAmount, Predicate<DateTime> isTransactionDate, string paymentDescription)
        {
            _scheduledPaymentAmount = scheduledPaymentAmount;
            _shouldAccrue = list => isTransactionDate(list.Date);
            _paymentDescription = paymentDescription;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedScheduledTransaction"/> class.
        /// </summary>
        /// <param name="scheduledPaymentAmount">The scheduled payment amount.</param>
        /// <param name="shouldAccrue">Predicate to indicate if the transaction should be added to a given <see cref="DateTransactionList"/>.</param>
        public FixedScheduledTransaction(decimal scheduledPaymentAmount, Predicate<DateTransactionList> shouldAccrue)
            : this(scheduledPaymentAmount, shouldAccrue, "Scheduled transaction")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedScheduledTransaction"/> class.
        /// </summary>
        /// <param name="scheduledPaymentAmount">The scheduled payment amount.</param>
        /// <param name="shouldAccrue">Predicate to indicate if the transaction should be added to a given <see cref="DateTransactionList"/>.</param>
        /// <param name="paymentDescription">The payment description.</param>
        public FixedScheduledTransaction(decimal scheduledPaymentAmount, Predicate<DateTransactionList> shouldAccrue, string paymentDescription)
        {
            _scheduledPaymentAmount = scheduledPaymentAmount;
            _shouldAccrue = shouldAccrue;
            _paymentDescription = paymentDescription;
        }

        /// <summary>
        /// Appends transactions to a given transaction list.
        /// </summary>
        /// <param name="dateTransactionList">The list of transaction to append to.</param>
        public void AppendToDailyTransaction(DateTransactionList dateTransactionList)
        {
            if (_shouldAccrue(dateTransactionList))
            {
                dateTransactionList.Transactions.Add(new Transaction(_scheduledPaymentAmount, _paymentDescription));
            }
        }
    }
}