using EasyCart.OrderApi.Data;
using EasyCart.OrderApi.DependencyInjection;
using EasyCart.OrderApi.Messaging;
using EasyCart.SharedLibrary.DependencyInjection;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
var keyVaultUrl = new Uri("https://easycart-kv.vault.azure.net/");

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddAzureKeyVault(keyVaultUrl, new Azure.Identity.DefaultAzureCredential());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithBearerAuth();

var rabbitSection = builder.Configuration.GetSection("RabbitMQ");
var serviceBusConnectionString = builder.Configuration.GetConnectionString("EasycartServiceBus");
var messagingTransport = builder.Configuration["Messaging:Transport"]
    ?? (builder.Environment.IsDevelopment() ? "RabbitMQ" : "AzureServiceBus");

//Configure MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderInventoryReservationConsumer>();

    if (string.Equals(messagingTransport, "RabbitMQ", StringComparison.OrdinalIgnoreCase))
    {
        x.UsingRabbitMq((context, config) =>
        {
            config.Host(rabbitSection["Host"]!, "/", h =>
            {
                h.Username(rabbitSection["Username"]!);
                h.Password(rabbitSection["Password"]!);
            });

            config.ConfigureEndpoints(context);
        });
    }
    else if (string.Equals(messagingTransport, "AzureServiceBus", StringComparison.OrdinalIgnoreCase))
    {
        if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
            throw new InvalidOperationException("Azure Service Bus connection string is not configured.");

        x.UsingAzureServiceBus((context, config) =>
        {
            config.Host(serviceBusConnectionString);
            config.ConfigureEndpoints(context);
        });
    }
    else
    {
        throw new InvalidOperationException($"Unsupported messaging transport: {messagingTransport}.");
    }
});

builder.Services.AddOrderApiServices(builder.Configuration);

var app = builder.Build();

app.MapHealthChecks("/health");

app.ApplyMigrations<OrderDbContext>();
app.UserOrderApiMiddlewares();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
