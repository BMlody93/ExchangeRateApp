using ExchangeRateApp.ApiClient.Nbp;
using ExchangeRateApp.Business.Implementations;
using ExchangeRateApp.Common.ConfiguationOptions;
using ExchangeRateApp.Common.Interfaces;
using ExchangeRateApp.WebApi.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddScoped<ICurrencyExchangeService, CurrencyExchangeService>();
builder.Services.AddScoped<ICurrencyApiClientFactory, CurrencyApiClientFactory>();
builder.Services.AddScoped<ICurrencyApiClient, NbpCurrencyApiClient>();

builder.Services.AddApiHttpClient<NbpCurrencyApiClient>("Nbp");

builder.Services.AddMemoryCache();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add configuration
builder.Services.Configure<AppConfiguration>(
    builder.Configuration.GetSection("AppConfiguration")
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
