using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Common.Interfaces
{
    public interface ICurrencyApiClientFactory
    {
        ICurrencyApiClient GetClient(string apiName);
    }
}
