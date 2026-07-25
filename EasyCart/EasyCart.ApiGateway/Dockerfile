# ======================
# Build Stage
# ======================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "EasyCart.ApiGateway/EasyCart.ApiGateway.csproj"

RUN dotnet publish "EasyCart.ApiGateway/EasyCart.ApiGateway.csproj" \
	-c Release \
	-o /app/publish

# ======================
# Runtime Stage
# ======================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "EasyCart.ApiGateway.dll"]