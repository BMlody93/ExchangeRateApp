using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Common.ConfiguationOptions
{
    public class AppConfiguration
    {
        public Dictionary<string, ExternalApiConfiguration> ExternalApiConfigurations { get; set; }
        public ApiConfiguration ApiConfiguration { get; set; }
    }

    public class ExternalApiConfiguration
    {
        public string BaseUrl { get; set; }
        public int CacheDurationMinutes { get; set; }
    }

    public class ApiConfiguration
    {
        public List<string> ApiList { get; set; }
        public CurrencySortOption SortOption { get; set; }          
        public string DateFormat { get; set; }          
    }

    public enum CurrencySortOption
    {
        CodeAscending,
        CodeDescending,
        NameAscending,
        NameDescending
    }
}
