using ExchangeRateApp.ApiClient.Nbp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.ApiClient.Nbp.Extensions
{
    public static class NbpTableExtensions
    {
        public static string ToApiCode(this NbpTable table)
        {
            return table switch
            {
                NbpTable.A => "A",
                NbpTable.B => "B",
                _ => throw new InvalidOperationException($"Table {table} does not have an API code")
            };
        }
    }
}
