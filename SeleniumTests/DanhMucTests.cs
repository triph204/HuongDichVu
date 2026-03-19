using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;

namespace SeleniumTests
{
    [TestFixture]
    public class DanhMucTests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        private const string BaseUrl          = "http://localhost:5192";
        private const string DanhMucUrl       = BaseUrl + "/DanhMuc";
        private const string LoginUrl         = BaseUrl + "/Dangnhap/Login";
        private const string AdminUsername    = "admin";
        private const string AdminPassword    = "admin123";
        private const string TenDanhMucMoi    = "TEST_DM_Selenium";
        private const string TenDanhMucSua    = "TEST_DM_Selenium_EDITED";
        private const string MoTaMoi          = "Mo ta tu dong tao boi Selenium";
        private const string MoTaSua          = "Mo ta da duoc chinh sua boi Selenium";
        private const int    Delay            = 1000;

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
            DangNhap();
        }

        [TearDown]
        public void TearDown()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

        // ==================== HELPER METHODS ====================

        private void Pause() => System.Threading.Thread.Sleep(Delay);

        private void DangNhap()
        {
            _driver.Navigate().GoToUrl(LoginUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Name("username")));
            Pause();
            _driver.FindElement(By.Name("username")).SendKeys(AdminUsername);
            Pause();
            _driver.FindElement(By.Name("password")).SendKeys(AdminPassword);
            Pause();
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang"));
            Pause();
        }

        private void NavigateToDanhMuc()
        {
            _driver.Navigate().GoToUrl(DanhMucUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
            Pause();
        }

        private IWebElement? TimDongTheoTen(string tenDanhMuc)
        {
            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            return rows.FirstOrDefault(r =>
            {
                var cells = r.FindElements(By.TagName("td"));
                return cells.Count > 0 && cells[0].Text.Trim() == tenDanhMuc;
            });
        }

        private void XoaNeuTonTai(string tenDanhMuc)
        {
            NavigateToDanhMuc();
            var row = TimDongTheoTen(tenDanhMuc);
            if (row != null)
            {
                var xoaBtn = row.FindElement(By.CssSelector("a.btn-danger"));
                ((IJavaScriptExecutor)_driver).ExecuteScript(
                    "arguments[0].removeAttribute('onclick');", xoaBtn);
                xoaBtn.Click();
                _wait.Until(d => !d.Url.Contains("/Delete/"));
                _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
                Pause();
            }
        }

        private void TaoDanhMuc(string ten, string moTa)
        {
            _driver.Navigate().GoToUrl(DanhMucUrl + "/Create");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='TenDanhMuc']")));
            Pause();
            var tenInput = _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']"));
            tenInput.Clear();
            tenInput.SendKeys(ten);
            Pause();
            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.Clear();
            moTaInput.SendKeys(moTa);
            Pause();
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();
            NavigateToDanhMuc();
        }

        // ==================== NHOM 1: THEM DANH MUC ====================

        [Test]
        [Order(1)]
        public void Test01_ThemDanhMuc_ThanhCong()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            XoaNeuTonTai(TenDanhMucSua);
            NavigateToDanhMuc();

            var themBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/DanhMuc/Create']")));
            Assert.That(themBtn.Displayed, Is.True, "Nut 'Them Danh Muc' phai hien thi.");
            themBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Create"));
            Pause();

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='TenDanhMuc']")));
            Assert.That(tenInput.Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("textarea[name='MoTa']")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Displayed, Is.True);

            tenInput.Clear();
            tenInput.SendKeys(TenDanhMucMoi);
            Pause();
            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.Clear();
            moTaInput.SendKeys(MoTaMoi);
            Pause();
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();

            NavigateToDanhMuc();
            var dongMoi = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongMoi, Is.Not.Null, $"Danh muc '{TenDanhMucMoi}' phai xuat hien sau khi them.");

            var cells = dongMoi!.FindElements(By.TagName("td"));
            Assert.That(cells[1].Text.Trim(), Is.EqualTo(MoTaMoi), "Mo ta phai khop.");

            Pause();
            Console.WriteLine($"[PASS] Them danh muc thanh cong: '{TenDanhMucMoi}'");
        }

        [Test]
        [Order(2)]
        public void Test02_ThemDanhMuc_BoTrongTen()
        {
            NavigateToDanhMuc();
            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/DanhMuc/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Create"));
            Pause();

            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).SendKeys("Mo ta khong co ten");
            Pause();
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            Pause();

            Assert.That(_driver.Url, Does.Contain("/Create"));

            var tenInput = _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']"));
            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;", tenInput);
            Assert.That(isInvalid, Is.True, "Input TenDanhMuc phai bi HTML5 validation khi bo trong.");

            Pause();
            Console.WriteLine("[PASS] Bo trong ten: Bi chan boi HTML5 required.");
        }

        // ==================== NHOM 2: THEM DANH MUC > HUY ====================

        [Test]
        [Order(3)]
        public void Test03_ThemDanhMuc_NutHuy()
        {
            NavigateToDanhMuc();
            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/DanhMuc/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Create"));
            Pause();

            _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']")).SendKeys("Ten se bi huy");
            Pause();
            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).SendKeys("Mo ta se bi huy");
            Pause();

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/DanhMuc']")));
            Assert.That(huyBtn.Displayed, Is.True);
            huyBtn.Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();

            Assert.That(_driver.Url, Does.Contain("/DanhMuc"));
            Assert.That(_driver.Url, Does.Not.Contain("/Create"));
            Assert.That(TimDongTheoTen("Ten se bi huy"), Is.Null);

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc));

            Pause();
            Console.WriteLine("[PASS] Nut Huy trang Them: Ve danh sach, khong luu du lieu.");
        }

        // ==================== NHOM 3: SUA DANH MUC ====================

        [Test]
        [Order(4)]
        public void Test04_SuaDanhMuc_ThanhCong()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            XoaNeuTonTai(TenDanhMucSua);
            TaoDanhMuc(TenDanhMucMoi, MoTaMoi);

            var dongTest = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongTest, Is.Not.Null);

            dongTest!.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Edit/"));
            Pause();

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='TenDanhMuc']")));
            Assert.That(tenInput.GetDomProperty("value"), Is.EqualTo(TenDanhMucMoi));

            Assert.That(_driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("a.btn-light[href='/DanhMuc']")).Displayed, Is.True);

            tenInput.Clear();
            tenInput.SendKeys(TenDanhMucSua);
            Pause();
            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.Clear();
            moTaInput.SendKeys(MoTaSua);
            Pause();
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Pause();

            NavigateToDanhMuc();
            var dongSua = TimDongTheoTen(TenDanhMucSua);
            Assert.That(dongSua, Is.Not.Null, $"Danh muc '{TenDanhMucSua}' phai xuat hien sau khi sua.");
            Assert.That(dongSua!.FindElements(By.TagName("td"))[1].Text.Trim(), Is.EqualTo(MoTaSua));
            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null);

            Pause();
            Console.WriteLine($"[PASS] Sua danh muc thanh cong: '{TenDanhMucMoi}' -> '{TenDanhMucSua}'");
        }

        [Test]
        [Order(5)]
        public void Test05_SuaDanhMuc_BoTrongTen()
        {
            NavigateToDanhMuc();
            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count, Is.GreaterThan(0));

            _wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.btn-primary.btn-sm"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Edit/"));
            Pause();

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='TenDanhMuc']")));
            tenInput.Clear();
            Pause();
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            Pause();

            Assert.That(_driver.Url, Does.Contain("/Edit/"));

            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;", tenInput);
            Assert.That(isInvalid, Is.True);

            Pause();
            Console.WriteLine("[PASS] Sua danh muc voi ten rong: Bi chan boi HTML5 required.");
        }

        // ==================== NHOM 4: SUA DANH MUC > HUY ====================

        [Test]
        [Order(6)]
        public void Test06_SuaDanhMuc_NutHuy()
        {
            NavigateToDanhMuc();
            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count, Is.GreaterThan(0));

            var firstRow = _driver.FindElement(By.CssSelector("table tbody tr"));
            string tenGoc = firstRow.FindElements(By.TagName("td"))[0].Text.Trim();

            firstRow.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Edit/"));
            Pause();

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='TenDanhMuc']")));
            tenInput.Clear();
            tenInput.SendKeys("Ten tam thoi khong luu");
            Pause();

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/DanhMuc']")));
            huyBtn.Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Pause();

            Assert.That(_driver.Url, Does.Contain("/DanhMuc"));
            Assert.That(TimDongTheoTen(tenGoc), Is.Not.Null, $"Ten goc '{tenGoc}' phai van con.");
            Assert.That(TimDongTheoTen("Ten tam thoi khong luu"), Is.Null);

            Pause();
            Console.WriteLine($"[PASS] Nut Huy trang Sua: Ten goc '{tenGoc}' van giu nguyen.");
        }

        // ==================== NHOM 5: XOA DANH MUC ====================

        [Test]
        [Order(7)]
        public void Test07_XoaDanhMuc_ThanhCong()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            TaoDanhMuc(TenDanhMucMoi, MoTaMoi);

            var dongCanXoa = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongCanXoa, Is.Not.Null);

            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].removeAttribute('onclick');", xoaBtn);
            xoaBtn.Click();
            _wait.Until(d => !d.Url.Contains("/Delete/"));
            Pause();

            NavigateToDanhMuc();
            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null);

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc - 1));

            Pause();
            Console.WriteLine($"[PASS] Xoa danh muc thanh cong. So dong: {soDongTruoc} -> {soDongSau}");
        }

        // ==================== NHOM 6: DIALOG XOA - OK ====================

        [Test]
        [Order(8)]
        public void Test08_XoaDanhMuc_Dialog_Ok()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            TaoDanhMuc(TenDanhMucMoi, MoTaMoi);

            var dongCanXoa = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongCanXoa, Is.Not.Null);
            Pause();

            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            Assert.That((xoaBtn.GetDomAttribute("onclick") ?? ""), Does.Contain("confirm"));

            ((IJavaScriptExecutor)_driver).ExecuteScript("window.confirm = function(){ return true; };");
            xoaBtn.Click();
            _wait.Until(d => !d.Url.Contains("/Delete/"));
            Pause();

            NavigateToDanhMuc();
            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null);

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc - 1));

            Pause();
            Console.WriteLine("[PASS] Dialog Xoa - OK: Danh muc da bi xoa.");
        }

        // ==================== NHOM 7: DIALOG XOA - HUY ====================

        [Test]
        [Order(9)]
        public void Test09_XoaDanhMuc_Dialog_Huy()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            TaoDanhMuc(TenDanhMucMoi, MoTaMoi);

            var dongCanXoa = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongCanXoa, Is.Not.Null);
            Pause();

            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            Assert.That((xoaBtn.GetDomAttribute("onclick") ?? ""), Does.Contain("confirm"));

            ((IJavaScriptExecutor)_driver).ExecuteScript("window.confirm = function(){ return false; };");
            xoaBtn.Click();
            Pause();

            Assert.That(_driver.Url, Does.Not.Contain("/Delete/"));
            Assert.That(_driver.Url, Does.Contain("/DanhMuc"));
            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Not.Null);

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc));

            Pause();
            Console.WriteLine($"[PASS] Dialog Xoa - Huy: Danh muc '{TenDanhMucMoi}' van con.");
        }

        // ==================== NHOM 8: DON DEP ====================

        [Test]
        [Order(10)]
        public void Test10_DonDep_XoaDuLieuTest()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            XoaNeuTonTai(TenDanhMucSua);

            NavigateToDanhMuc();
            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null);
            Assert.That(TimDongTheoTen(TenDanhMucSua), Is.Null);

            Pause();
            Console.WriteLine("[PASS] Don dep du lieu test danh muc thanh cong.");
        }
    }
}
