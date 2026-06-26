using EasyCart.InventoryApi.DependencyInjection;
using EasyCart.InventoryApi.RabbitMQ.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
