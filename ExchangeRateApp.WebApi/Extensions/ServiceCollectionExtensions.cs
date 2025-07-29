using ExchangeRateApp.Common.ConfiguationOptions;
using Microsoft.Extensions.Options;

namespace ExchangeRateApp.WebApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiHttpClient<TClient>(this IServiceCollection services, string apiKey)
            where TClient : class
        {
            services.AddHttpClient<TClient>((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<AppConfiguration>>().Value;

                if (!config.ExternalApiConfigurations.TryGetValue(apiKey, out var apiEntry))
                {
                    throw new InvalidOperationException($"No base URL configured for API key '{apiKey}'");
                }

                client.BaseAddress = new Uri(apiEntry.BaseUrl);
            });

            return services;
        }
    }
}
