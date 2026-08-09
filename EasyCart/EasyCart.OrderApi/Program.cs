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

//Configure MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderInventoryReservationConsumer>();

    x.UsingRabbitMq((context, config) =>
    {
        config.Host(rabbitSection["Host"]!, "/", h =>
        {
            h.Username(rabbitSection["Username"]!);
            h.Password(rabbitSection["Password"]!);
        });

        config.ConfigureEndpoints(context);
    });
});

builder.Services.AddOrderApiServices(builder.Configuration);

var app = builder.Build();

app.MapHealthChecks("/health");

//app.ApplyMigrations<OrderDbContext>();
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
