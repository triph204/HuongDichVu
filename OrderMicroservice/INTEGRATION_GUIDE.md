# ?? H??ng d?n Tích h?p Order Microservice vào Backend Chính

## 1?? C?u trúc Th? m?c

```
OrderFoodSystem/
??? RestaurantBackend-backend/      (Backend chính c?)
??? WebClient/                       (App khách hàng)
??? WebAdmin/                        (App qu?n tr?)
??? OrderMicroservice/              (?? Microservice m?i)
    ??? Order.API/
    ??? Order.Domain/
    ??? Order.Infrastructure/
    ??? Order.Application/
    ??? OrderMicroservice.sln
```

## 2?? B??c Setup

### Trên máy ch?:

#### Option A: Ch?y riêng (Recommended cho microservices)
```bash
cd OrderMicroservice
dotnet restore
dotnet ef database update --project Order.Infrastructure
dotnet run --project Order.API
```

**K?t qu?:** API ch?y trên `https://localhost:5001`

#### Option B: Docker Compose (Production)
```bash
cd OrderMicroservice
docker-compose up -d
```

## 3?? Thay ??i c?u hình

### WebClient - C?p nh?t API endpoint

File: `WebClient/wwwroot/js/cart.js`

**C?:**
```javascript
const API_BASE_URL = "http://localhost:5137";
const API_ORDER_URL = `${API_BASE_URL}/api/DonHang`;
```

**M?i:**
```javascript
const API_BASE_URL = "http://localhost:5001"; // Order Microservice
const API_ORDER_URL = `${API_BASE_URL}/api/orders`;
```

### WebAdmin - C?p nh?t API endpoint

File: `WebAdmin/appsettings.json`

```json
{
  "Apis": {
    "OrderApiUrl": "http://localhost:5001/api"
  }
}
```

## 4?? C?p nh?t Backend Chính (Optional)

N?u backend chính v?n c?n m?t API gateway ?? g?i Order Microservice:

### T?o Order API Client

File: `RestaurantBackend-backend/Services/OrderServiceClient.cs`

```csharp
public interface IOrderServiceClient
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task<OrderDto> GetOrderAsync(int id);
    Task<List<OrderDto>> GetAllOrdersAsync();
}

public class OrderServiceClient : IOrderServiceClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5001/api";

    public OrderServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/orders", dto);
        return await response.Content.ReadAsAsync<OrderDto>();
    }

    // Các method khác...
}
```

### ??ng ký trong Program.cs

```csharp
builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>()
    .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));
```

## 5?? API Mapping

### Old API ? New API

| Old | New |
|-----|-----|
| `POST /api/DonHang` | `POST /api/orders` |
| `GET /api/DonHang` | `GET /api/orders` |
| `GET /api/DonHang/{id}` | `GET /api/orders/{id}` |
| `PUT /api/DonHang/{id}` | `PUT /api/orders/{id}` |
| `DELETE /api/DonHang/{id}` | `DELETE /api/orders/{id}` |

### DTOs Mapping

**Request Body**

**Old:**
```json
{
  "BanId": 1,
  "SoBan": "1",
  "TenBan": "Bàn 1",
  "GhiChuKhach": "Không cay",
  "MonOrder": [
    { "MonId": 1, "SoLuong": 2 }
  ]
}
```

**New:**
```json
{
  "tableId": 1,
  "tableName": "Bàn 1",
  "customerNote": "Không cay",
  "items": [
    {
      "dishId": 1,
      "dishName": "Ph? Bò",
      "quantity": 2,
      "unitPrice": 50000
    }
  ]
}
```

## 6?? SignalR Integration

### WebClient - K?t n?i SignalR

File: `WebClient/wwwroot/js/order.js`

**C?:**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5137/orderHub")
    .build();
```

**M?i:**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5001/orderHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNewOrder", (order) => {
    console.log("?? ??n hàng m?i:", order);
    updateOrderList();
});

connection.on("OrderStatusChanged", (data) => {
    console.log("?? Tr?ng thái thay ??i:", data);
    refreshOrder(data.orderId);
});
```

## 7?? Database Migration

### T?o Order DB riêng

```bash
cd OrderMicroservice/Order.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Connection String:** `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=OrderMicroserviceDb;Trusted_Connection=true;"
  }
}
```

## 8?? Monitoring & Logging

### View logs

```bash
# Development
dotnet run --project Order.API

# Production (Docker)
docker logs ordermicroservice-order-api-1
```

### Health Check

```
GET http://localhost:5001/health
```

## 9?? Testing

### Postman Collection

T?o file `Order.Microservice.postman_collection.json`

```json
{
  "info": {
    "name": "Order Microservice",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Create Order",
      "request": {
        "method": "POST",
        "url": "http://localhost:5001/api/orders",
        "body": {
          "mode": "raw",
          "raw": "{...}"
        }
      }
    }
  ]
}
```

## ?? Troubleshooting

### ? Connection refused
```
? Ki?m tra firewall
? ??m b?o port 5001 kh? d?ng
```

### ? Database connection error
```
? Ki?m tra connection string
? Ch?y: dotnet ef database update
```

### ? CORS error
```
? Microservice ?ã config CORS "AllowAll"
? Ki?m tra URL khách hàng
```

## ? Checklist Tri?n khai

- [ ] Clone/Copy OrderMicroservice
- [ ] C?u hình Connection String
- [ ] Ch?y database migration
- [ ] Update API endpoints trong clients
- [ ] Test API b?ng Postman
- [ ] K?t n?i SignalR
- [ ] Deploy (Docker ho?c standalone)
- [ ] Monitor logs
- [ ] Backup database c?
- [ ] Decommission old Order API (n?u c?n)

## ?? Support

Xem `OrderMicroservice/README.md` ?? bi?t thêm chi ti?t API.
