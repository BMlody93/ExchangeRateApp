using ExchangeRateApp.Common.ConfiguationOptions;
using ExchangeRateApp.Common.Interfaces;
using ExchangeRateApp.Common.Models;
using ExchangeRateApp.WebApi.Controllers;
using ExchangeRateApp.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class ExchangeRateControllerTests
{
    private readonly Mock<ICurrencyExchangeService> _serviceMock = new();
    private readonly Mock<ILogger<ExchangeRateController>> _loggerMock = new();
    private readonly Mock<IOptions<AppConfiguration>> _optionsMock = new();
    private readonly ExchangeRateController _controller;

    public ExchangeRateControllerTests()
    {
        var appConfig = new AppConfiguration
        {
            ApiConfiguration = new ApiConfiguration
            {
                ApiList = new List<string> { "Nbp", "SomeApi" },
                DateFormat = "yyyy-MM-dd",
                SortOption = CurrencySortOption.CodeAscending
            }
        };
        _optionsMock.Setup(o => o.Value).Returns(appConfig);

        _controller = new ExchangeRateController(_serviceMock.Object, _optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void GetApis_ReturnsOkWithApiList()
    {
        // Act
        var result = _controller.GetApis();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Contains("Nbp", list);
        Assert.Contains("SomeApi", list);
    }

    [Fact]
    public void GetApis_ReturnsNoContent_WhenApiListIsNull()
    {
        // Arrange
        _optionsMock.Setup(o => o.Value).Returns(new AppConfiguration
        {
            ApiConfiguration = new ApiConfiguration
            {
                ApiList = null
            }
        });
        var controller = new ExchangeRateController(_serviceMock.Object, _optionsMock.Object, _loggerMock.Object);

        // Act
        var result = controller.GetApis();

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task GetCurrencies_ReturnsBadRequest_WhenApiNameIsMissing()
    {
        // Act
        var result = await _controller.GetCurrencies("");

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Missing apiName", badRequest.Value);
    }

    [Fact]
    public async Task GetCurrencies_ReturnsNoContent_WhenNoCurrencies()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetCurrenciesAsync("Nbp")).ReturnsAsync(new List<Currency>());

        // Act
        var result = await _controller.GetCurrencies("Nbp");

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task GetCurrencies_ReturnsSortedCurrencies()
    {
        // Arrange
        var currencies = new List<Currency>
        {
            new Currency("US Dollar", "USD"),
            new Currency("Euro", "EUR"),
            new Currency("Yen", "JPY"),
        };
        _serviceMock.Setup(s => s.GetCurrenciesAsync("Nbp")).ReturnsAsync(currencies);

        // Act
        var result = await _controller.GetCurrencies("Nbp");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCurrencies = Assert.IsAssignableFrom<IEnumerable<Currency>>(okResult.Value);

        // Sprawdzamy sortowanie rosnąco po kodzie waluty
        var sorted = returnedCurrencies.ToList();
        Assert.Equal("EUR", sorted[0].CurrencyCode);
        Assert.Equal("JPY", sorted[1].CurrencyCode);
        Assert.Equal("USD", sorted[2].CurrencyCode);
    }

    [Fact]
    public async Task GetExchangeRate_ReturnsBadRequest_WhenRequestInvalid()
    {
        // Arrange
        var invalidRequest = new ExchangeRequestDto
        (
            "Nbp",
            "USD",
            "EUR",
            DateTime.Today.AddDays(1), 
            DateTime.Today
        );

        // Act
        var result = await _controller.GetExchangeRate(invalidRequest);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid request", badRequest.Value);
    }

    [Fact]
    public async Task GetExchangeRate_ReturnsNoContent_WhenNoRatesFound()
    {
        // Arrange
        var request = new ExchangeRequestDto
        (
            "Nbp",
            "USD",
            "EUR",
            DateTime.Today.AddDays(-1),
            DateTime.Today
        );

        _serviceMock.Setup(s => s.CalculateExchangeAsync(request.ApiName, request.From, request.To, request.DateFrom, request.DateTo))
            .ReturnsAsync(new ExchangeResult
            {
                From = new Currency("USD", "US Dollar"),
                To = new Currency("EUR", "Euro"),
                Rates = new List<ExchangeRate>() // pusta lista
            });

        // Act
        var result = await _controller.GetExchangeRate(request);

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task GetExchangeRate_ReturnsOk_WithExchangeResultDto()
    {
        // Arrange
        var request = new ExchangeRequestDto
        (
            "Nbp",
            "USD",
            "EUR",
            DateTime.Today.AddDays(-1),
            DateTime.Today
        );

        var rates = new List<ExchangeRate>
        {
            new ExchangeRate(DateTime.Today.AddDays(-1), 4.0m),
            new ExchangeRate(DateTime.Today, 4.2m),
        };
        var exchangeResult = new ExchangeResult
        {
            From = new Currency("US Dollar", "USD"),
            To = new Currency("Euro", "EUR"),
            Rates = rates
        };
        _serviceMock.Setup(s => s.CalculateExchangeAsync(request.ApiName, request.From, request.To, request.DateFrom, request.DateTo))
            .ReturnsAsync(exchangeResult);

        // Act
        var result = await _controller.GetExchangeRate(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ExchangeResultDto>(okResult.Value);
        Assert.Equal("USD", dto.From.CurrencyCode);
        Assert.Equal("EUR", dto.To.CurrencyCode);
        Assert.Equal(2, dto.Rates.Count);
        Assert.Equal("yyyy-MM-dd", _optionsMock.Object.Value.ApiConfiguration.DateFormat);
        Assert.Equal(exchangeResult.Min, dto.Min);
        Assert.Equal(exchangeResult.Max, dto.Max);
        Assert.Equal(exchangeResult.Avg, dto.Avg);
    }
}
