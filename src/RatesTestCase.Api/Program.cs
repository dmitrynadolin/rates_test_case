using Microsoft.Extensions.DependencyInjection;
using RatesTestCase.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BinanceFeedSettings>(builder.Configuration.GetSection(nameof(BinanceFeedSettings)));
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<FeedListenerService>();

builder.Services.AddSingleton<WebSocketSessionManager>();
builder.Services.AddSingleton<IWebSocketSessionManager>(sp => sp.GetService<WebSocketSessionManager>());
builder.Services.AddSingleton<ICurrencyRatesWriter>(sp => sp.GetService<WebSocketSessionManager>());

builder.Services.AddSingleton<CurrentRatesService>();
builder.Services.AddSingleton<ICurrencyRatesReader>(sp => sp.GetService<CurrentRatesService>());
builder.Services.AddSingleton<ICurrencyRatesWriter>(sp => sp.GetService<CurrentRatesService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.UseWebSockets();

app.Run();
