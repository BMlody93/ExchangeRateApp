using ExchangeRateApp.Common.Models;

namespace ExchangeRateApp.WebApi.DTOs
{
    public record ExchangeResultDto(
        Currency From,
        Currency To, 
        List<ExchangeRateDto> Rates,
        decimal Min,
        decimal Max,
        decimal Avg
    );

    public record ExchangeRateDto(
        string Date, 
        decimal Value
    );
}
