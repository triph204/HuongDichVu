# ?? Order Microservice API Documentation

## ?? Base URL
```
http://localhost:5001/api
```

## ?? Endpoints

### 1. Orders

#### Get All Orders
```http
GET /orders
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "orderNumber": "ORD-240101120000",
    "tableId": 1,
    "tableName": "Bàn 1",
    "totalAmount": 250000,
    "status": "HoanThanh",
    "customerNote": "Không cay",
    "createdAt": "2024-01-01T12:00:00",
    "updatedAt": "2024-01-01T12:30:00",
    "completedAt": "2024-01-01T12:45:00",
    "details": [
      {
        "id": 1,
        "orderId": 1,
        "dishId": 1,
        "dishName": "Ph? Bò",
        "quantity": 2,
        "unitPrice": 50000,
        "totalPrice": 100000,
        "dishNote": "Thêm n??c m?m"
      }
    ]
  }
]
```

---

#### Get Order by ID
```http
GET /orders/{id}
```

**Parameters:**
- `id` (int, required) - Order ID

**Response (200 OK):**
```json
{
  "id": 1,
  "orderNumber": "ORD-240101120000",
  "tableId": 1,
  "tableName": "Bàn 1",
  "totalAmount": 250000,
  "status": "HoanThanh",
  "customerNote": "Không cay",
  "createdAt": "2024-01-01T12:00:00",
  "details": []
}
```

**Errors:**
- `404 Not Found` - ??n hàng không t?n t?i

---

#### Get Orders by Status
```http
GET /orders/status/{status}
```

**Parameters:**
- `status` (string, required) - Order status

**Valid Status Values:**
- `ChoXacNhan` - Ch? xác nh?n
- `DaXacNhan` - ?ã xác nh?n
- `DangChuan` - ?ang chu?n b?
- `HoanThanh` - Hoàn thành
- `Huy` - H?y

**Example:**
```http
GET /orders/status/ChoXacNhan
```

---

#### Get Orders by Table
```http
GET /orders/table/{tableId}
```

**Parameters:**
- `tableId` (int, required) - Table ID

---

#### Create Order
```http
POST /orders
```

**Request Body:**
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
      "unitPrice": 50000,
      "dishNote": "Thêm n??c m?m"
    },
    {
      "dishId": 2,
      "dishName": "Bún Ch?",
      "quantity": 1,
      "unitPrice": 150000
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "orderNumber": "ORD-240101120000",
  "tableId": 1,
  "tableName": "Bàn 1",
  "totalAmount": 250000,
  "status": "ChoXacNhan",
  "customerNote": "Không cay",
  "createdAt": "2024-01-01T12:00:00",
  "details": [...]
}
```

---

#### Update Order
```http
PUT /orders/{id}
```

**Request Body:**
```json
{
  "customerNote": "Không cay, ít mu?i",
  "totalAmount": 280000
}
```

**Response (200 OK):**
```json
{
  "message": "C?p nh?t ??n hàng thành công"
}
```

---

#### Update Order Status
```http
PUT /orders/{id}/status
```

**Request Body:**
```json
{
  "newStatus": "DaXacNhan"
}
```

**Response (200 OK):**
```json
{
  "message": "C?p nh?t tr?ng thái thành công"
}
```

---

#### Delete Order
```http
DELETE /orders/{id}
```

**Response (200 OK):**
```json
{
  "message": "Xóa ??n hàng thành công"
}
```

---

### 2. Order Details

#### Get Order Details
```http
GET /orders/{orderId}/orderdetails
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "orderId": 1,
    "dishId": 1,
    "dishName": "Ph? Bò",
    "quantity": 2,
    "unitPrice": 50000,
    "totalPrice": 100000,
    "dishNote": "Thêm n??c m?m"
  }
]
```

---

#### Add Dish to Order
```http
POST /orders/{orderId}/orderdetails
```

**Request Body:**
```json
{
  "dishId": 3,
  "dishName": "C?m Gà",
  "quantity": 1,
  "unitPrice": 60000,
  "dishNote": "Gà quay"
}
```

**Response (201 Created):**
```json
{
  "id": 3,
  "orderId": 1,
  "dishId": 3,
  "dishName": "C?m Gà",
  "quantity": 1,
  "unitPrice": 60000,
  "totalPrice": 60000,
  "dishNote": "Gà quay"
}
```

---

#### Update Dish Quantity
```http
PUT /orders/{orderId}/orderdetails/{detailId}
```

**Request Body:**
```json
{
  "quantity": 5
}
```

**Response (200 OK):**
```json
{
  "message": "C?p nh?t s? l??ng thành công"
}
```

---

#### Remove Dish from Order
```http
DELETE /orders/{orderId}/orderdetails/{detailId}
```

**Response (200 OK):**
```json
{
  "message": "Xóa món thành công"
}
```

---

### 3. Health Check

#### Health Status
```http
GET /health
```

**Response (200 OK):**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T12:00:00Z",
  "service": "Order Microservice",
  "version": "1.0.0"
}
```

#### Readiness Probe
```http
GET /health/ready
```

#### Liveness Probe
```http
GET /health/live
```

---

## ?? SignalR Events

**WebSocket Endpoint:** `ws://localhost:5001/orderHub`

### Client Events

#### ReceiveNewOrder
```javascript
connection.on("ReceiveNewOrder", (order) => {
    console.log("??n hàng m?i:", order);
});
```

#### OrderStatusChanged
```javascript
connection.on("OrderStatusChanged", (data) => {
    console.log("Tr?ng thái thay ??i:", data);
    // {
    //   orderId: 1,
    //   soDon: "ORD-240101120000",
    //   banId: 1,
    //   soBan: "Bàn 1",
    //   oldStatus: "ChoXacNhan",
    //   newStatus: "DaXacNhan",
    //   ngayCapNhat: "2024-01-01T12:05:00"
    // }
});
```

#### ReceiveOrderUpdate
```javascript
connection.on("ReceiveOrderUpdate", (message) => {
    console.log("C?p nh?t ??n:", message);
});
```

---

## ?? Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Missing required parameter: tableId",
  "data": null,
  "errors": null
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Resource not found",
  "data": null,
  "errors": null
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Internal server error",
  "data": null,
  "errors": ["Database connection failed"]
}
```

---

## ?? Usage Examples

### cURL

#### Create Order
```bash
curl -X POST http://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{
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
  }'
```

### JavaScript (Fetch)

```javascript
const createOrder = async () => {
  const response = await fetch('http://localhost:5001/api/orders', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      tableId: 1,
      tableName: "Bàn 1",
      items: [
        {
          dishId: 1,
          dishName: "Ph? Bò",
          quantity: 2,
          unitPrice: 50000
        }
      ]
    })
  });

  const order = await response.json();
  console.log(order);
};
```

### C# (HttpClient)

```csharp
var client = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };

var order = new
{
    tableId = 1,
    tableName = "Bàn 1",
    items = new[] {
        new {
            dishId = 1,
            dishName = "Ph? Bò",
            quantity = 2,
            unitPrice = 50000m
        }
    }
};

var response = await client.PostAsJsonAsync("/api/orders", order);
var result = await response.Content.ReadAsAsync<dynamic>();
```

---

## ?? Support

- **API Version:** 1.0.0
- **Last Updated:** 2024-01-01
- **Environment:** Development / Production
