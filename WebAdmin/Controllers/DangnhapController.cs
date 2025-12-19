using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebAdmin.Filters;

namespace WebAdmin.Controllers
{
    [AllowAnonymous]
    public class DangnhapController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public DangnhapController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // ==================== GET: TRANG ĐĂNG NHẬP ====================
        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(GetTokenFromCookie()))
                return RedirectToAction("Index", "DonHang");

            return View();
        }

        // ==================== POST: XỬ LÝ ĐĂNG NHẬP ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("", "Vui lòng nhập username và mật khẩu");
                    return View(model);
                }

                var apiUrl = _configuration["ApiSettings:BaseUrl"];
                var loginUrl = $"{apiUrl}/api/auth/login";

                var loginRequest = new
                {
                    username = model.Username,
                    password = model.Password
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(loginRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(loginUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                    {
                        var root = doc.RootElement;
                        var token = root.GetProperty("token").GetString();
                        var userRole = root.GetProperty("user").GetProperty("role").GetString();
                        var username = root.GetProperty("user").GetProperty("username").GetString();
                        var userId = root.GetProperty("user").GetProperty("id").GetInt32();

                        Response.Cookies.Append("token", token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = false,
                            SameSite = SameSiteMode.Lax,
                            Path = "/",
                            Expires = DateTimeOffset.UtcNow.AddDays(7)
                        });

                        HttpContext.Session.SetString("Username", username);
                        HttpContext.Session.SetString("Role", userRole);
                        HttpContext.Session.SetInt32("UserId", userId);

                        return RedirectToAction("Index", "DonHang");
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    try
                    {
                        using (JsonDocument doc = JsonDocument.Parse(errorResponse))
                        {
                            var root = doc.RootElement;
                            var message = root.GetProperty("message").GetString();
                            ModelState.AddModelError("", message ?? "Đăng nhập thất bại");
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Đăng nhập thất bại");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
            }

            return View(model);
        }

        // ==================== GET: TRANG ĐĂNG KÝ ====================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ==================== POST: XỬ LÝ ĐĂNG KÝ ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin");
                    return View(model);
                }

                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Mật khẩu không khớp");
                    return View(model);
                }

                if (model.Password.Length < 6)
                {
                    ModelState.AddModelError("", "Mật khẩu phải ít nhất 6 ký tự");
                    return View(model);
                }

                var apiUrl = _configuration["ApiSettings:BaseUrl"];
                var registerUrl = $"{apiUrl}/api/auth/register";

                var registerRequest = new
                {
                    username = model.Username,
                    password = model.Password
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(registerRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(registerUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    try
                    {
                        using (JsonDocument doc = JsonDocument.Parse(errorResponse))
                        {
                            var root = doc.RootElement;
                            var message = root.GetProperty("message").GetString();
                            ModelState.AddModelError("", message ?? "Đăng ký thất bại");
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Đăng ký thất bại");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
            }

            return View(model);
        }

        // ==================== POST: ĐĂNG XUẤT ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Xóa Cookie token
            Response.Cookies.Delete("token");

            // Xóa Session
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Dangnhap");
        }

        // ==================== HELPER: LẤY TOKEN TỪ COOKIE ====================
        private string? GetTokenFromCookie()  // ✅ THÊM METHOD NÀY
        {
            Request.Cookies.TryGetValue("token", out var token);
            return token;
        }
    }

    // ==================== VIEW MODELS ====================
    public class LoginViewModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}