using EasyCart.InventoryApi.Data;
using EasyCart.InventoryApi.DependencyInjection;
using EasyCart.InventoryApi.RabbitMQ.Consumers;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
