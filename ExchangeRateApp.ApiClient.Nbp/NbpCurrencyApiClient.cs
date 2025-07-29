using ExchangeRateApp.Common.Interfaces;
using ExchangeRateApp.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.ApiClient.Nbp
{
    public class NbpCurrencyApiClient : ICurrencyApiClient
    {
        public Task<List<Currency>> GetCurrencyListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IList<ExchangeRate>> GetExchangeRatesAsync(Currency currency, DateTime dateFrom, DateTime dateTo)
        {
            throw new NotImplementedException();
        }
    }
}
