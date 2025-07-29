using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Common.Models
{
    public class Currency
    {
        public string CurrencyName { get; }
        public string CurrencyCode { get; }

        public Currency(string currencyName, string currencyCode)
        {
            ArgumentException.ThrowIfNullOrEmpty(currencyName);
            ArgumentException.ThrowIfNullOrEmpty(currencyCode);
            CurrencyName = currencyName;
            CurrencyCode = currencyCode;
        }
    }
}
