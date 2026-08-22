using EasyCart.ProductApi.Data;
using EasyCart.ProductApi.DependencyInjection;
using EasyCart.SharedLibrary.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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

builder.Services.AddProductApiServices(builder.Configuration);

var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
if (string.IsNullOrWhiteSpace(redisConnectionString))
{
    Log.Information("Redis is not configured; Product API will read products directly from PostgreSQL.");
}
else
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        // Do not prevent the API from starting when an optional cache is offline.
        options.Configuration = $"{redisConnectionString},abortConnect=false,connectTimeout=1000,syncTimeout=1000,asyncTimeout=1000,connectRetry=1";
        options.InstanceName = "ProductService_";
    });

    Log.Information("Redis cache is configured for the Product API.");
}

var app = builder.Build();

app.MapHealthChecks("/health");

// Configure the HTTP request pipeline.
app.ApplyMigrations<ProductDbContext>();
app.UseProductApiMiddlewares();

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
