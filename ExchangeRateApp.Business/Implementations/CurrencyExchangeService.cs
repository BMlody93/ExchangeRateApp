using ExchangeRateApp.Common.Interfaces;
using ExchangeRateApp.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Business.Implementations
{
    public class CurrencyExchangeService : ICurrencyExchangeService
    {
        public Task<ExchangeResult> CalculateExchangeAsync(string api, string from, string to, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Currency>> GetCurrenciesAsync(string api)
        {
            throw new NotImplementedException();
        }
    }
}
