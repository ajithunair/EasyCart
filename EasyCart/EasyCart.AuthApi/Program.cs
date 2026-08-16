using EasyCart.AuthApi.Data;
using EasyCart.AuthApi.DependencyInjection;
using EasyCart.SharedLibrary.DependencyInjection;
using Azure.Identity;

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
    .AddAzureKeyVault(keyVaultUrl, new DefaultAzureCredential());

builder.Services.AddAuthenticationApiService(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithBearerAuth();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.ApplyMigrations<AuthenticationDbContext>();
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
