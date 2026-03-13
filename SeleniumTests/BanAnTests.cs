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
    public class BanAnTests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        private const string BaseUrl       = "http://localhost:5192";
        private const string BanAnUrl      = BaseUrl + "/BanAn";
        private const string LoginUrl      = BaseUrl + "/Dangnhap/Login";
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";

        private const string SoBanMoi  = "9901";
        private const string SoBanSua  = "9902";
        private const int Delay = 2000;

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
            _driver.FindElement(By.Name("password")).SendKeys(AdminPassword);
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang"));
            Pause();
        }

        private void NavigateToBanAn()
        {
            _driver.Navigate().GoToUrl(BanAnUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
            Pause();
        }

        private IWebElement? TimDongTheoSoBan(string soBan)
        {
            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            return rows.FirstOrDefault(r =>
            {
                var cells = r.FindElements(By.TagName("td"));
                if (cells.Count == 0) return false;
                string cellText = cells[0].Text.Trim();
                return cellText == $"Bàn {soBan}" || cellText == $"Ban {soBan}" || cellText == soBan;
            });
        }

        private void XoaNeuTonTai(string soBan)
        {
            NavigateToBanAn();
            var row = TimDongTheoSoBan(soBan);
            if (row != null)
            {
                var xoaBtn = row.FindElement(By.CssSelector("a.btn-danger"));
                ((IJavaScriptExecutor)_driver).ExecuteScript(
                    "arguments[0].removeAttribute('onclick');", xoaBtn);
                xoaBtn.Click();
                _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
                Pause();
            }
        }

        private void TaoBanAn(string soBan)
        {
            _driver.Navigate().GoToUrl(BanAnUrl + "/Create");
            _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='SoBan']")));
            Pause();

            var soBanInput = _driver.FindElement(By.CssSelector("input[name='SoBan']"));
            soBanInput.Clear();
            soBanInput.SendKeys(soBan);
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();
            NavigateToBanAn();
        }

        // ==================== TEST 1: XEM DANH SACH BAN AN ====================

        [Test]
        [Order(1)]
        public void Test01_XemDanhSachBanAn()
        {
            NavigateToBanAn();

            Assert.That(_driver.Url, Does.Contain("/BanAn"),
                "Phai dieu huong den trang /BanAn.");

            var themBtn = _driver.FindElement(By.CssSelector("a[href='/BanAn/Create']"));
            Assert.That(themBtn.Displayed, Is.True, "Phai co nut '+ Them Ban'.");

            bool coNoiDung =
                _driver.FindElements(By.CssSelector("table tbody tr")).Count > 0 ||
                _driver.FindElements(By.CssSelector(".no-data")).Count > 0;
            Assert.That(coNoiDung, Is.True, "Trang phai hien thi bang ban an hoac thong bao rong.");

            var headers = _driver.FindElements(By.CssSelector("table thead tr th"));
            Assert.That(headers.Count, Is.EqualTo(3), "Bang ban an phai co 3 cot.");

            Pause();
            Console.WriteLine($"[PASS] Xem danh sach ban an thanh cong. URL: {_driver.Url}");
        }

        // ==================== TEST 2: XEM DANH SACH BAN AN CO DU LIEU ====================

        [Test]
        [Order(2)]
        public void Test02_XemDanhSachBanAn_CoDuLieu()
        {
            NavigateToBanAn();

            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count == 0)
            {
                Assert.Pass("Chua co du lieu ban an de kiem tra.");
                return;
            }

            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                Assert.That(cells.Count, Is.EqualTo(3), "Moi dong phai co dung 3 cot.");
            }

            var badge = rows[0].FindElement(By.CssSelector("span.badge"));
            Assert.That(badge.Displayed, Is.True, "Cot Trang Thai phai co badge.");

            var suaBtn = rows[0].FindElement(By.CssSelector("a.btn-primary.btn-sm"));
            var xoaBtn = rows[0].FindElement(By.CssSelector("a.btn-danger.btn-sm"));
            Assert.That(suaBtn.Displayed, Is.True, "Phai co nut Sua.");
            Assert.That(xoaBtn.Displayed, Is.True, "Phai co nut Xoa.");

            Pause();
            Console.WriteLine($"[PASS] Danh sach ban an hien thi dung. So dong: {rows.Count}");
        }

        // ==================== TEST 3: THEM BAN AN THANH CONG ====================

        [Test]
        [Order(3)]
        public void Test03_ThemBanAn_ThanhCong()
        {
            XoaNeuTonTai(SoBanMoi);
            XoaNeuTonTai(SoBanSua);

            NavigateToBanAn();

            var themBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/BanAn/Create']")));
            themBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Create"));
            Pause();

            var soBanInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='SoBan']")));
            soBanInput.Clear();
            soBanInput.SendKeys(SoBanMoi);
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();

            NavigateToBanAn();

            var dongMoi = TimDongTheoSoBan(SoBanMoi);
            Assert.That(dongMoi, Is.Not.Null,
                $"Ban an so '{SoBanMoi}' phai xuat hien sau khi them.");

            Pause();
            Console.WriteLine($"[PASS] Them ban an thanh cong: So ban {SoBanMoi}");
        }

        // ==================== TEST 4: THEM BAN AN BO TRONG SO BAN ====================

        [Test]
        [Order(4)]
        public void Test04_ThemBanAn_BoTrongSoBan()
        {
            NavigateToBanAn();

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/BanAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Create"));
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            Pause();

            var soBanInput = _driver.FindElement(By.CssSelector("input[name='SoBan']"));
            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;", soBanInput);
            Assert.That(isInvalid, Is.True,
                "Input SoBan phai bi HTML5 validation khi bo trong.");

            Assert.That(_driver.Url, Does.Contain("/Create"),
                "Khi bo trong so ban phai o lai trang Create.");

            Pause();
            Console.WriteLine("[PASS] Bo trong so ban: Bi chan boi HTML5 required.");
        }

        // ==================== TEST 5: XEM TRANG SUA BAN AN ====================

        [Test]
        [Order(5)]
        public void Test05_XemTrangSuaBanAn()
        {
            NavigateToBanAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phai co ban an de test trang sua.");

            var suaBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-primary.btn-sm")));
            suaBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Edit/"));
            Pause();

            var soBanInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='SoBan']")));
            Assert.That(soBanInput.Displayed, Is.True, "Phai co input SoBan.");

            var trangThaiSelect = _driver.FindElement(By.CssSelector("select[name='TrangThai']"));
            Assert.That(trangThaiSelect.Displayed, Is.True, "Phai co select TrangThai.");

            string soBanHienTai = soBanInput.GetDomProperty("value") ?? "";
            Assert.That(soBanHienTai.Length, Is.GreaterThan(0),
                "SoBan phai co gia tri san tu DB.");

            Assert.That(_driver.FindElement(
                By.CssSelector("button[type='submit'].btn-success")).Displayed, Is.True,
                "Phai co nut 'Cap Nhat'.");
            Assert.That(_driver.FindElement(
                By.CssSelector("a.btn-light[href='/BanAn']")).Displayed, Is.True,
                "Phai co nut 'Huy'.");

            Pause();
            Console.WriteLine($"[PASS] Xem trang sua ban an. So ban: '{soBanHienTai}'");
        }

        // ==================== TEST 6: SUA BAN AN THANH CONG ====================

        [Test]
        [Order(6)]
        public void Test06_SuaBanAn_ThanhCong()
        {
            XoaNeuTonTai(SoBanMoi);
            XoaNeuTonTai(SoBanSua);
            TaoBanAn(SoBanMoi);

            var dongTest = TimDongTheoSoBan(SoBanMoi);
            Assert.That(dongTest, Is.Not.Null, $"Phai tim thay ban so '{SoBanMoi}'.");

            dongTest!.FindElement(By.CssSelector("a.btn-primary.btn-sm")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Edit/"));
            Pause();

            var soBanInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='SoBan']")));
            soBanInput.Clear();
            soBanInput.SendKeys(SoBanSua);
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Pause();

            NavigateToBanAn();

            var dongSua = TimDongTheoSoBan(SoBanSua);
            Assert.That(dongSua, Is.Not.Null,
                $"Ban so '{SoBanSua}' phai xuat hien sau khi sua.");

            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null,
                $"So ban cu '{SoBanMoi}' khong duoc ton tai sau khi sua.");

            Pause();
            Console.WriteLine($"[PASS] Sua ban an thanh cong: {SoBanMoi} -> {SoBanSua}");
        }

        // ==================== TEST 7: SUA BAN AN BO TRONG SO BAN ====================

        [Test]
        [Order(7)]
        public void Test07_SuaBanAn_BoTrongSoBan()
        {
            NavigateToBanAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phai co ban an de test sua voi so ban rong.");

            var suaBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-primary.btn-sm")));
            suaBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Edit/"));
            Pause();

            var soBanInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='SoBan']")));
            soBanInput.Clear();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            Pause();

            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;",
                    _driver.FindElement(By.CssSelector("input[name='SoBan']")));
            Assert.That(isInvalid, Is.True,
                "SoBan phai bi HTML5 validation khi bo trong luc sua.");

            Assert.That(_driver.Url, Does.Contain("/Edit/"),
                "Khi bo trong so ban luc sua phai o lai trang Edit.");

            Pause();
            Console.WriteLine("[PASS] Sua ban an voi so ban rong: Bi chan boi HTML5 required.");
        }

        // ==================== TEST 8: NUT HUY TRANG TAO ====================

        [Test]
        [Order(8)]
        public void Test08_NutHuy_TrangTao()
        {
            NavigateToBanAn();

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/BanAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Create"));
            Pause();

            _driver.FindElement(By.CssSelector("input[name='SoBan']")).SendKeys("9999");
            Pause();

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/BanAn']")));
            huyBtn.Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();

            Assert.That(_driver.Url, Does.Contain("/BanAn"),
                "Sau khi click Huy phai ve trang danh sach.");
            Assert.That(_driver.Url, Does.Not.Contain("/Create"),
                "Khong duoc o lai trang Create.");

            Assert.That(TimDongTheoSoBan("9999"), Is.Null,
                "Ban an khong duoc luu khi bam Huy.");

            Pause();
            Console.WriteLine("[PASS] Nut Huy trang Tao hoat dong dung.");
        }

        // ==================== TEST 9: NUT HUY TRANG SUA ====================

        [Test]
        [Order(9)]
        public void Test09_NutHuy_TrangSua()
        {
            NavigateToBanAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phai co ban an de test nut Huy.");

            var firstRow = _driver.FindElement(By.CssSelector("table tbody tr"));
            string soBanGocText = firstRow.FindElements(By.TagName("td"))[0].Text.Trim();

            firstRow.FindElement(By.CssSelector("a.btn-primary.btn-sm")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Edit/"));
            Pause();

            var soBanInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='SoBan']")));
            soBanInput.Clear();
            soBanInput.SendKeys("8888");
            Pause();

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/BanAn']")));
            huyBtn.Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Pause();

            Assert.That(_driver.Url, Does.Contain("/BanAn"),
                "Sau khi click Huy phai ve trang danh sach.");

            var dongGoc = _driver.FindElements(By.CssSelector("table tbody tr"))
                .FirstOrDefault(r =>
                {
                    var cells = r.FindElements(By.TagName("td"));
                    return cells.Count > 0 && cells[0].Text.Trim() == soBanGocText;
                });
            Assert.That(dongGoc, Is.Not.Null,
                $"Ban goc '{soBanGocText}' phai van con sau khi click Huy.");

            Pause();
            Console.WriteLine($"[PASS] Nut Huy trang Sua: Ban goc '{soBanGocText}' van giu nguyen.");
        }

        // ==================== TEST 10: XOA BAN AN THANH CONG ====================

        [Test]
        [Order(10)]
        public void Test10_XoaBanAn_ThanhCong()
        {
            XoaNeuTonTai(SoBanMoi);
            TaoBanAn(SoBanMoi);

            var dongCanXoa = TimDongTheoSoBan(SoBanMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Ban so '{SoBanMoi}' phai ton tai truoc khi xoa.");
            Pause();

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger.btn-sm"));
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].removeAttribute('onclick');", xoaBtn);
            xoaBtn.Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
            Pause();

            NavigateToBanAn();

            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null,
                $"Ban so '{SoBanMoi}' phai bi xoa khoi bang.");

            Pause();
            Console.WriteLine($"[PASS] Xoa ban an thanh cong: So ban {SoBanMoi}");
        }

        // ==================== TEST 11: XAC NHAN DIALOG KHI XOA ====================

        [Test]
        [Order(11)]
        public void Test11_XoaBanAn_XacNhanDialog()
        {
            XoaNeuTonTai(SoBanMoi);
            TaoBanAn(SoBanMoi);

            var dongCanXoa = TimDongTheoSoBan(SoBanMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Ban so '{SoBanMoi}' phai ton tai de test confirm dialog.");
            Pause();

            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("window.confirm = function(){ return true; };");

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger.btn-sm"));
            xoaBtn.Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
            Pause();

            NavigateToBanAn();

            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null,
                "Khi xac nhan 'OK' trong dialog, ban an phai bi xoa.");

            Pause();
            Console.WriteLine("[PASS] Xac nhan dialog xoa: Ban an da bi xoa sau khi bam OK.");
        }

        // ==================== TEST 12: DON DEP DU LIEU TEST ====================

        [Test]
        [Order(12)]
        public void Test12_DonDep_XoaDuLieuTest()
        {
            XoaNeuTonTai(SoBanMoi);
            XoaNeuTonTai(SoBanSua);
            XoaNeuTonTai("9999");
            XoaNeuTonTai("8888");

            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null,
                $"Ban so '{SoBanMoi}' phai duoc don dep.");
            Assert.That(TimDongTheoSoBan(SoBanSua), Is.Null,
                $"Ban so '{SoBanSua}' phai duoc don dep.");

            Pause();
            Console.WriteLine("[PASS] Don dep du lieu test ban an thanh cong.");
        }
    }
}
