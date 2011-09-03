using System;
using System.Globalization;

namespace ArtemisWest.PropertyInvestment.Calculator.Entities
{
    /// <summary>
    /// Represents a financial position at a point in time.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Date={Date}; Value={Value}")]
    public struct Position
    {
        private readonly DateTime _date;
        private readonly decimal _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Position"/> class.
        /// </summary>
        /// <param name="date">The date the position is for.</param>
        /// <param name="value">The value at the <paramref name="date"/>.</param>
        public Position(DateTime date, decimal value)
        {
            _date = date;
            _value = value;
        }

        /// <summary>
        /// Gets the date the position is relevant for.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date
        {
            get { return _date; }
        }

        /// <summary>
        /// Gets the value of the position.
        /// </summary>
        /// <value>The value of the position.</value>
        public decimal Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Position (Date:{0}, Value:{1:c})", Date, Value);
        }
    }
}