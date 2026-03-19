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

        private const string BaseUrl       = "http://localhost:5192";
        private const string LoginUrl      = BaseUrl + "/Dangnhap/Login";
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";
        private const int    Delay         = 1000;

        // ==================== SETUP / TEARDOWN ====================

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            _driver = new ChromeDriver(options);
            _wait   = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        [TearDown]
        public void TearDown()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

        // ==================== HELPER METHODS ====================

        private void Pause() => System.Threading.Thread.Sleep(Delay);

        private void NavigateToLogin()
        {
            _driver.Navigate().GoToUrl(LoginUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Name("username")));
            Pause();
        }

        private void FillLoginForm(string username, string password)
        {
            var usernameInput = _driver.FindElement(By.Name("username"));
            usernameInput.Clear();
            usernameInput.SendKeys(username);
            Pause();

            var passwordInput = _driver.FindElement(By.Name("password"));
            passwordInput.Clear();
            passwordInput.SendKeys(password);
            Pause();
        }

        private void ClickLoginButton()
        {
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-primary")).Click();
            Pause();
        }

        // ==================== TEST 1: DANG NHAP THANH CONG ====================

        [Test]
        [Order(1)]
        public void Test01_DangNhapThanhCong()
        {
            NavigateToLogin();
            FillLoginForm(AdminUsername, AdminPassword);
            ClickLoginButton();

            _wait.Until(ExpectedConditions.UrlContains("/DonHang"));

            Assert.That(_driver.Url, Does.Contain("/DonHang"),
                "Sau khi dang nhap thanh cong phai chuyen huong den trang Don Hang.");

            Console.WriteLine($"[PASS] Dang nhap thanh cong. URL: {_driver.Url}");
        }

        // ==================== TEST 2: BO TRONG USERNAME VA MAT KHAU ====================

        [Test]
        [Order(2)]
        public void Test02_DangNhapBoTrong_UsernameVaMatKhau()
        {
            NavigateToLogin();
            ClickLoginButton();
            System.Threading.Thread.Sleep(2000);

            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Khi bo trong username va password khong duoc dang nhap vao trang quan tri.");

            Console.WriteLine($"[PASS] Bo trong username & password: Khong vao duoc DonHang. URL: {_driver.Url}");
        }

        // ==================== TEST 3: BO TRONG USERNAME ====================

        [Test]
        [Order(3)]
        public void Test03_DangNhapBoTrong_ChiUsername()
        {
            NavigateToLogin();
            FillLoginForm("", AdminPassword);
            ClickLoginButton();
            System.Threading.Thread.Sleep(2000);

            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Khi bo trong username khong duoc chuyen huong den trang Don Hang.");

            Console.WriteLine($"[PASS] Bo trong username: Khong vao duoc DonHang. URL: {_driver.Url}");
        }

        // ==================== TEST 4: BO TRONG MAT KHAU ====================

        [Test]
        [Order(4)]
        public void Test04_DangNhapBoTrong_ChiMatKhau()
        {
            NavigateToLogin();
            FillLoginForm(AdminUsername, "");
            ClickLoginButton();
            System.Threading.Thread.Sleep(2000);

            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Khi bo trong password khong duoc chuyen huong den trang Don Hang.");

            Console.WriteLine($"[PASS] Bo trong password: Khong vao duoc DonHang. URL: {_driver.Url}");
        }

        // ==================== TEST 5: SAI MAT KHAU ====================

        [Test]
        [Order(5)]
        public void Test05_DangNhapSaiMatKhau()
        {
            NavigateToLogin();
            FillLoginForm(AdminUsername, "matkhau_sai_123");
            ClickLoginButton();

            var longWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            longWait.Until(d => !d.Url.Contains("DonHang") || d.Url.Contains("Dangnhap"));
            Pause();

            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Khi sai mat khau khong duoc chuyen huong den trang Don Hang.");

            try
            {
                var errorMessage = _driver.FindElement(
                    By.CssSelector(".validation-summary-errors li, .text-danger"));
                if (!string.IsNullOrWhiteSpace(errorMessage.Text))
                    Console.WriteLine($"[PASS] Sai mat khau. Thong bao loi: '{errorMessage.Text}'");
                else
                    Console.WriteLine("[PASS] Sai mat khau: Khong vao duoc DonHang.");
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine($"[PASS] Sai mat khau: Khong vao duoc DonHang. URL: {_driver.Url}");
            }
        }

        // ==================== TEST 6: SAI USERNAME ====================

        [Test]
        [Order(6)]
        public void Test06_DangNhapSaiUsername()
        {
            NavigateToLogin();
            FillLoginForm("username_khong_ton_tai", AdminPassword);
            ClickLoginButton();
            System.Threading.Thread.Sleep(3000);

            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Khi sai username khong duoc chuyen huong den trang Don Hang.");

            Console.WriteLine($"[PASS] Sai username: Khong vao duoc DonHang. URL: {_driver.Url}");
        }

        // ==================== TEST 7: HIEN THI / AN MAT KHAU ====================

        [Test]
        [Order(7)]
        public void Test07_HienThiMatKhau_Toggle()
        {
            NavigateToLogin();

            var passwordInput  = _driver.FindElement(By.Name("password"));
            var toggleBtn      = _driver.FindElement(By.CssSelector(".toggle-password-btn"));
            var visibilityIcon = _driver.FindElement(By.CssSelector(".icon-visibility"));

            Assert.That(passwordInput.GetDomProperty("type"), Is.EqualTo("password"),
                "Ban dau input mat khau phai co type='password'.");
            Assert.That(visibilityIcon.Text, Is.EqualTo("visibility"),
                "Ban dau icon phai la 'visibility'.");

            passwordInput.SendKeys(AdminPassword);
            toggleBtn.Click();
            _wait.Until(d => d.FindElement(By.Name("password")).GetDomProperty("type") == "text");

            Assert.That(passwordInput.GetDomProperty("type"), Is.EqualTo("text"),
                "Sau khi bam toggle, input phai chuyen sang type='text'.");
            Assert.That(visibilityIcon.Text, Is.EqualTo("visibility_off"),
                "Sau khi bam toggle, icon phai doi thanh 'visibility_off'.");

            Console.WriteLine("[PASS] Toggle lan 1: Mat khau duoc hien thi.");

            toggleBtn.Click();
            _wait.Until(d => d.FindElement(By.Name("password")).GetDomProperty("type") == "password");

            Assert.That(passwordInput.GetDomProperty("type"), Is.EqualTo("password"),
                "Sau khi bam toggle lan 2, input phai tro lai type='password'.");
            Assert.That(visibilityIcon.Text, Is.EqualTo("visibility"),
                "Sau khi bam toggle lan 2, icon phai tro lai 'visibility'.");

            Console.WriteLine("[PASS] Toggle lan 2: Mat khau duoc an lai.");
        }

        // ==================== TEST 8: DANG XUAT THANH CONG ====================

        [Test]
        [Order(8)]
        public void Test08_DangXuatThanhCong()
        {
            NavigateToLogin();
            FillLoginForm(AdminUsername, AdminPassword);
            ClickLoginButton();

            _wait.Until(ExpectedConditions.UrlContains("/DonHang"));
            Assert.That(_driver.Url, Does.Contain("/DonHang"),
                "Phai dang nhap thanh cong truoc khi test dang xuat.");

            var logoutBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("form[action*='Logout'] button[type='submit']")));
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].scrollIntoView(true);", logoutBtn);
            System.Threading.Thread.Sleep(300);
            logoutBtn.Click();

            _wait.Until(d => !d.Url.Contains("/DonHang"));

            Assert.That(_driver.Url, Does.Not.Contain("/DonHang"),
                "Sau khi dang xuat khong duoc o trang Don Hang.");

            _driver.Navigate().GoToUrl(BaseUrl + "/DonHang");
            System.Threading.Thread.Sleep(2000);

            bool redirectedToLogin = _driver.Url.Contains("Dangnhap") || _driver.Url.Contains("Login");
            bool showsLoginForm = false;
            try { showsLoginForm = _driver.FindElement(By.Name("username")).Displayed; }
            catch (NoSuchElementException) { }

            Assert.That(redirectedToLogin || showsLoginForm, Is.True,
                $"Sau khi dang xuat, truy cap DonHang phai bi chan. URL: {_driver.Url}");

            Console.WriteLine($"[PASS] Dang xuat thanh cong. Truy cap lai bi chan tai: {_driver.Url}");
        }
    }
}