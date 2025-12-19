FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["Order.API/Order.API.csproj", "Order.API/"]
COPY ["Order.Domain/Order.Domain.csproj", "Order.Domain/"]
COPY ["Order.Infrastructure/Order.Infrastructure.csproj", "Order.Infrastructure/"]
COPY ["Order.Application/Order.Application.csproj", "Order.Application/"]

# Restore
RUN dotnet restore "Order.API/Order.API.csproj"

# Copy source code
COPY . .

# Build
RUN dotnet build "Order.API/Order.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "Order.API/Order.API.csproj" -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Order.API.dll"]
