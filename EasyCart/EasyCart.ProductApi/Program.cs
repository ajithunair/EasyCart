using EasyCart.ProductApi.Data;
using EasyCart.ProductApi.DependencyInjection;
using EasyCart.SharedLibrary.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("RedisConnection");

// Add services to the container.

builder.Services.AddControllers();
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
