using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.ApiClient.Nbp.Models
{
    public class NbpRatesResponse
    {
        public string Table { get; set; }
        public string Currency { get; set; }
        public string Code { get; set; }
        public List<NbpRateResponse> Rates { get; set; }
    }

    public class NbpRateResponse
    {
        public DateTime EffectiveDate { get; set; }
        public decimal Mid { get; set; }
    }
}
