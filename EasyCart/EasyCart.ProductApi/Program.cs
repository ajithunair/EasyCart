using EasyCart.ProductApi.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("RedisConnection");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProductApiServices(builder.Configuration);

builder.Services.AddStackExchangeRedisCache(options =>
{
    // The connection string pointing to localhost:6379 (local) or Azure Redis
    options.Configuration = connectionString;

    // Optional prefix so your keys look like "ProductService_Product:1" in Redis Insight
    options.InstanceName = "ProductService_";
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseProductApiMiddlewares();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
