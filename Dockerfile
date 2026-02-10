# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for dependency caching
COPY ["src/Inventory.Domain/Inventory.Domain.csproj", "src/Inventory.Domain/"]
COPY ["src/Inventory.Application/Inventory.Application.csproj", "src/Inventory.Application/"]
COPY ["src/Inventory.Infrastructure/Inventory.Infrastructure.csproj", "src/Inventory.Infrastructure/"]
COPY ["src/Inventory.API/Inventory.API.csproj", "src/Inventory.API/"]

# Restore dependencies
RUN dotnet restore "src/Inventory.API/Inventory.API.csproj"

# Copy the rest of the source code
COPY . .

# Publish the API
WORKDIR "/src/src/Inventory.API"
RUN dotnet publish "Inventory.API.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Inventory.API.dll"]