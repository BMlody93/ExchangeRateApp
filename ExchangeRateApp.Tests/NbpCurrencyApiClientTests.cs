using ExchangeRateApp.Common.ConfiguationOptions;
using ExchangeRateApp.Common.Exceptions;
using ExchangeRateApp.Common.Models;
using ExchangeRateApp.ApiClient.Nbp;
using ExchangeRateApp.ApiClient.Nbp.Models;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace ExchangeRateApp.Tests
{
    public class NbpCurrencyApiClientTests
    {
        private readonly Mock<HttpMessageHandler> _httpHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly Mock<ILogger<NbpCurrencyApiClient>> _loggerMock;
        private readonly IOptions<AppConfiguration> _options;

        public NbpCurrencyApiClientTests()
        {
            _httpHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.nbp.pl/")
            };

            _cache = new MemoryCache(new MemoryCacheOptions());
            _loggerMock = new Mock<ILogger<NbpCurrencyApiClient>>();

            _options = Options.Create(new AppConfiguration
            {
                ExternalApiConfigurations = new Dictionary<string, ExternalApiConfiguration>
                {
                    ["Nbp"] = new ExternalApiConfiguration { CacheDurationMinutes = 10 }
                }
            });
        }

        [Fact]
        public async Task GetCurrencyListAsync_ShouldReturnMappedCurrencyList()
        {
            // Arrange: response from NBP
            var nbpResponse = JsonSerializer.Serialize(new List<NbpExchangeTable>
            {
                new()
                {
                    Rates = new List<NbpRate>
                    {
                        new() { Code = "USD", Currency = "US Dollar", Mid = 4.00m }
                    }
                }
            });

            SetupHttpResponse("/api/exchangerates/tables/A/", nbpResponse);
            SetupHttpResponse("/api/exchangerates/tables/B/", "[]");

            var client = new NbpCurrencyApiClient(_options, _httpClient, _cache, _loggerMock.Object);

            // Act
            var result = await client.GetCurrencyListAsync();

            // Assert
            result.Should().Contain(c => c.CurrencyCode == "USD" && c.CurrencyName == "US Dollar");
            result.Should().Contain(c => c.CurrencyCode == "PLN"); // z Virtual
        }

        [Fact]
        public async Task GetExchangeRatesAsync_ShouldReturnRates_WhenDataIsValid()
        {
            // Arrange
            var currency = new Currency("US Dollar", "USD");

            // Currency list (contains USD)
            var currencyList = JsonSerializer.Serialize(new List<NbpExchangeTable>
            {
                new() { Rates = new List<NbpRate>
                {
                    new() { Code = currency.CurrencyCode, Currency = currency.CurrencyName, Mid = 4.00m }
                }}
            });

            SetupHttpResponse("/api/exchangerates/tables/A/", currencyList);
            SetupHttpResponse("/api/exchangerates/tables/B/", "[]");

            // Exchange rate response
            var rateResponse = JsonSerializer.Serialize(new NbpRatesResponse
            {
                Table = "A",
                Code = currency.CurrencyCode,
                Currency = currency.CurrencyName,
                Rates = new List<NbpRateResponse>
                {
                    new() {EffectiveDate = new DateTime(2025, 7, 2), Mid = 4.2m},
                    new() {EffectiveDate = new DateTime(2025, 7, 3), Mid = 4.3m }
                }
            });


            SetupHttpResponse("/api/exchangerates/rates/A/USD/2025-07-01/2025-07-02/", rateResponse);

            var client = new NbpCurrencyApiClient(_options, _httpClient, _cache, _loggerMock.Object);

            // Act
            var result = await client.GetExchangeRatesAsync(currency, new DateTime(2025, 7, 1), new DateTime(2025, 7, 2));

            // Assert
            result.Should().HaveCount(2);
            result[0].Value.Should().Be(4.2m);
        }

        [Fact]
        public async Task GetExchangeRatesAsync_ShouldReturnEmpty_WhenNotFound()
        {
            // Arrange
            var currency = new Currency("US Dollar", "USD" );

            SetupHttpResponse("/api/exchangerates/tables/A/", JsonSerializer.Serialize(new List<NbpExchangeTable>
            {
                new() { Rates = new List<NbpRate> { new() { Code = currency.CurrencyCode, Currency = currency.CurrencyName, Mid = 4.00m } } }
            }));

            SetupHttpResponse("/api/exchangerates/tables/B/", "[]");

            SetupHttpResponseWithStatus("/api/exchangerates/rates/A/USD/2025-07-01/2025-07-02/", HttpStatusCode.NotFound);

            var client = new NbpCurrencyApiClient(_options, _httpClient, _cache, _loggerMock.Object);

            // Act
            var result = await client.GetExchangeRatesAsync(currency, new DateTime(2025, 7, 1), new DateTime(2025, 7, 2));

            // Assert
            result.Should().BeEmpty();
        }

        private void SetupHttpResponse(string path, string jsonContent)
        {
            _httpHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == _httpClient.BaseAddress.MakeAbsoluteUri(path)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                });
        }

        private void SetupHttpResponseWithStatus(string path, HttpStatusCode statusCode)
        {
            _httpHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == _httpClient.BaseAddress.MakeAbsoluteUri(path)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode
                });
        }
    }

    // Extension method to correctly combine Uri for matching requests:
    internal static class UriExtensions
    {
        public static Uri MakeAbsoluteUri(this Uri baseUri, string relativePath)
        {
            return new Uri(baseUri, relativePath.TrimStart('/'));
        }
    }
}

