using EasyCart.ApiGateway.Middlewares;
using EasyCart.SharedLibrary.DependencyInjection;
using Azure.Identity;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true)

    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true)

    .AddEnvironmentVariables();

var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
    builder.Configuration.AddEnvironmentVariables();
}

// Register telemetry after all configuration providers, including Key Vault, are loaded.
builder.Services.AddSharedOpenTelemetry(builder.Configuration);

var gatewayLogFile = builder.Configuration["Serilog:FileName"] ?? "Serilog/ApiGateway.log";
Directory.CreateDirectory(Path.GetDirectoryName(gatewayLogFile) ?? "Serilog");
builder.Host.UseSerilog((_, _, loggerConfiguration) => loggerConfiguration
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Debug()
    .WriteTo.File(
        gatewayLogFile,
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));

builder.Services
    .AddOcelot(builder.Configuration)
    .AddCacheManager(x =>
    {
        x.WithDictionaryHandle();
    });

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Configure the HTTP request pipeline.
var app = builder.Build();

app.UseMiddleware<TraceMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "EasyCart API Gateway Running");

app.UseMiddleware<AttachApiGatewaySignarureToRequest>();

await app.UseOcelot();

app.Run();
