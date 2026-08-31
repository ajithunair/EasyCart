using EasyCart.OrderApi.Data;
using EasyCart.OrderApi.DependencyInjection;
using EasyCart.OrderApi.Messaging;
using EasyCart.SharedLibrary.DependencyInjection;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true)
    .AddEnvironmentVariables();

var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new Azure.Identity.DefaultAzureCredential());
    builder.Configuration.AddEnvironmentVariables();
}

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

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    app.ApplyMigrations<OrderDbContext>();
}
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
