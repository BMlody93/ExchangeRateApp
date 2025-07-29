using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Common.Models
{
    public class ExchangeRate
    {
        public DateTime Date { get; }
        public decimal Value { get; }

        public ExchangeRate(DateTime date, decimal value)
        {
            Date = date;
            Value = value;
        }
    }
}
