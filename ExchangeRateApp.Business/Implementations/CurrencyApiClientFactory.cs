using ExchangeRateApp.ApiClient.Nbp;
using ExchangeRateApp.Common.Exceptions;
using ExchangeRateApp.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Business.Implementations
{
    public class CurrencyApiClientFactory : ICurrencyApiClientFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CurrencyApiClientFactory> _logger;

        public CurrencyApiClientFactory(IServiceProvider serviceProvider, ILogger<CurrencyApiClientFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public ICurrencyApiClient GetClient(string apiName)
        {
            if (string.IsNullOrWhiteSpace(apiName))
            {
                _logger.LogWarning("Attempted to get a currency API client with a null or empty apiName");
                throw new ArgumentException("apiName must not be null or empty", nameof(apiName));
            }

            _logger.LogInformation("Resolving API client for API name: {ApiName}", apiName);

            try
            {
                return apiName.ToLower() switch
                {
                    "nbp" => _serviceProvider.GetRequiredService<NbpCurrencyApiClient>(),
                    _ => throw new NotSupportedException($"Currency API '{apiName}' is not supported")
                };
            }
            catch (NotSupportedException ex)
            {
                _logger.LogError(ex, "Failed to resolve client for API: {ApiName}", apiName);
                throw new NotFoundException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve client for API: {ApiName}", apiName);
                throw;
            }
        }
    }
}
