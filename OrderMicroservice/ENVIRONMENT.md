# Order Microservice - Environment Configuration

## Development Environment

```bash
# PowerShell / Windows
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Server=.;Database=OrderMicroserviceDb;Trusted_Connection=true;TrustServerCertificate=true;"

# Bash / Linux / Mac
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Server=localhost;Database=OrderMicroserviceDb;User Id=sa;Password=YourPassword@123;TrustServerCertificate=true;"

dotnet run --project Order.API
```

## Production Environment

```bash
# Docker Compose
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Kubernetes
kubectl apply -f k8s/order-microservice-deployment.yaml
```

## Environment Variables

| Variable | Value | Description |
|----------|-------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Development/Production | Environment mode |
| `ASPNETCORE_URLS` | http://+:5001 | API listen URL |
| `ConnectionStrings__DefaultConnection` | DB connection string | Database connection |
| `Logging__LogLevel__Default` | Information/Debug | Log level |

## appsettings Override

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=OrderDb;..."
  }
}
```
