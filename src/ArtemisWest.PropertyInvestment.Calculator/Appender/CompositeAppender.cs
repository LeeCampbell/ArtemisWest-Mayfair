using System;
using System.Collections.Generic;
using ArtemisWest.PropertyInvestment.Calculator.Entities;

namespace ArtemisWest.PropertyInvestment.Calculator.Appender
{
    internal sealed class CompositeAppender : IDailyTransactionAppender
    {
        private readonly IEnumerable<IDailyTransactionAppender> _appenders;

        public CompositeAppender(params IDailyTransactionAppender[] appenders)
        {
            Guard.ArgumentNotNullOrEmpty(appenders, "appenders");
            foreach (var appender in appenders)
            {
                if (appender == null)
                {
                    throw new ArgumentException("Collection can not contain null values.", "appenders");
                }
            }
            _appenders = appenders;
        }

        public void AppendToDailyTransaction(DateTransactionList dateTransactionList)
        {
            foreach (var appender in _appenders)
            {
                appender.AppendToDailyTransaction(dateTransactionList);
            }
        }
    }
}