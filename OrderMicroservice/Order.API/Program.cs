using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Order.API.Middleware;
using Order.Infrastructure.Data;
using Order.Infrastructure.Repositories;
using Order.Application.Services;
using Order.Application.Security;
using Order.API.Hubs;
using Order.Infrastructure.Migrations;
using Order.Infrastructure.Seeds;

var builder = WebApplication.CreateBuilder(args);

// ========== 1. DATABASE CONFIGURATION ==========
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========== 2. DEPENDENCY INJECTION - REPOSITORIES ==========
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ========== 3. DEPENDENCY INJECTION - SERVICES ==========
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();

// ========== 4. DEPENDENCY INJECTION - SECURITY ==========
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();

// ========== 5. MEMORY CACHE (for Rate Limiting) ==========
builder.Services.AddMemoryCache();

// ========== 6. CORS CONFIGURATION ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    // Stricter CORS policy for production
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "https://yourdomain.com",
                "https://admin.yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ========== 7. SIGNALR CONFIGURATION ==========
builder.Services.AddSignalR();

// ========== 8. CONTROLLERS ==========
builder.Services.AddControllers(options =>
{
    // Validate model state automatically
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = false;
});

// ========== 9. SWAGGER CONFIGURATION ==========
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order Microservice API",
        Version = "v1",
        Description = "Microservice qu?n lý ??n hàng nhà hàng - Clean Architecture + Security"
    });
    
    // Thêm Security Definition cho JWT (n?u có)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ========== MIDDLEWARE PIPELINE (Order matters!) ==========

// 1. Security Headers - Thêm ??u tiên
app.UseSecurityHeaders();

// 2. Rate Limiting - Phòng ch?ng DDoS
app.UseRateLimiting();

// 3. Request Validation - L?c request ??c h?i
app.UseRequestValidation();

// 4. Global Exception Handler
app.UseMiddleware<GlobalExceptionMiddleware>();

// 5. HTTPS Redirection
app.UseHttpsRedirection();

// 6. CORS
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("Production");
}

// 7. Swagger (ch? Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Microservice API V1");
        c.RoutePrefix = string.Empty;
    });
}

// 8. Routing & Authorization
app.UseRouting();
app.UseAuthorization();

// 9. Map Controllers & Hubs
app.MapControllers();
app.MapHub<OrderHub>("/orderHub");

// ========== DATABASE MIGRATION & SEEDING ==========
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    
    MigrationHelper.ApplyMigrations(dbContext);
    
    if (app.Environment.IsDevelopment())
    {
        DatabaseSeeder.Seed(dbContext);
    }
}

app.Run();
