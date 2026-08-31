using EasyCart.AuthApi.Data;
using EasyCart.AuthApi.DependencyInjection;
using EasyCart.SharedLibrary.DependencyInjection;
using Azure.Identity;

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
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
    // Container/App Service settings intentionally take precedence over Key Vault values.
    builder.Configuration.AddEnvironmentVariables();
}

builder.Services.AddAuthenticationApiService(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithBearerAuth();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    app.ApplyMigrations<AuthenticationDbContext>();
}
app.UseAuthenticationService();
app.UseSwagger();
app.UseSwaggerUI();

if(!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
