using ExchangeRateApp.Business.Implementations;
using ExchangeRateApp.Common.Exceptions;
using ExchangeRateApp.Common.Interfaces;
using ExchangeRateApp.Common.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class CurrencyExchangeServiceTests
{
    private readonly Mock<ICurrencyApiClientFactory> _factoryMock = new();
    private readonly Mock<ICurrencyApiClient> _clientMock = new();
    private readonly Mock<ILogger<CurrencyExchangeService>> _loggerMock = new();
    private readonly CurrencyExchangeService _service;

    public CurrencyExchangeServiceTests()
    {
        _factoryMock.Setup(f => f.GetClient(It.IsAny<string>())).Returns(_clientMock.Object);
        _service = new CurrencyExchangeService(_factoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CalculateExchangeAsync_ReturnsCorrectResult()
    {
        // Arrange
        var fromCurrency = new Currency("US Dollar", "USD");
        var toCurrency = new Currency("Euro", "EUR");

        var currencyList = new List<Currency> { fromCurrency, toCurrency };
        _clientMock.Setup(c => c.GetCurrencyListAsync()).ReturnsAsync(currencyList);

        var fromRates = new List<ExchangeRate>
        {
            new(DateTime.Today.AddDays(-1), 4.0m),
            new(DateTime.Today, 4.2m),
        };
        var toRates = new List<ExchangeRate>
        {
            new(DateTime.Today.AddDays(-1), 2.0m),
            new(DateTime.Today, 2.1m),
        };

        _clientMock.Setup(c => c.GetExchangeRatesAsync(fromCurrency, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(fromRates);
        _clientMock.Setup(c => c.GetExchangeRatesAsync(toCurrency, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(toRates);

        var dateFrom = DateTime.Today.AddDays(-1);
        var dateTo = DateTime.Today;

        // Act
        var result = await _service.CalculateExchangeAsync("Nbp", "USD", "EUR", dateFrom, dateTo);

        // Assert
        Assert.Equal(fromCurrency, result.From);
        Assert.Equal(toCurrency, result.To);
        Assert.Equal(2, result.Rates.Count);

        Assert.Equal(4.0m / 2.0m, result.Rates[0].Value);
        Assert.Equal(4.2m / 2.1m, result.Rates[1].Value);
    }

    [Fact]
    public async Task CalculateExchangeAsync_ThrowsBadRequest_WhenApiIsNullOrEmpty()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.CalculateExchangeAsync(null!, "USD", "EUR", DateTime.Today.AddDays(-1), DateTime.Today));
    }

    [Fact]
    public async Task CalculateExchangeAsync_ThrowsBadRequest_WhenFromCurrencyUnknown()
    {
        _clientMock.Setup(c => c.GetCurrencyListAsync()).ReturnsAsync(new List<Currency> { new Currency("EUR", "Euro") });

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.CalculateExchangeAsync("Nbp", "USD", "EUR", DateTime.Today.AddDays(-1), DateTime.Today));
    }

    [Fact]
    public async Task CalculateExchangeAsync_ThrowsBadRequest_WhenDatesAreInvalid()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.CalculateExchangeAsync("Nbp", "USD", "EUR", DateTime.Today.AddDays(1), DateTime.Today));
    }

    [Fact]
    public async Task GetCurrenciesAsync_ReturnsCurrencyList()
    {
        // Arrange
        var currencies = new List<Currency>
        {
            new("US Dollar", "USD"),
            new("Euro", "EUR"),
        };
        _clientMock.Setup(c => c.GetCurrencyListAsync()).ReturnsAsync(currencies);

        // Act
        var result = await _service.GetCurrenciesAsync("Nbp");

        // Assert
        Assert.Equal(currencies.Count, result.Count);
        Assert.Contains(result, c => c.CurrencyCode == "USD");
        Assert.Contains(result, c => c.CurrencyCode == "EUR");
    }

    [Fact]
    public async Task GetCurrenciesAsync_ThrowsBadRequest_WhenApiIsNullOrEmpty()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _service.GetCurrenciesAsync(null!));
    }
}