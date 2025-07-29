using ExchangeRateApp.Common.Exceptions;
using ExchangeRateApp.Common.Interfaces;
using ExchangeRateApp.Common.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Business.Implementations
{
    public class CurrencyExchangeService : ICurrencyExchangeService
    {
        private readonly ICurrencyApiClientFactory _currencyApiClientFactory;
        private readonly ILogger<CurrencyExchangeService> _logger;

        public CurrencyExchangeService(
            ICurrencyApiClientFactory currencyApiClientFactory,
            ILogger<CurrencyExchangeService> logger)
        {
            _currencyApiClientFactory = currencyApiClientFactory;
            _logger = logger;
        }


        public async Task<ExchangeResult> CalculateExchangeAsync(string api, string from, string to, DateTime dateFrom, DateTime dateTo)
        {

            try
            {
                _logger.LogInformation("Calculating exchange from {From} to {To} using API {Api} for period {DateFrom} - {DateTo}", from, to, api, dateFrom, dateTo);

                ArgumentException.ThrowIfNullOrEmpty(api);
                ArgumentException.ThrowIfNullOrEmpty(from);
                ArgumentException.ThrowIfNullOrEmpty(to);
                if (dateFrom > dateTo || dateFrom > DateTime.Today || dateTo > DateTime.Today)
                    throw new ArgumentException("Wrong dates");


                var currencyApiClient = _currencyApiClientFactory.GetClient(api);

                var currencyList = await currencyApiClient.GetCurrencyListAsync()
                    ?? throw new NotFoundException("Currency list is null");

                var fromCurrency = currencyList.FirstOrDefault(x => x.CurrencyCode.Equals(from, StringComparison.OrdinalIgnoreCase))
                    ?? throw new ArgumentException($"Unknown currency code: {from}", nameof(from));

                var toCurrency = currencyList.FirstOrDefault(x => x.CurrencyCode.Equals(to, StringComparison.OrdinalIgnoreCase))
                    ?? throw new ArgumentException($"Unknown currency code: {to}", nameof(to));

                var exchangeRatesFrom = await currencyApiClient.GetExchangeRatesAsync(fromCurrency, dateFrom, dateTo);
                var exchangeRatesTo = await currencyApiClient.GetExchangeRatesAsync(toCurrency, dateFrom, dateTo);

                var toRateMap = exchangeRatesTo.ToDictionary(r => r.Date, r => r.Value);

                var rates = exchangeRatesFrom
                    .Where(rate => toRateMap.ContainsKey(rate.Date))
                    .Select(rate =>
                    {
                        if (toRateMap.TryGetValue(rate.Date, out var toValue))
                        {
                            //    throw new GeneralException($"Missing exchange rate for {to} on {rate.Date:yyyy-MM-dd}");
                        }
                        return new ExchangeRate(rate.Date, rate.Value / toValue);
                    }).ToList();

                _logger.LogInformation("Successfully calculated exchange result for {From} to {To} with {Count} rates.", from, to, rates.Count);

                return new ExchangeResult
                {
                    From = fromCurrency,
                    To = toCurrency,
                    Rates = rates
                };
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error calculating exchange from {From} to {To} using API {Api}", from, to, api);
                throw new BadRequestException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating exchange from {From} to {To} using API {Api}", from, to, api);
                throw;
            }
        }

        public async Task<IList<Currency>> GetCurrenciesAsync(string api)
        {
            try
            {
                _logger.LogInformation("Retrieving currencies from API {Api}", api);

                ArgumentException.ThrowIfNullOrEmpty(api);

                var currencyApiClient = _currencyApiClientFactory.GetClient(api);
                var currencyList = await currencyApiClient.GetCurrencyListAsync();

                _logger.LogInformation("Retrieved {Count} currencies from API {Api}", currencyList?.Count ?? 0, api);

                return currencyList ?? new List<Currency>();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error retrieving currencies from API {Api}", api);
                throw new BadRequestException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currencies from API {Api}", api);
                throw;
            }
        }
    }
}
