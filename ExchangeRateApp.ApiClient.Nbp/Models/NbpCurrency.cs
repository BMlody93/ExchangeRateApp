using ExchangeRateApp.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.ApiClient.Nbp.Models
{
    public class NbpCurrency
    {
        public string Name { get; }
        public string CurrencyCode { get; }
        public NbpTable Table { get; }

        public NbpCurrency(string name, string currencyCode, NbpTable table)
        {
            Name = name;
            CurrencyCode = currencyCode;
            Table = table;
        }

        public Currency ToCurrency() => new(Name, CurrencyCode);
    }
}
