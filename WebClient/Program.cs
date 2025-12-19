var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Cấu hình port
builder.WebHost.UseUrls("http://localhost:5002");

// THÊM CORS để client có thể gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// DÙNG CORS
app.UseCors("AllowAll");

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Không dùng HTTPS redirect để test dễ hơn
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Default route -> Menu/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Menu}/{action=Index}/{id?}");

app.Run();