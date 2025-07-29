using ExchangeRateApp.Common.ConfiguationOptions;
using ExchangeRateApp.Common.Exceptions;
using ExchangeRateApp.Common.Interfaces;
using ExchangeRateApp.Common.Models;
using ExchangeRateApp.ApiClient.Nbp.Extensions;
using ExchangeRateApp.ApiClient.Nbp.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExchangeRateApp.ApiClient.Nbp
{
    public class NbpCurrencyApiClient : ICurrencyApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ExternalApiConfiguration _nbpApiConfiguration;
        private readonly ILogger<NbpCurrencyApiClient> _logger;

        public NbpCurrencyApiClient(
            IOptions<AppConfiguration> options,
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<NbpCurrencyApiClient> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _nbpApiConfiguration = options.Value.ExternalApiConfigurations["Nbp"];
        }

        public async Task<List<Currency>> GetCurrencyListAsync()
        {
            try
            {
                var list = await GetNbpCurrencyListAsync();
                _logger.LogInformation("Fetched {Count} currencies from NBP API", list.Count);
                return list.Select(n => n.ToCurrency()).ToList();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error occurred while fetching NBP currency list");
                throw new ServiceUnavailableException("nbp", ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching NBP currency list");
                throw;
            }
        }

        public async Task<IList<ExchangeRate>> GetExchangeRatesAsync(Currency currency, DateTime dateFrom, DateTime dateTo)
        {
            ArgumentNullException.ThrowIfNull(currency);

            if (dateFrom > dateTo || dateFrom > DateTime.Today || dateTo > DateTime.Today)
                throw new ArgumentException("Wrong dates");

            try
            {
                var nbpCurrencies = await GetNbpCurrencyListAsync();
                var nbpCurrency = nbpCurrencies.FirstOrDefault(c => c.CurrencyCode.Equals(currency.CurrencyCode, StringComparison.OrdinalIgnoreCase))
                                  ?? throw new ArgumentException($"Currency '{currency.CurrencyCode}' not found");

                if (nbpCurrency.Table == NbpTable.Virtual)
                {
                    _logger.LogInformation("Using virtual currency conversion for {Currency}", currency.CurrencyCode);
                    return Enumerable.Range(0, (dateTo - dateFrom).Days + 1)
                        .Select(offset => new ExchangeRate(dateFrom.AddDays(offset), 1.0m))
                        .ToList();
                }

                var table = nbpCurrency.Table.ToApiCode();
                var fromStr = dateFrom.ToString("yyyy-MM-dd");
                var toStr = dateTo.ToString("yyyy-MM-dd");

                var url = $"api/exchangerates/rates/{table}/{currency.CurrencyCode}/{fromStr}/{toStr}/";
                _logger.LogInformation("Requesting exchange rates from NBP: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Exchange rates not found for {Currency} between {From} and {To}", currency.CurrencyCode, dateFrom, dateTo);
                    return new List<ExchangeRate>();
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<NbpRatesResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Rates == null)
                {
                    _logger.LogWarning("Empty or invalid response from NBP for {Currency}", currency.CurrencyCode);
                    return new List<ExchangeRate>();
                }

                return result.Rates
                    .Select(r => new ExchangeRate(r.EffectiveDate, r.Mid))
                    .ToList();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Failed to retrieve exchange rates for {Currency}", currency.CurrencyCode);
                throw new BadRequestException(ex.Message, ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve exchange rates for {Currency}", currency.CurrencyCode);
                throw new ServiceUnavailableException("nbp", ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve exchange rates for {Currency}", currency.CurrencyCode);
                throw;
            }
        }

        private async Task<List<NbpCurrency>> FetchNbpCurrenciesFromTable(NbpTable table)
        {
            var url = $"api/exchangerates/tables/{table.ToApiCode()}/";
            _logger.LogInformation("Fetching currency table {Table} from NBP API", table);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tables = JsonSerializer.Deserialize<List<NbpExchangeTable>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var result = tables?
                .SelectMany(t => t.Rates.Select(rate => new NbpCurrency(rate.Currency, rate.Code, table)))
                .ToList() ?? new List<NbpCurrency>();

            _logger.LogInformation("Fetched {Count} currencies from NBP table {Table}", result.Count, table);
            return result;
        }

        private async Task<List<NbpCurrency>> GetNbpCurrencyListAsync()
        {
            const string cacheKey = "nbp-currency-list-internal";

            if (_cache.TryGetValue(cacheKey, out List<NbpCurrency> nbpCurrencies))
            {
                _logger.LogDebug("Loaded NBP currency list from cache");
                return nbpCurrencies;
            }

            _logger.LogInformation("Fetching NBP currency list from remote API");

            nbpCurrencies = new List<NbpCurrency>();
            nbpCurrencies.AddRange(await FetchNbpCurrenciesFromTable(NbpTable.A));
            nbpCurrencies.AddRange(await FetchNbpCurrenciesFromTable(NbpTable.B));
            nbpCurrencies.Add(new NbpCurrency("złoty polski", "PLN", NbpTable.Virtual));

            _cache.Set(cacheKey, nbpCurrencies, TimeSpan.FromMinutes(_nbpApiConfiguration.CacheDurationMinutes));
            _logger.LogInformation("NBP currency list cached for {Minutes} minutes", _nbpApiConfiguration.CacheDurationMinutes);

            return nbpCurrencies;
        }
    }
}
