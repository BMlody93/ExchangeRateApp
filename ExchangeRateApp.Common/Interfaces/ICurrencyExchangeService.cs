using ExchangeRateApp.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Common.Interfaces
{
    public interface ICurrencyExchangeService
    {
        Task<ExchangeResult> CalculateExchangeAsync(string api, string from, string to, DateTime dateFrom, DateTime dateTo);
        Task<IList<Currency>> GetCurrenciesAsync(string api);
    }
}
