var builder = DistributedApplication.CreateBuilder(args);

// Add services to the container.

var authApi = builder.AddProject<Projects.EasyCart_AuthApi>("auth-api");
var inventoryApi = builder.AddProject<Projects.EasyCart_InventoryApi>("inventory-api");
var orderApi = builder.AddProject<Projects.EasyCart_OrderApi>("order-api");
var productApi = builder.AddProject<Projects.EasyCart_ProductApi>("product-api");

builder.AddProject<Projects.EasyCart_ApiGateway>("api-gateway")
    .WithReference(authApi)
    .WithReference(inventoryApi)
    .WithReference(orderApi)
    .WithReference(productApi);

builder.Build().Run();
