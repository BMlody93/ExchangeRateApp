using ExchangeRateApp.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Business.Implementations
{
    public class CurrencyApiClientFactory : ICurrencyApiClientFactory
    {
        public ICurrencyApiClient GetClient(string apiName)
        {
            throw new NotImplementedException();
        }
    }
}
