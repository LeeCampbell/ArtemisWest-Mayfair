using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtemisWest.PropertyInvestment.Calculator.Entities
{
    [System.Diagnostics.DebuggerDisplay("Date={Date}; InitialValue={InitialValue}")]
    public sealed class DateTransactionList
    {
        private readonly DateTime _date;
        private readonly decimal _initialValue;
        private readonly ICollection<Transaction> _transactions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTransactionList"/> class.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="transactions">The transactions.</param>
        public DateTransactionList(DateTime date, decimal initialValue, ICollection<Transaction> transactions)
        {
            Guard.ArgumentNotNull(transactions, "transactions");
            _date = date;
            _initialValue = initialValue;
            _transactions = transactions;
        }

        /// <summary>
        /// Gets the date for the transaction list.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date
        {
            get { return _date; }
        }

        /// <summary>
        /// Gets the intial value.
        /// </summary>
        /// <value>The intial value.</value>
        public decimal InitialValue
        {
            get { return _initialValue; }
        }

        /// <summary>
        /// Gets the transactions.
        /// </summary>
        /// <value>The transactions.</value>
        public ICollection<Transaction> Transactions
        {
            get { return _transactions; }
        }

        //TODO: Closing value? If so should realy cache value. If we do that then also consider why we dont construct the transactions internally instad of having it passed in.
        public decimal CurrentBalance()
        {
            return InitialValue + Transactions.Sum(t => t.Amount);
        }

        #region System.Object overrides.
        /// <summary>
        /// Determines whether the specified <see cref="DateTransactionList"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The other instance of <see cref="DateTransactionList"/> to be compared.</param>
        /// <returns>true if the specified <see cref="DateTransactionList"/> is equal to the current instance; otherwise, false.</returns>
        public bool Equals(DateTransactionList other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (other.Date == Date && other.InitialValue == InitialValue)
            {
                if (other.Transactions.Count == Transactions.Count)
                {
                    var thisEnum = Transactions.GetEnumerator();
                    var otherEnum = other.Transactions.GetEnumerator();
                    while (thisEnum.MoveNext() && otherEnum.MoveNext())
                    {
                        if (!Equals(thisEnum.Current, otherEnum.Current))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. 
        ///                 </param><exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.
        ///                 </exception><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(DateTransactionList)) return false;
            return Equals((DateTransactionList)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = _date.GetHashCode();
                result = (result * 397) ^ _initialValue.GetHashCode();
                var transactionHash = 0;
                if (_transactions != null)
                {
                    foreach (var transaction in _transactions)
                    {
                        transactionHash = transactionHash ^ transaction.GetHashCode();
                    }
                }
                return (result * 397) ^ transactionHash;
            }
        }
        #endregion
    }
}
