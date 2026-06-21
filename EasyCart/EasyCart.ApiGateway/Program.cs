using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using EasyCart.SharedLibrary.DependencyInjection;
using EasyCart.ApiGateway.Middlewares;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot().AddCacheManager(x => x.WithDictionaryHandle());
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

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "EasyCart API Gateway Running");

app.UseMiddleware<AttachApiGatewaySignarureToRequest>();
await app.UseOcelot();

app.Run();

