# ?? Quick Start Guide - Order Microservice

## ?? Yêu c?u h? th?ng

- **.NET 10.0 SDK** ho?c cao h?n
- **SQL Server** (2019 tr? lên) ho?c **SQL Server Express**
- **Docker** (tùy ch?n, cho development)
- **Visual Studio 2022** ho?c **VS Code**

---

## ? Setup nhanh (5 phút)

### B??c 1: Clone/Copy d? án

```bash
cd OrderFoodSystem
# Copy th? m?c OrderMicroservice n?u ch?a có
```

### B??c 2: C?u hình Database

M? `OrderMicroservice/Order.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OrderMicroserviceDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**Ho?c s? d?ng SQL Server trên Docker:**

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword@123" \
  -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest

# C?p nh?t connection string:
# "Server=localhost;Database=OrderMicroserviceDb;User Id=sa;Password=YourPassword@123;TrustServerCertificate=true;"
```

### B??c 3: Restore & Build

```bash
cd OrderMicroservice
dotnet restore
dotnet build
```

### B??c 4: Migrate Database

```bash
# T? trong th? m?c OrderMicroservice
dotnet ef database update --project Order.Infrastructure --startup-project Order.API
```

? Khi thành công, b?n s? th?y:
```
Done. 2 migrations successfully applied.
```

### B??c 5: Ch?y API

```bash
dotnet run --project Order.API
```

? API ?ang ch?y:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

---

## ?? Test API

### Swagger UI
Truy c?p: **https://localhost:5001/swagger**

### Ho?c dùng cURL

```bash
# Get all orders
curl https://localhost:5001/api/orders

# Create order
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
  }'

# Health check
curl https://localhost:5001/health
```

---

## ?? Docker Deployment

### Development (v?i Docker Compose)

```bash
cd OrderMicroservice
docker-compose up -d
```

**Ki?m tra:**
```bash
# View logs
docker-compose logs -f order-api

# Stop
docker-compose down
```

### Production (Build image)

```bash
docker build -t order-microservice:latest .
docker run -p 5001:8080 \
  -e "ConnectionStrings__DefaultConnection=Server=db.example.com;Database=OrderDb;..." \
  order-microservice:latest
```

---

## ?? Common Tasks

### Reset Database

```bash
# Xóa database
dotnet ef database drop --project Order.Infrastructure --startup-project Order.API

# T?o l?i
dotnet ef database update --project Order.Infrastructure --startup-project Order.API
```

### View Migration History

```bash
dotnet ef migrations list --project Order.Infrastructure
```

### Add New Migration

```bash
dotnet ef migrations add {MigrationName} --project Order.Infrastructure --startup-project Order.API
```

### Run Tests

```bash
dotnet test Order.Tests/Order.Tests.csproj
```

---

## ?? Tích h?p v?i Frontend

### WebClient (JavaScript)

```javascript
// appsettings hay config
const API_BASE = "http://localhost:5001";

// Create order
const createOrder = async (order) => {
  const res = await fetch(`${API_BASE}/api/orders`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(order)
  });
  return res.json();
};

// SignalR connection
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API_BASE}/orderHub`)
  .withAutomaticReconnect()
  .build();

connection.on("ReceiveNewOrder", (order) => {
  console.log("?? New order:", order);
});

connection.start();
```

### C# Client

```csharp
// HttpClient Factory
services.AddHttpClient<IOrderClient>(client => 
{
    client.BaseAddress = new Uri("http://localhost:5001");
});

// Call API
var order = new CreateOrderDto { ... };
var response = await client.PostAsJsonAsync("/api/orders", order);
var result = await response.Content.ReadAsAsync<OrderDto>();
```

---

## ?? Database Schema

### Orders Table
| Column | Type | Nullable |
|--------|------|----------|
| Id | int | No (PK) |
| OrderNumber | nvarchar(50) | No |
| TableId | int | No |
| TableName | nvarchar(50) | No |
| TotalAmount | decimal(10,2) | No |
| Status | nvarchar(50) | No |
| CustomerNote | nvarchar(500) | Yes |
| CreatedAt | datetime2 | No |
| UpdatedAt | datetime2 | Yes |
| CompletedAt | datetime2 | Yes |

### OrderDetails Table
| Column | Type | Nullable |
|--------|------|----------|
| Id | int | No (PK) |
| OrderId | int | No (FK) |
| DishId | int | No |
| DishName | nvarchar(200) | No |
| Quantity | int | No |
| UnitPrice | decimal(10,2) | No |
| TotalPrice | decimal(10,2) | No |
| DishNote | nvarchar(300) | Yes |

---

## ?? Troubleshooting

### ? "Connection string is not configured"
```
? Ki?m tra appsettings.json
? ??m b?o SQL Server ?ang ch?y
```

### ? "Migration failed: Index already exists"
```
? Ch?y: dotnet ef database drop
? Sau ?ó: dotnet ef database update
```

### ? Port 5001 ?ang ???c dùng
```bash
# Thay ??i port trong launchSettings.json
# ho?c kill process ?ang dùng
lsof -ti :5001 | xargs kill -9
```

### ? CORS error t? frontend
```
? Microservice ?ã c?u hình CORS "AllowAll"
? Ki?m tra URL endpoint
? Xem CORS policy trong Program.cs
```

---

## ?? File C?u hình

### appsettings.json
- Logging levels
- Connection strings
- Swagger docs

### appsettings.Development.json
- Development-specific settings
- Enable detailed logging
- Seed data settings

### docker-compose.yml
- SQL Server container
- Order API service
- Volumes & networks

---

## ?? Tài li?u thêm

- `README.md` - T?ng quan d? án
- `API_DOCUMENTATION.md` - Chi ti?t API
- `INTEGRATION_GUIDE.md` - H??ng d?n tích h?p
- `Order.API/Controllers/` - Controller examples
- `Order.Tests/` - Unit tests

---

## ? Checklist hoàn t?t

- [ ] .NET 10.0 SDK installed
- [ ] SQL Server running
- [ ] `dotnet restore` thành công
- [ ] Database migration hoàn t?t
- [ ] API ch?y trên port 5001
- [ ] Swagger UI accessible
- [ ] Test endpoint thành công
- [ ] Frontend config updated
- [ ] SignalR connected
- [ ] Ready for production! ??

---

## ?? Questions?

Xem **API_DOCUMENTATION.md** ho?c tham kh?o code examples trong Controllers.

**Happy coding! ??**
