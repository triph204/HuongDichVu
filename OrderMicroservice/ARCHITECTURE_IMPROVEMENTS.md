# ?? CLEAN ARCHITECTURE & SOLID PRINCIPLES - T?NG K?T C?I TI?N

## ?? T?NG QUAN
D? án **OrderMicroservice** ?ã ???c c?i thi?n toàn di?n theo các nguyên t?c:
- ? **Clean Architecture** (Ki?n trúc s?ch)
- ? **SOLID Principles** (5 nguyên t?c thi?t k? h??ng ??i t??ng)
- ? **Design Patterns** (Các m?u thi?t k?)
- ? **Clean Code** (Code s?ch, d? ??c, d? b?o trì)

---

## ?? C?U TRÚC D? ÁN (CLEAN ARCHITECTURE)

```
OrderMicroservice/
?
??? Order.API/                          # Presentation Layer
?   ??? Controllers/                    # API Controllers (RESTful endpoints)
?   ??? Middleware/                     # Custom middlewares
?   ??? Hubs/                          # SignalR Hubs
?
??? Order.Application/                  # Application Layer
?   ??? DTOs/                          # Data Transfer Objects
?   ??? Services/                      # Application Services (Business Logic)
?   ??? Mappers/                       # Entity <-> DTO Mapping
?   ??? Validators/                    # Business Rules Validation
?   ??? Common/                        # Shared application logic (Result Pattern)
?
??? Order.Domain/                       # Domain Layer (Core Business)
?   ??? Entities/                      # Domain Entities (Aggregate Roots)
?   ??? ValueObjects/                  # Value Objects (Immutable)
?   ??? Constants/                     # Domain Constants
?
??? Order.Infrastructure/               # Infrastructure Layer
    ??? Data/                          # DbContext, EF Core configuration
    ??? Repositories/                  # Repository Pattern implementation
    ??? Migrations/                    # Database migrations
    ??? Seeds/                         # Database seeding
```

---

## ?? CÁC C?I TI?N CHI TI?T

### 1?? **DOMAIN LAYER** - Core Business Logic

#### ? **Domain Entities v?i Encapsulation**
- **File**: `OrderEntity.cs`, `OrderDetailEntity.cs`
- **C?i ti?n**:
  - ?? Private setters ?? b?o v? state
  - ?? Static Factory Methods thay vì public constructors
  - ?? Domain Logic trong Entity (AddOrderDetail, UpdateStatus, RemoveOrderDetail)
  - ?? Business Rules validation (CanModifyOrder, IsValidStatusTransition)
  - ?? Read-only collections cho navigation properties

**Ví d?**:
```csharp
// ? TR??C - Entity thi?u encapsulation
public decimal TotalAmount { get; set; }

// ? SAU - Protected v?i domain logic
public decimal TotalAmount { get; private set; }

public void AddOrderDetail(...)
{
    if (!CanModifyOrder())
        throw new InvalidOperationException("Không th? thêm món khi ??n hàng ? tr?ng thái này");
    
    // Business logic...
    RecalculateTotalAmount();
}
```

#### ? **Value Objects**
- **Files**: `OrderNumber.cs`, `Money.cs`
- **Nguyên t?c**:
  - ?? Immutability (không th? thay ??i sau khi t?o)
  - ?? Value equality (so sánh theo giá tr?, không theo reference)
  - ?? Validation trong constructor
  - ?? Self-validation

**Ví d?**:
```csharp
// Value Object - OrderNumber
public sealed class OrderNumber : IEquatable<OrderNumber>
{
    public string Value { get; }
    
    private OrderNumber(string value) => Value = value;
    
    public static OrderNumber Create() => new OrderNumber($"ORD-{DateTime.Now:yyMMddHHmmss}");
}
```

#### ? **Domain Constants**
- **File**: `OrderStatus.cs`
- **L?i ích**: Tránh magic strings, d? maintain

---

### 2?? **APPLICATION LAYER** - Business Logic

#### ? **DTOs (Data Transfer Objects)**
- **Files**: `OrderDto.cs`, `CreateOrderDto.cs`, `UpdateOrderDto.cs`, etc.
- **Nguyên t?c**:
  - ?? Tách bi?t Domain Entities và API contracts
  - ?? Data Annotations cho validation c? b?n
  - ?? Immutability khi có th?

#### ? **Mappers - Single Responsibility**
- **Files**: `OrderMapper.cs`, `OrderDetailMapper.cs`
- **Design Pattern**: Static Mapper Pattern
- **L?i ích**:
  - ?? Tách logic mapping kh?i Services
  - ?? D? test và maintain
  - ?? Single Responsibility Principle

**Ví d?**:
```csharp
// ? TR??C - Mapping trong Service
private OrderDto MapToDto(OrderEntity order)
{
    return new OrderDto { ... };
}

// ? SAU - Mapping tách riêng
public static class OrderMapper
{
    public static OrderDto ToDto(OrderEntity entity) { ... }
    public static OrderEntity ToEntity(CreateOrderDto dto) { ... }
}
```

#### ? **Validators - Separation of Concerns**
- **Files**: `CreateOrderDtoValidator.cs`, `OrderStatusValidator.cs`
- **L?i ích**:
  - ?? Business rules validation tách bi?t
  - ?? D? test và reuse
  - ?? Clear error messages

#### ? **Result Pattern**
- **File**: `Result.cs`
- **Design Pattern**: Result Pattern (thay vì exception cho business logic failures)
- **L?i ích**:
  - ?? Explicit error handling
  - ?? Tránh try-catch cho business logic
  - ?? Type-safe error handling

#### ? **Services - Dependency Injection**
- **Files**: `OrderService.cs`, `OrderDetailService.cs`
- **C?i ti?n**:
  - ?? Constructor injection v?i null checks
  - ?? S? d?ng Mappers thay vì mapping inline
  - ?? Delegate domain logic cho Entities
  - ?? Clean error handling

**Ví d?**:
```csharp
// ? S? d?ng Domain Logic và Mapper
public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
{
    var order = OrderMapper.ToEntity(dto); // S? d?ng Mapper
    var created = await _orderRepository.CreateAsync(order);
    return OrderMapper.ToDto(created);
}
```

---

### 3?? **INFRASTRUCTURE LAYER** - Data Access

#### ? **Repository Pattern**
- **Files**: `IRepository.cs`, `Repository.cs` (Base)
- **Design Pattern**: Generic Repository Pattern
- **L?i ích**:
  - ?? Abstraction over data access
  - ?? DRY - Avoid code duplication
  - ?? Easy to mock for testing

**Ví d?**:
```csharp
// Base Repository
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly OrderDbContext Context;
    protected readonly DbSet<TEntity> DbSet;
    
    // Common CRUD operations
}

// Specific Repository
public class OrderRepository : Repository<OrderEntity>, IOrderRepository
{
    // Ch? implement các query ??c bi?t
    public async Task<IEnumerable<OrderEntity>> GetByStatusAsync(string status) { ... }
}
```

#### ? **Unit of Work Pattern**
- **Files**: `IUnitOfWork.cs`, `UnitOfWork.cs`
- **Design Pattern**: Unit of Work Pattern
- **L?i ích**:
  - ?? Transaction management
  - ?? Coordinating multiple repositories
  - ?? Ensuring data consistency

---

### 4?? **API LAYER** - Presentation

#### ? **Controllers - RESTful Best Practices**
- **File**: `OrdersController.cs`
- **C?i ti?n**:
  - ?? Dependency Injection v?i null checks
  - ?? Logging cho m?i action
  - ?? Input validation v?i Validators
  - ?? Proper HTTP status codes
  - ?? API Documentation v?i ProducesResponseType
  - ?? Structured error responses

**Ví d?**:
```csharp
[HttpPost]
[ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
{
    _logger.LogInformation("Creating new order for table: {TableName}", dto.TableName);
    
    // Validation
    var (isValid, errors) = CreateOrderDtoValidator.Validate(dto);
    if (!isValid)
        return BadRequest(new { message = "D? li?u không h?p l?", errors });
    
    // Business logic
    var order = await _orderService.CreateOrderAsync(dto);
    
    return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
}
```

#### ? **Dependency Injection Configuration**
- **File**: `Program.cs`
- **C?i ti?n**:
  - ?? Organized DI registration v?i comments
  - ?? Scoped lifetime cho Repositories và Services
  - ?? Swagger documentation improvements
  - ?? CORS configuration
  - ?? Middleware pipeline organization

---

## ?? SOLID PRINCIPLES ÁP D?NG

### **S - Single Responsibility Principle**
? M?i class ch? có 1 lý do ?? thay ??i:
- `OrderMapper` - Ch? ??m nhi?m mapping
- `CreateOrderDtoValidator` - Ch? ??m nhi?m validation
- `OrderService` - Ch? ??m nhi?m business logic
- `OrderRepository` - Ch? ??m nhi?m data access

### **O - Open/Closed Principle**
? M? cho m? r?ng, ?óng cho s?a ??i:
- Base `Repository<T>` có th? extend mà không modify
- `OrderRepository` extends `Repository<OrderEntity>`
- Value Objects có th? extend functionality

### **L - Liskov Substitution Principle**
? Derived classes có th? thay th? base classes:
- `OrderRepository` có th? thay th? `Repository<OrderEntity>`
- `IOrderRepository` có th? thay th? `IRepository<OrderEntity>`

### **I - Interface Segregation Principle**
? Clients không b? ép ph? thu?c vào interfaces không dùng:
- `IOrderRepository` extends `IRepository<OrderEntity>` v?i custom methods
- `IOrderDetailRepository` tách bi?t v?i `IOrderRepository`

### **D - Dependency Inversion Principle**
? Ph? thu?c vào abstractions, không ph? thu?c vào concrete implementations:
- Services ph? thu?c vào `IOrderRepository` (interface)
- Controllers ph? thu?c vào `IOrderService` (interface)
- Dependency Injection container qu?n lý dependencies

---

## ?? DESIGN PATTERNS S? D?NG

### 1. **Repository Pattern**
- `IRepository<T>`, `Repository<T>`
- Abstraction over data access layer

### 2. **Unit of Work Pattern**
- `IUnitOfWork`, `UnitOfWork`
- Transaction management

### 3. **Factory Pattern**
- `OrderEntity.Create(...)` - Static Factory Method
- `OrderDetailEntity.Create(...)` - Static Factory Method

### 4. **Result Pattern**
- `Result`, `Result<T>`
- Explicit error handling

### 5. **Mapper Pattern**
- `OrderMapper`, `OrderDetailMapper`
- Object-to-object mapping

### 6. **Dependency Injection Pattern**
- Constructor injection throughout all layers

### 7. **Aggregate Root Pattern (DDD)**
- `OrderEntity` là Aggregate Root
- `OrderDetailEntity` là child entity

---

## ?? CLEAN CODE PRINCIPLES

### ? **Meaningful Names**
```csharp
// ? BAD
var o = new Order();
var d = GetData();

// ? GOOD
var order = OrderEntity.Create(...);
var orderDetails = await _orderRepository.GetByIdAsync(id);
```

### ? **Small Functions**
- M?i method làm 1 vi?c
- Average 10-20 lines per method
- Clear responsibility

### ? **Comments Where Needed**
```csharp
/// <summary>
/// Order Service - Application Layer
/// Áp d?ng SOLID principles và Clean Architecture
/// </summary>
```

### ? **Error Handling**
- Try-catch ? boundaries (Controllers)
- Domain exceptions cho business rules
- Structured error responses

### ? **DRY - Don't Repeat Yourself**
- Base Repository ?? tránh duplicate code
- Mappers ?? reuse mapping logic
- Validators ?? reuse validation logic

---

## ?? K?T QU? ??T ???C

### ? **Maintainability** (D? b?o trì)
- Code rõ ràng, d? ??c
- Responsibilities phân tách rõ ràng
- D? tìm và fix bugs

### ? **Testability** (D? test)
- Dependency Injection cho m?i dependency
- Business logic trong Domain Entities (d? unit test)
- Repositories có th? mock d? dàng

### ? **Scalability** (D? m? r?ng)
- Thêm features m?i không ?nh h??ng code c?
- Plugin pattern v?i DI container
- Microservice-ready architecture

### ? **Readability** (D? ??c)
- Meaningful names
- Clear structure
- Good documentation

---

## ?? H??NG D?N S? D?NG

### Build Project
```bash
dotnet build
```

### Run Migrations
```bash
dotnet ef database update --project Order.Infrastructure --startup-project Order.API
```

### Run Application
```bash
dotnet run --project Order.API
```

### Access Swagger UI
```
https://localhost:<port>/
```

---

## ?? TÀI LI?U THAM KH?O

1. **Clean Architecture** - Robert C. Martin
2. **Domain-Driven Design** - Eric Evans
3. **SOLID Principles** - Robert C. Martin
4. **Design Patterns** - Gang of Four
5. **Clean Code** - Robert C. Martin

---

**Tác gi?**: Development Team  
**Ngày c?p nh?t**: 2024  
**Version**: 1.0
