# ??? ORDER MICROSERVICE - CLEAN ARCHITECTURE

## ?? T?NG QUAN

Microservice qu?n lý ??n hàng nhà hàng ???c xây d?ng theo **Clean Architecture**, áp d?ng **SOLID Principles**, **Design Patterns** và **Clean Code**.

---

## ? CÁC C?I TI?N ?Ã TH?C HI?N

### ?? **1. DOMAIN LAYER (Core Business Logic)**

#### ? **Domain Entities v?i Encapsulation**
- ? Private constructors và setters
- ? Static Factory Methods: `OrderEntity.Create()`
- ? Domain Logic trong Entity: `AddOrderDetail()`, `UpdateStatus()`, `RemoveOrderDetail()`
- ? Business Rules validation: `CanModifyOrder()`, `IsValidStatusTransition()`
- ? Read-only collections: `IReadOnlyCollection<OrderDetailEntity>`

**File:** `Order.Domain/Entities/OrderEntity.cs`, `OrderDetailEntity.cs`

#### ? **Value Objects (Immutable)**
- ? `OrderNumber` - Mã ??n hàng v?i validation
- ? `Money` - X? lý ti?n t? an toàn
- ? Equality comparison theo value
- ? Self-validation

**File:** `Order.Domain/ValueObjects/OrderNumber.cs`, `Money.cs`

#### ? **Domain Constants**
- ? `OrderStatus` - Tránh magic strings

**File:** `Order.Domain/Constants/OrderStatus.cs`

---

### ?? **2. APPLICATION LAYER (Business Logic)**

#### ? **DTOs (Data Transfer Objects)**
- ? `OrderDto`, `OrderDetailDto` - Response DTOs
- ? `CreateOrderDto`, `CreateOrderDetailDto` - Create DTOs
- ? `UpdateOrderDto`, `UpdateOrderStatusDto` - Update DTOs
- ? `AddOrderDetailDto`, `UpdateOrderDetailDto` - Detail DTOs
- ? Data Annotations validation

**Folder:** `Order.Application/DTOs/`

#### ? **Mappers (Single Responsibility)**
- ? `OrderMapper` - Entity ?? DTO mapping
- ? `OrderDetailMapper` - Detail mapping
- ? Static methods, không side effects

**Folder:** `Order.Application/Mappers/`

#### ? **Validators (Separation of Concerns)**
- ? `CreateOrderDtoValidator` - Validate business rules
- ? `OrderStatusValidator` - Validate tr?ng thái h?p l?
- ? Clear error messages

**Folder:** `Order.Application/Validators/`

#### ? **Result Pattern**
- ? `Result<T>` - Explicit error handling
- ? Thay th? exceptions cho business logic failures

**File:** `Order.Application/Common/Result.cs`

#### ? **Services (Business Logic)**
- ? `OrderService`, `OrderDetailService`
- ? Dependency Injection v?i null checks
- ? S? d?ng Mappers và Domain Logic
- ? Clean error handling

**Folder:** `Order.Application/Services/`

---

### ?? **3. INFRASTRUCTURE LAYER (Data Access)**

#### ? **Generic Repository Pattern**
- ? `IRepository<T>`, `Repository<T>` - Base CRUD operations
- ? Tránh code duplication (DRY)
- ? Easy to mock for testing

**File:** `Order.Infrastructure/Repositories/IRepository.cs`, `Repository.cs`

#### ? **Specific Repositories**
- ? `OrderRepository` extends `Repository<OrderEntity>`
- ? `OrderDetailRepository` extends `Repository<OrderDetailEntity>`
- ? Ch? implement custom queries

**File:** `Order.Infrastructure/Repositories/OrderRepository.cs`, etc.

#### ? **Unit of Work Pattern**
- ? `IUnitOfWork`, `UnitOfWork`
- ? Transaction management
- ? Coordinating multiple repositories

**File:** `Order.Infrastructure/Repositories/IUnitOfWork.cs`, `UnitOfWork.cs`

#### ? **DbContext Configuration**
- ? H? tr? private constructors và setters
- ? EF Core conventions tuân th? DDD

**File:** `Order.Infrastructure/Data/OrderDbContext.cs`

#### ? **Database Seeding**
- ? S? d?ng Factory Methods t? Domain
- ? Seed data cho Development environment

**File:** `Order.Infrastructure/Seeds/DatabaseSeeder.cs`

#### ? **Migration Helper**
- ? Auto-apply migrations khi start
- ? Không ph? thu?c vào ASP.NET Core (Clean Architecture)

**File:** `Order.Infrastructure/Migrations/MigrationHelper.cs`

---

### ?? **4. API LAYER (Presentation)**

#### ? **Controllers (RESTful Best Practices)**
- ? `OrdersController` - CRUD operations
- ? Dependency Injection v?i ILogger
- ? Input validation v?i Validators
- ? Proper HTTP status codes (200, 201, 400, 404)
- ? API Documentation v?i `[ProducesResponseType]`
- ? Structured error responses
- ? Logging cho m?i action

**File:** `Order.API/Controllers/OrdersController.cs`

#### ? **Dependency Injection Configuration**
- ? Organized DI registration
- ? Scoped lifetime cho Repositories và Services
- ? CORS configuration
- ? Swagger documentation
- ? SignalR integration

**File:** `Order.API/Program.cs`

---

## ?? SOLID PRINCIPLES ÁP D?NG

| Nguyên t?c | Mô t? | Ví d? |
|-----------|-------|-------|
| **S** - Single Responsibility | M?i class có 1 nhi?m v? duy nh?t | `OrderMapper` ch? làm mapping, `OrderService` ch? làm business logic |
| **O** - Open/Closed | M? cho m? r?ng, ?óng cho s?a ??i | `Repository<T>` có th? extend mà không c?n modify |
| **L** - Liskov Substitution | Derived class thay th? ???c base class | `OrderRepository` thay th? `Repository<OrderEntity>` |
| **I** - Interface Segregation | Interface nh?, t?p trung | `IOrderRepository` ch? ch?a methods c?n thi?t |
| **D** - Dependency Inversion | Ph? thu?c vào abstractions | Services ph? thu?c vào `IOrderRepository`, không ph?i `OrderRepository` |

---

## ?? DESIGN PATTERNS S? D?NG

### 1. **Repository Pattern**
- Abstraction over data access
- `IRepository<T>`, `Repository<T>`

### 2. **Unit of Work Pattern**
- Transaction management
- `IUnitOfWork`, `UnitOfWork`

### 3. **Factory Pattern**
- Static Factory Methods
- `OrderEntity.Create()`, `OrderDetailEntity.Create()`

### 4. **Result Pattern**
- Explicit error handling
- `Result<T>`

### 5. **Mapper Pattern**
- Object-to-object mapping
- `OrderMapper`, `OrderDetailMapper`

### 6. **Dependency Injection Pattern**
- Constructor injection throughout

### 7. **Aggregate Root Pattern (DDD)**
- `OrderEntity` là Aggregate Root
- `OrderDetailEntity` là child entity

---

## ?? CLEAN CODE PRINCIPLES

? **Meaningful Names** - Tên bi?n, method rõ ràng  
? **Small Functions** - M?i method ~10-20 lines  
? **Comments Where Needed** - XML docs cho public APIs  
? **Error Handling** - Try-catch ? boundaries, domain exceptions cho business rules  
? **DRY** - Không l?p code (Base Repository, Mappers, Validators)  
? **KISS** - Gi? ??n gi?n, d? hi?u  

---

## ?? C?U TRÚC FOLDERS

```
OrderMicroservice/
?
??? Order.API/                          # ?? API Layer (Controllers, Middleware)
?   ??? Controllers/
?   ?   ??? OrdersController.cs        # ? RESTful API v?i validation, logging
?   ?   ??? OrderDetailsController.cs
?   ??? Middleware/
?   ?   ??? GlobalExceptionMiddleware.cs
?   ??? Hubs/
?   ?   ??? OrderHub.cs
?   ??? Program.cs                      # ? DI Configuration, Middleware Pipeline
?
??? Order.Application/                  # ?? Application Layer (Business Logic)
?   ??? DTOs/                           # ? Data Transfer Objects
?   ?   ??? OrderDto.cs
?   ?   ??? CreateOrderDto.cs
?   ?   ??? UpdateOrderDto.cs
?   ?   ??? OrderDetailDto.cs
?   ?   ??? ...
?   ??? Services/                       # ? Business Logic Services
?   ?   ??? IOrderService.cs
?   ?   ??? OrderService.cs
?   ?   ??? ...
?   ??? Mappers/                        # ? Entity ? DTO Mapping
?   ?   ??? OrderMapper.cs
?   ?   ??? OrderDetailMapper.cs
?   ??? Validators/                     # ? Business Rules Validation
?   ?   ??? CreateOrderDtoValidator.cs
?   ?   ??? OrderStatusValidator.cs
?   ??? Common/
?       ??? Result.cs                   # ? Result Pattern
?
??? Order.Domain/                       # ?? Domain Layer (Core Business)
?   ??? Entities/                       # ? Domain Entities (Aggregate Roots)
?   ?   ??? OrderEntity.cs             # ? Encapsulation, Domain Logic, Factory Methods
?   ?   ??? OrderDetailEntity.cs
?   ??? ValueObjects/                   # ? Immutable Value Objects
?   ?   ??? OrderNumber.cs
?   ?   ??? Money.cs
?   ??? Constants/
?       ??? OrderStatus.cs              # ? Domain Constants
?
??? Order.Infrastructure/               # ??? Infrastructure Layer (Data Access)
    ??? Data/
    ?   ??? OrderDbContext.cs           # ? EF Core DbContext
    ??? Repositories/                   # ? Repository Pattern
    ?   ??? IRepository.cs              # ? Generic base
    ?   ??? Repository.cs
    ?   ??? IOrderRepository.cs
    ?   ??? OrderRepository.cs
    ?   ??? IUnitOfWork.cs              # ? Unit of Work Pattern
    ?   ??? UnitOfWork.cs
    ??? Migrations/
    ?   ??? MigrationHelper.cs          # ? Auto-apply migrations
    ??? Seeds/
        ??? DatabaseSeeder.cs           # ? Seed data using Factory Methods
```

---

## ?? H??NG D?N S? D?NG

### 1?? **Clone Repository**
```bash
git clone https://github.com/ThanhBinhhhhhh/HuongDichVu
cd HuongDichVu/OrderMicroservice
```

### 2?? **Restore Dependencies**
```bash
dotnet restore
```

### 3?? **Configure Database**
C?p nh?t connection string trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OrderMicroserviceDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4?? **Run Migrations**
```bash
dotnet ef database update --project Order.Infrastructure --startup-project Order.API
```

### 5?? **Build Project**
```bash
dotnet build
```

### 6?? **Run Application**
```bash
dotnet run --project Order.API
```

### 7?? **Access Swagger UI**
```
https://localhost:5001/
```

---

## ?? API ENDPOINTS

### ?? **Orders**

| Method | Endpoint | Mô t? |
|--------|----------|-------|
| `GET` | `/api/orders` | L?y t?t c? ??n hàng |
| `GET` | `/api/orders/{id}` | L?y ??n hàng theo ID |
| `GET` | `/api/orders/status/{status}` | L?y ??n hàng theo tr?ng thái |
| `GET` | `/api/orders/table/{tableId}` | L?y ??n hàng theo bàn |
| `POST` | `/api/orders` | T?o ??n hàng m?i |
| `PUT` | `/api/orders/{id}` | C?p nh?t ??n hàng |
| `PUT` | `/api/orders/{id}/status` | C?p nh?t tr?ng thái |
| `DELETE` | `/api/orders/{id}` | Xóa ??n hàng |

### ??? **Order Details**

| Method | Endpoint | Mô t? |
|--------|----------|-------|
| `GET` | `/api/orders/{orderId}/details` | L?y chi ti?t ??n hàng |
| `POST` | `/api/orders/{orderId}/details` | Thêm món vào ??n |
| `PUT` | `/api/orders/{orderId}/details/{detailId}` | C?p nh?t s? l??ng |
| `DELETE` | `/api/orders/{orderId}/details/{detailId}` | Xóa món kh?i ??n |

---

## ?? K?T QU? ??T ???C

### ? **Maintainability** (D? b?o trì)
- Code rõ ràng, d? ??c
- Responsibilities phân tách rõ ràng
- D? tìm và fix bugs

### ? **Testability** (D? test)
- Dependency Injection cho m?i dependency
- Business logic trong Domain Entities
- Repositories có th? mock d? dàng

### ? **Scalability** (D? m? r?ng)
- Thêm features không ?nh h??ng code c?
- Plugin pattern v?i DI
- Microservice-ready architecture

### ? **Readability** (D? ??c)
- Meaningful names
- Clear structure
- Good documentation

---

## ?? TÀI LI?U THAM KH?O

1. **Clean Architecture** - Robert C. Martin
2. **Domain-Driven Design** - Eric Evans
3. **SOLID Principles** - Robert C. Martin
4. **Design Patterns** - Gang of Four
5. **Clean Code** - Robert C. Martin
6. **Microsoft .NET Documentation**

---

**Version:** 1.0  
**Last Updated:** 2024  
**Status:** ? Production Ready
