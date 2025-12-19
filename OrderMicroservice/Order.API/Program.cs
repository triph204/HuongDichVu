using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Order.API.Middleware;
using Order.Infrastructure.Data;
using Order.Infrastructure.Repositories;
using Order.Application.Services;
using Order.API.Hubs;
using Order.Infrastructure.Migrations;
using Order.Infrastructure.Seeds;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Register Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();

// 3. Register Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();

// 4. Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// 5. Add SignalR
builder.Services.AddSignalR();

// 6. Add Controllers
builder.Services.AddControllers();

// 7. Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order Microservice API",
        Version = "v1",
        Description = "Microservice qu?n lý ??n hàng nhà hàng"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add exception middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/orderHub");

// Apply migrations
MigrationHelper.ApplyMigrations(app);

// Seed data (only in Development)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        DatabaseSeeder.Seed(dbContext);
    }
}

app.Run();
