using EasyCart.InventoryApi.Data;
using EasyCart.InventoryApi.DependencyInjection;
using EasyCart.InventoryApi.RabbitMQ.Consumers;
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

builder.Services.AddMassTransit(x =>
{
    // Register the consumer
    x.AddConsumer<OrderPlacedConsumer>();

    x.UsingRabbitMq((context, config) =>
    {
        config.Host(rabbitSection["Host"]!, "/", h =>
        {
            h.Username(rabbitSection["Username"]!);
            h.Password(rabbitSection["Password"]!);
        });

        // Automatically configure endpoints (Queues) based on the registered consumers
        config.ConfigureEndpoints(context);
    });
});

builder.Services.AddInventoryApiServices(builder.Configuration);

var app = builder.Build();

app.MapHealthChecks("/health");

app.ApplyMigrations<InventoryDbContext>();
app.UseInventoryApiMiddlewares();
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
