using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace SeleniumTests
{
    [TestFixture]
    public class AdminLoginLogoutTests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        private const string BaseUrl = "http://localhost:5192";
        private const string LoginUrl = BaseUrl + "/Dangnhap/Login";
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            // Bỏ comment dòng dưới nếu muốn chạy không hiện trình duyệt (headless)
            // options.AddArgument("--headless=new");
            options.AddArgument("--start-maximized");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            // Selenium Manager tự động tải ChromeDriver khớp với Chrome hiện tại
            // Không truyền service path để Selenium Manager tự xử lý
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        [TearDown]
        public void TearDown()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

        // ==================== HELPER METHODS ====================

        private void NavigateToLogin()
        {
            _driver.Navigate().GoToUrl(LoginUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Name("username")));
        }

        private void FillLoginForm(string username, string password)
        {
            var usernameInput = _driver.FindElement(By.Name("username"));
            usernameInput.Clear();
            usernameInput.SendKeys(username);

            var passwordInput = _driver.FindElement(By.Name("password"));
            passwordInput.Clear();
            passwordInput.SendKeys(password);
        }

        private void ClickLoginButton()
        {
            var loginBtn = _driver.FindElement(By.CssSelector("button[type='submit'].btn-primary"));
            loginBtn.Click();
        }

        // ==================== TEST 1: ĐĂNG NHẬP THÀNH CÔNG ====================

        [Test]
        [Order(1)]
        public void Test01_DangNhapThanhCong()
        {
            // Arrange
            NavigateToLogin();

            // Act
            FillLoginForm(AdminUsername, AdminPassword);
            ClickLoginButton();

            // Assert - Sau khi đăng nhập thành công, chuyển hướng đến trang DonHang
            _wait.Until(ExpectedConditions.UrlContains("/DonHang"));

            Assert.That(_driver.Url, Does.Contain("/DonHang"),
                "Sau khi đăng nhập thành công phải chuyển hướng đến trang Đơn Hàng.");
            Console.WriteLine($"[PASS] Đăng nhập thành công. URL hiện tại: {_driver.Url}");
        }

        // ==================== TEST 2: ĐĂNG NHẬP BỎ TRỐNG USERNAME VÀ MẬT KHẨU ====================

        [Test]
        [Order(2)]
        public void Test02_DangNhapBoTrong_UsernameVaMatKhau()
        {
            // Arrange
            NavigateToLogin();

            // Act - Không nhập gì, bấm đăng nhập
            ClickLoginButton();

            // Chờ trang phản hồi
            System.Threading.Thread.Sleep(2000);

            // Assert - Không được vào trang DonHang (bất kể redirect đi đâu cũng phải sai)
            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Khi bỏ trống username và password không được đăng nhập vào trang quản trị.");

            Console.WriteLine($"[PASS] Bỏ trống username & password: Không vào được DonHang. URL: {_driver.Url}");
        }

        // ==================== TEST 3: ĐĂNG NHẬP CHỈ BỎ TRỐNG USERNAME ====================

        [Test]
        [Order(3)]
        public void Test03_DangNhapBoTrong_ChiUsername()
        {
            // Arrange
            NavigateToLogin();

            // Act - Chỉ nhập password, không nhập username
            FillLoginForm("", AdminPassword);
            ClickLoginButton();

            System.Threading.Thread.Sleep(2000);

            // Assert
            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Khi bỏ trống username không được chuyển hướng đến trang Đơn Hàng.");
            Console.WriteLine($"[PASS] Bỏ trống username: Không vào được DonHang. URL: {_driver.Url}");
        }

        // ==================== TEST 4: ĐĂNG NHẬP CHỈ BỎ TRỐNG MẬT KHẨU ====================

        [Test]
        [Order(4)]
        public void Test04_DangNhapBoTrang_ChiMatKhau()
        {
            // Arrange
            NavigateToLogin();

            // Act - Chỉ nhập username, không nhập password
            FillLoginForm(AdminUsername, "");
            ClickLoginButton();

            System.Threading.Thread.Sleep(2000);

            // Assert
            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Khi bỏ trống password không được chuyển hướng đến trang Đơn Hàng.");
            Console.WriteLine($"[PASS] Bỏ trống password: Không vào được DonHang. URL: {_driver.Url}");
        }

        // ==================== TEST 5: ĐĂNG NHẬP SAI MẬT KHẨU ====================

        [Test]
        [Order(5)]
        public void Test05_DangNhapSaiMatKhau()
        {
            // Arrange
            NavigateToLogin();

            // Act
            FillLoginForm(AdminUsername, "matkhau_sai_123");
            ClickLoginButton();

            // Chờ trang phản hồi API (tối đa 15 giây)
            var longWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            longWait.Until(d => !d.Url.Contains("DonHang") || d.Url.Contains("Dangnhap"));

            System.Threading.Thread.Sleep(1000);

            // Assert - Không được vào trang DonHang
            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Khi sai mật khẩu không được chuyển hướng đến trang Đơn Hàng.");

            // Kiểm tra có thông báo lỗi
            try
            {
                var errorMessage = _driver.FindElement(
                    By.CssSelector(".validation-summary-errors li, .text-danger"));
                if (!string.IsNullOrWhiteSpace(errorMessage.Text))
                    Console.WriteLine($"[PASS] Sai mật khẩu. Thông báo lỗi: '{errorMessage.Text}'");
                else
                    Console.WriteLine("[PASS] Sai mật khẩu: Không vào được DonHang.");
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine($"[PASS] Sai mật khẩu: Không vào được DonHang. URL: {_driver.Url}");
            }
        }

        // ==================== TEST 6: ĐĂNG NHẬP SAI USERNAME ====================

        [Test]
        [Order(6)]
        public void Test06_DangNhapSaiUsername()
        {
            // Arrange
            NavigateToLogin();

            // Act
            FillLoginForm("username_khong_ton_tai", AdminPassword);
            ClickLoginButton();

            // Chờ trang phản hồi API (tối đa 15 giây)
            System.Threading.Thread.Sleep(3000);

            // Assert
            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Khi sai username không được chuyển hướng đến trang Đơn Hàng.");
            Console.WriteLine($"[PASS] Sai username: Không vào được DonHang. URL: {_driver.Url}");
        }

        // ==================== TEST 7: HIỂN THỊ / ẨN MẬT KHẨU ====================

        [Test]
        [Order(7)]
        public void Test07_HienThiMatKhau_Toggle()
        {
            // Arrange
            NavigateToLogin();

            var passwordInput = _driver.FindElement(By.Name("password"));
            var toggleBtn = _driver.FindElement(By.CssSelector(".toggle-password-btn"));
            var visibilityIcon = _driver.FindElement(By.CssSelector(".icon-visibility"));

            // Kiểm tra trạng thái ban đầu: type="password", icon = "visibility"
            Assert.That(passwordInput.GetDomProperty("type"), Is.EqualTo("password"),
                "Ban đầu input mật khẩu phải có type='password'.");
            Assert.That(visibilityIcon.Text, Is.EqualTo("visibility"),
                "Ban đầu icon phải là 'visibility'.");

            // Act - Nhập mật khẩu rồi bấm toggle để hiện
            passwordInput.SendKeys(AdminPassword);
            toggleBtn.Click();
            _wait.Until(d => d.FindElement(By.Name("password")).GetDomProperty("type") == "text");

            // Assert - Sau khi click: type="text", icon = "visibility_off"
            Assert.That(passwordInput.GetDomProperty("type"), Is.EqualTo("text"),
                "Sau khi bấm toggle, input phải chuyển sang type='text' (hiện mật khẩu).");
            Assert.That(visibilityIcon.Text, Is.EqualTo("visibility_off"),
                "Sau khi bấm toggle, icon phải đổi thành 'visibility_off'.");

            Console.WriteLine("[PASS] Bấm toggle lần 1: Mật khẩu được hiển thị, icon = 'visibility_off'.");

            // Act - Bấm toggle lần 2 để ẩn lại
            toggleBtn.Click();
            _wait.Until(d => d.FindElement(By.Name("password")).GetDomProperty("type") == "password");

            // Assert - Sau khi click lần 2: type="password", icon = "visibility"
            Assert.That(passwordInput.GetDomProperty("type"), Is.EqualTo("password"),
                "Sau khi bấm toggle lần 2, input phải trở lại type='password' (ẩn mật khẩu).");
            Assert.That(visibilityIcon.Text, Is.EqualTo("visibility"),
                "Sau khi bấm toggle lần 2, icon phải trở lại 'visibility'.");

            Console.WriteLine("[PASS] Bấm toggle lần 2: Mật khẩu được ẩn lại, icon = 'visibility'.");
        }

        // ==================== TEST 8: ĐĂNG XUẤT THÀNH CÔNG ====================

        [Test]
        [Order(8)]
        public void Test08_DangXuatThanhCong()
        {
            // Arrange - Đăng nhập trước
            NavigateToLogin();
            FillLoginForm(AdminUsername, AdminPassword);
            ClickLoginButton();

            _wait.Until(ExpectedConditions.UrlContains("/DonHang"));
            Assert.That(_driver.Url, Does.Contain("/DonHang"),
                "Phải đăng nhập thành công trước khi test đăng xuất.");
            Console.WriteLine($"[INFO] Đã đăng nhập, URL: {_driver.Url}");

            // Act - Click nút Đăng Xuất
            var logoutBtn = _wait.Until(
                ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("form[action*='Logout'] button[type='submit']")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", logoutBtn);
            System.Threading.Thread.Sleep(300);
            logoutBtn.Click();

            // Chờ thoát khỏi trang DonHang (tối đa 10 giây)
            _wait.Until(d => !d.Url.Contains("/DonHang"));

            // Assert 1 - Không còn ở trang DonHang sau khi đăng xuất
            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Sau khi đăng xuất không được ở trang Đơn Hàng.");
            Console.WriteLine($"[INFO] Sau đăng xuất URL: {_driver.Url}");

            // Assert 2 - Thử vào lại DonHang, phải bị chặn (không vào được hoặc redirect)
            _driver.Navigate().GoToUrl(BaseUrl + "/DonHang");
            System.Threading.Thread.Sleep(2000);

            // Kiểm tra: hoặc bị redirect về trang login, hoặc trang hiện form đăng nhập (username input)
            bool redirectedToLogin = _driver.Url.Contains("Dangnhap") || _driver.Url.Contains("Login");
            bool showsLoginForm = false;
            try
            {
                showsLoginForm = _driver.FindElement(By.Name("username")).Displayed;
            }
            catch (NoSuchElementException) { }

            Assert.That(redirectedToLogin || showsLoginForm, Is.True,
                $"Sau khi đăng xuất, truy cập DonHang phải bị chặn (redirect về login hoặc hiện form login). URL thực tế: {_driver.Url}");

            Console.WriteLine($"[PASS] Đăng xuất thành công. Truy cập lại bị chặn tại: {_driver.Url}");
        }
    }
}