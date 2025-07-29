using ExchangeRateApp.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Common.Interfaces
{
    public interface ICurrencyApiClient
    {
        Task<List<Currency>> GetCurrencyListAsync();
        Task<IList<ExchangeRate>> GetExchangeRatesAsync(Currency currency, DateTime dateFrom, DateTime dateTo);
    }
}
