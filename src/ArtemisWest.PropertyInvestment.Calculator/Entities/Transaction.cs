namespace ArtemisWest.PropertyInvestment.Calculator.Entities
{
    /// <summary>
    /// Represents a financial transaction.
    /// </summary>
    public sealed class Transaction
    {
        private readonly string _description;
        private readonly decimal _amount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public Transaction(decimal amount)
            : this(amount, amount < 0 ? "Withdrawal" : "Deposit")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="description">The description.</param>
        public Transaction(decimal amount, string description)
        {
            _description = description;
            _amount = amount;
        }

        /// <summary>
        /// Gets the amount.
        /// </summary>
        /// <value>The amount.</value>
        public decimal Amount
        {
            get { return _amount; }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return _description; }
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return _amount.GetHashCode() ^ _description.GetHashCode();
        }
    }
}