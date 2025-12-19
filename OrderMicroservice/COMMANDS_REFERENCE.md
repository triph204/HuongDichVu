# ??? Useful Commands - Order Microservice

## ?? Quick Reference

### Restore & Build
```bash
# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Build in Release mode
dotnet build -c Release
```

### Database Operations
```bash
# Create/Update database
dotnet ef database update --project Order.Infrastructure --startup-project Order.API

# Drop database (careful!)
dotnet ef database drop --project Order.Infrastructure --startup-project Order.API

# List migrations
dotnet ef migrations list --project Order.Infrastructure

# Add new migration
dotnet ef migrations add MigrationName --project Order.Infrastructure --startup-project Order.API

# Remove last migration
dotnet ef migrations remove --project Order.Infrastructure --startup-project Order.API

# Get migration SQL
dotnet ef migrations script --project Order.Infrastructure --startup-project Order.API
```

### Running API
```bash
# Run with default profile
dotnet run --project Order.API

# Run HTTPS
dotnet run --project Order.API --launch-profile https

# Run HTTP
dotnet run --project Order.API --launch-profile http

# Run with specific environment
dotnet run --project Order.API --environment Development
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "OrderServiceTests"

# Run with verbose output
dotnet test --verbosity detailed

# Run and generate coverage
dotnet test /p:CollectCoverage=true
```

### Docker
```bash
# Build Docker image
docker build -t order-microservice:1.0 .

# Run container
docker run -p 5001:8080 order-microservice:1.0

# Docker Compose - start
docker-compose up -d

# Docker Compose - stop
docker-compose down

# Docker Compose - logs
docker-compose logs -f order-api

# Remove all containers
docker-compose down -v
```

### Nuget Package Management
```bash
# Add package
dotnet add Order.API package PackageName

# Remove package
dotnet remove Order.API package PackageName

# Update packages
dotnet nuget update

# List outdated packages
dotnet list package --outdated
```

### Cleaning
```bash
# Clean build artifacts
dotnet clean

# Remove bin & obj folders
Remove-Item -Path "OrderMicroservice/**/bin" -Recurse -Force
Remove-Item -Path "OrderMicroservice/**/obj" -Recurse -Force

# Clean NuGet cache
dotnet nuget locals all --clear
```

### Publishing
```bash
# Publish for Windows
dotnet publish -c Release -r win-x64 -o ./publish

# Publish for Linux
dotnet publish -c Release -r linux-x64 -o ./publish

# Publish framework-dependent
dotnet publish -c Release -o ./publish
```

### Diagnostic Commands
```bash
# Check .NET version
dotnet --version

# List installed SDKs
dotnet --list-sdks

# List installed runtimes
dotnet --list-runtimes

# Check project references
dotnet list reference

# Analyze dependencies
dotnet list package
```

---

## ?? Useful PowerShell Commands

### Port Management
```powershell
# Check what's using port 5001
netstat -ano | findstr :5001

# Kill process on port 5001
$process = Get-NetTCPConnection -LocalPort 5001 | Select-Object OwningProcess
Stop-Process -Id $process.OwningProcess -Force

# Or simpler
Get-Process -Id (Get-NetTCPConnection -LocalPort 5001).OwningProcess | Stop-Process
```

### File Operations
```powershell
# Find all .csproj files
Get-ChildItem -Filter "*.csproj" -Recurse

# Count lines of code
Get-ChildItem -Filter "*.cs" -Recurse | 
    Measure-Object -Line -Sum -Average | 
    Select Lines

# Find large files
Get-ChildItem -Recurse | 
    Where-Object {$_.Length -gt 1MB} | 
    Sort-Object Length -Descending
```

---

## ?? Git Commands

```bash
# Initialize repo
git init

# Add all files
git add .

# Commit
git commit -m "Initial Order Microservice"

# Create branch
git branch feature/new-feature

# Switch branch
git checkout feature/new-feature

# Merge
git merge feature/new-feature

# Push to remote
git push origin main

# Pull from remote
git pull origin main
```

---

## ?? Debugging

### Debug in VS Code
```json
// .vscode/launch.json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/Order.API/bin/Debug/net9.0/Order.API.dll",
      "args": [],
      "cwd": "${workspaceFolder}/Order.API",
      "stopAtEntry": false,
      "console": "integratedTerminal"
    }
  ]
}
```

### Debug Logging
```csharp
logger.LogDebug("Debug message");
logger.LogInformation("Info message");
logger.LogWarning("Warning message");
logger.LogError("Error message");
logger.LogCritical("Critical message");
```

---

## ?? Testing API with cURL

```bash
# GET all orders
curl https://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -k

# GET single order
curl https://localhost:5001/api/orders/1 -k

# POST create order
curl -X POST https://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "tableId": 1,
    "tableName": "Bàn 1",
    "items": [{
      "dishId": 1,
      "dishName": "Ph? Bò",
      "quantity": 2,
      "unitPrice": 50000
    }]
  }' \
  -k

# PUT update order
curl -X PUT https://localhost:5001/api/orders/1 \
  -H "Content-Type: application/json" \
  -d '{"customerNote": "No spicy"}' \
  -k

# DELETE order
curl -X DELETE https://localhost:5001/api/orders/1 -k

# Health check
curl https://localhost:5001/health -k
```

---

## ?? Testing with Postman

1. Import collection:
   - File ? Import ? Select `Order.Microservice.postman_collection.json`

2. Set environment:
   - Select `Development` environment
   - API URL: `https://localhost:5001`

3. Run tests:
   - Collections ? Order Microservice ? Run

---

## ?? Performance Monitoring

```bash
# Check memory usage
Get-Process dotnet | Select-Object Name, WorkingSet

# CPU usage
Get-Process | Where-Object {$_.Name -eq "dotnet"} | Select-Object Name, CPU, Handles
```

---

## ?? Troubleshooting Commands

```bash
# Check if port is available
Test-NetConnection -ComputerName localhost -Port 5001

# Check active connections
netstat -an | findstr :5001

# View process details
tasklist /fi "ImageName eq dotnet.exe"

# Kill process by name
taskkill /IM dotnet.exe /F

# Check SQL Server connection
sqlcmd -S localhost -Q "SELECT @@VERSION"
```

---

## ?? Common Workflow

```bash
# 1. Start fresh
dotnet clean
Remove-Item -Path "*/bin" -Recurse -Force
Remove-Item -Path "*/obj" -Recurse -Force

# 2. Restore & build
dotnet restore
dotnet build

# 3. Database
dotnet ef database drop
dotnet ef database update --project Order.Infrastructure --startup-project Order.API

# 4. Run API
dotnet run --project Order.API

# 5. Test (in another terminal)
curl https://localhost:5001/health -k
```

---

## ?? Environment Setup

### Windows PowerShell
```powershell
# Set environment variable
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=...;..."

# Verify
$env:ASPNETCORE_ENVIRONMENT
```

### Linux/Mac Bash
```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Server=...;Database=...;..."

echo $ASPNETCORE_ENVIRONMENT
```

---

## ?? Solution Management

```bash
# List all projects
dotnet sln list

# Add project to solution
dotnet sln add Order.API/Order.API.csproj

# Remove project from solution
dotnet sln remove Order.API/Order.API.csproj
```

---

## ?? Quick Commands by Task

### Just Want to Run?
```bash
dotnet ef database update --project Order.Infrastructure --startup-project Order.API
dotnet run --project Order.API
```

### Just Want to Test?
```bash
dotnet test
```

### Just Want to Deploy?
```bash
docker-compose up -d
```

### Want to Reset Everything?
```bash
docker-compose down -v
dotnet clean
dotnet restore
dotnet ef database update --project Order.Infrastructure --startup-project Order.API
dotnet run --project Order.API
```

---

**?? Tip:** Create a batch file or script for frequently used commands!

Example: `run.ps1`
```powershell
# Setup & run
dotnet clean
dotnet restore
dotnet build
dotnet ef database update --project Order.Infrastructure --startup-project Order.API
dotnet run --project Order.API
```

Then just: `./run.ps1`

---

*Version: 1.0.0*
*Last Updated: 2024*
