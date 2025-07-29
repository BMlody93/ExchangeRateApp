namespace ExchangeRateApp.WebApi.DTOs
{
    public record ExchangeRequestDto(
        string ApiName,
        string From,
        string To,
        DateTime DateFrom,
        DateTime DateTo
    );
}
