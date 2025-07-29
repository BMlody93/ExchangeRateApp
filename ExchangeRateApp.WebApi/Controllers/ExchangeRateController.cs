using ExchangeRateApp.Common.ConfiguationOptions;
using ExchangeRateApp.Common.Interfaces;
using ExchangeRateApp.Common.Models;
using ExchangeRateApp.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ExchangeRateApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeRateController : ControllerBase
    {
        private readonly ICurrencyExchangeService _currencyExchangeService;
        private readonly ILogger<ExchangeRateController> _logger;
        private readonly AppConfiguration _appConfiguration;

        public ExchangeRateController(
            ICurrencyExchangeService currencyExchangeService,
            IOptions<AppConfiguration> appSettings,
            ILogger<ExchangeRateController> logger)
        {
            _currencyExchangeService = currencyExchangeService;
            _logger = logger;
            _appConfiguration = appSettings.Value;
        }

        [HttpGet("apis")]
        public ActionResult<IEnumerable<string>> GetApis()
        {

            if (_appConfiguration.ApiConfiguration.ApiList is null || !_appConfiguration.ApiConfiguration.ApiList.Any())
            {
                _logger.LogWarning("No API list found in configuration.");
                return NoContent();
            }

            return Ok(_appConfiguration.ApiConfiguration.ApiList);

        }

        [HttpGet("currencies")]
        public async Task<ActionResult<IEnumerable<Currency>>> GetCurrencies([FromQuery] string apiName)
        {

            if (string.IsNullOrWhiteSpace(apiName))
            {
                _logger.LogWarning("Missing apiName in request.");
                return BadRequest("Missing apiName");
            }

            var currencies = await _currencyExchangeService.GetCurrenciesAsync(apiName);

            if (currencies == null || !currencies.Any())
            {
                _logger.LogInformation("No currencies found for API: {ApiName}", apiName);
                return NoContent();
            }

            return Ok(SortCurrencies(currencies));

        }

        [HttpPost("rates")]
        public async Task<ActionResult<ExchangeResultDto>> GetExchangeRate([FromBody] ExchangeRequestDto query)
        {

            if (query == null || string.IsNullOrEmpty(query.From) || string.IsNullOrEmpty(query.To) || query.DateFrom > query.DateTo ||
                query.DateFrom > DateTime.Today || query.DateTo > DateTime.Today)
            {
                _logger.LogWarning("Invalid exchange rate request received: {@Query}", query);
                return BadRequest("Invalid request");
            }

            var exchangeResult = await _currencyExchangeService.CalculateExchangeAsync(
                query.ApiName,
                query.From,
                query.To,
                query.DateFrom,
                query.DateTo
            );

            if (exchangeResult is null || exchangeResult.Rates.Count == 0)
            {
                _logger.LogInformation("No exchange rates found for query: {@Query}", query);
                return NoContent();
            }

            var result = new ExchangeResultDto(
                exchangeResult.From,
                exchangeResult.To,
                exchangeResult.Rates.Select(x => new ExchangeRateDto(x.Date.ToString(_appConfiguration.ApiConfiguration.DateFormat), x.Value)).ToList(),
                exchangeResult.Min,
                exchangeResult.Max,
                exchangeResult.Avg);

            return Ok(result);

        }

        private IList<Currency> SortCurrencies(IList<Currency> currencies)
        {
            return _appConfiguration.ApiConfiguration.SortOption switch
            {
                CurrencySortOption.CodeAscending => currencies.OrderBy(c => c.CurrencyCode).ToList(),
                CurrencySortOption.CodeDescending => currencies.OrderByDescending(c => c.CurrencyCode).ToList(),
                CurrencySortOption.NameAscending => currencies.OrderBy(c => c.CurrencyName).ToList(),
                CurrencySortOption.NameDescending => currencies.OrderByDescending(c => c.CurrencyName).ToList(),
                _ => currencies
            };
        }
    }
}
