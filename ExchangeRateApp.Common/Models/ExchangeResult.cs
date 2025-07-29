using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Common.Models
{
    public class ExchangeResult
    {
        public required Currency From { get; set; }
        public required Currency To { get; set; }
        public decimal Min => Rates.Min(r => r.Value);
        public decimal Max => Rates.Max(r => r.Value);
        public decimal Avg => Math.Round(Rates.Average(r => r.Value), 6);
        public required List<ExchangeRate> Rates { get; set; }
    }
}
