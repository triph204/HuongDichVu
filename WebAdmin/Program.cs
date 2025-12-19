using WebAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Add services to the container
// ============================================

// 1. Http Client cho API calls
builder.Services.AddHttpClient<IRestaurantApiClient, RestaurantApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHttpContextAccessor();


// 2. HttpClient thường (dùng cho Auth)
builder.Services.AddHttpClient();

// 3. Controllers with Views
builder.Services.AddControllersWithViews(options =>
{
    // ✅ Áp dụng AuthorizeToken cho TẤT CẢ controllers
    options.Filters.Add(new WebAdmin.Filters.AuthorizeTokenAttribute());
});

// 4. Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Không bắt HTTPS khi localhost
});

// 5. CORS (nếu cần)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5137",  // Server API URL
                "http://127.0.0.1:5137"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // ⚠️ Quan trọng cho SignalR
    });
});

// 6. Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// ============================================
// Build app
// ============================================
var app = builder.Build();

// ============================================
// Configure the HTTP request pipeline
// ============================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Phục vụ static files (CSS, JS, images)
app.UseStaticFiles();

app.UseRouting();

// ✅ THÊM CÁC MIDDLEWARE NÀY
app.UseSession();
app.UseCors("AllowAll");
app.UseAuthorization();

// ============================================
// Map Controllers
// ============================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dangnhap}/{action=Login}/{id?}");

app.Run();