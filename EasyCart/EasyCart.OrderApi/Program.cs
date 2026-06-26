using EasyCart.OrderApi.DependencyInjection;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rabbitSection = builder.Configuration.GetSection("RabbitMQ");

//Configure MassTransit
builder.Services.AddMassTransit(x =>
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
});

builder.Services.AddOrderApiServices(builder.Configuration);

var app = builder.Build();

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
