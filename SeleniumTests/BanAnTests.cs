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
        private const int Delay = 1000;

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

        // ==================== TEST 1: XEM DANH SÁCH BÀN ĂN ====================

        [Test]
        [Order(1)]
        public void Test01_XemDanhSachBanAn()
        {
            NavigateToBanAn();

            Assert.That(_driver.Url, Does.Contain("/BanAn"),
                "Phải điều hướng đến trang /BanAn.");

            var themBtn = _driver.FindElement(By.CssSelector("a[href='/BanAn/Create']"));
            Assert.That(themBtn.Displayed, Is.True, "Phải có nút '+ Thêm Bàn'.");

            bool coNoiDung =
                _driver.FindElements(By.CssSelector("table tbody tr")).Count > 0 ||
                _driver.FindElements(By.CssSelector(".no-data")).Count > 0;
            Assert.That(coNoiDung, Is.True, "Trang phải hiển thị bảng bàn ăn hoặc thông báo rỗng.");

            var headers = _driver.FindElements(By.CssSelector("table thead tr th"));
            Assert.That(headers.Count, Is.EqualTo(3), "Bảng bàn ăn phải có 3 cột.");

            Pause();
            Console.WriteLine($"[PASS] Xem danh sách bàn ăn thành công. URL: {_driver.Url}");
        }

        // ==================== TEST 2: XEM DANH SÁCH BÀN ĂN CÓ DỮ LIỆU ====================

        [Test]
        [Order(2)]
        public void Test02_XemDanhSachBanAn_CoDuLieu()
        {
            NavigateToBanAn();

            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count == 0)
            {
                Assert.Pass("Chưa có dữ liệu bàn ăn để kiểm tra.");
                return;
            }

            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                Assert.That(cells.Count, Is.EqualTo(3), "Mỗi dòng phải có đúng 3 cột.");
            }

            var badge = rows[0].FindElement(By.CssSelector("span.badge"));
            Assert.That(badge.Displayed, Is.True, "Cột Trạng Thái phải có badge.");

            var suaBtn = rows[0].FindElement(By.CssSelector("a.btn-primary.btn-sm"));
            var xoaBtn = rows[0].FindElement(By.CssSelector("a.btn-danger.btn-sm"));
            Assert.That(suaBtn.Displayed, Is.True, "Phải có nút Sửa.");
            Assert.That(xoaBtn.Displayed, Is.True, "Phải có nút Xóa.");

            Pause();
            Console.WriteLine($"[PASS] Danh sách bàn ăn hiển thị đúng. Số dòng: {rows.Count}");
        }

        // ==================== TEST 3: THÊM BÀN ĂN THÀNH CÔNG ====================

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
                $"Bàn ăn số '{SoBanMoi}' phải xuất hiện sau khi thêm.");

            Pause();
            Console.WriteLine($"[PASS] Thêm bàn ăn thành công: Số bàn {SoBanMoi}");
        }

        // ==================== TEST 4: THÊM BÀN ĂN BỎ TRỐNG SỐ BÀN ====================

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
                "Input SoBan phải bị HTML5 validation khi bỏ trống.");

            Assert.That(_driver.Url, Does.Contain("/Create"),
                "Khi bỏ trống số bàn phải ở lại trang Create.");

            Pause();
            Console.WriteLine("[PASS] Bỏ trống số bàn: Bị chặn bởi HTML5 required.");
        }

        // ==================== TEST 5: XEM TRANG SỬA BÀN ĂN ====================

        [Test]
        [Order(5)]
        public void Test05_XemTrangSuaBanAn()
        {
            NavigateToBanAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phải có bàn ăn để test trang sửa.");

            var suaBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-primary.btn-sm")));
            suaBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Edit/"));
            Pause();

            var soBanInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='SoBan']")));
            Assert.That(soBanInput.Displayed, Is.True, "Phải có input SoBan.");

            var trangThaiSelect = _driver.FindElement(By.CssSelector("select[name='TrangThai']"));
            Assert.That(trangThaiSelect.Displayed, Is.True, "Phải có select TrangThai.");

            string soBanHienTai = soBanInput.GetDomProperty("value") ?? "";
            Assert.That(soBanHienTai.Length, Is.GreaterThan(0),
                "SoBan phải có giá trị sẵn từ DB.");

            Assert.That(_driver.FindElement(
                By.CssSelector("button[type='submit'].btn-success")).Displayed, Is.True,
                "Phải có nút 'Cập Nhật'.");
            Assert.That(_driver.FindElement(
                By.CssSelector("a.btn-light[href='/BanAn']")).Displayed, Is.True,
                "Phải có nút 'Hủy'.");

            Pause();
            Console.WriteLine($"[PASS] Xem trang sửa bàn ăn. Số bàn: '{soBanHienTai}'");
        }

        // ==================== TEST 6: SỬA BÀN ĂN THÀNH CÔNG ====================

        [Test]
        [Order(6)]
        public void Test06_SuaBanAn_ThanhCong()
        {
            XoaNeuTonTai(SoBanMoi);
            XoaNeuTonTai(SoBanSua);
            TaoBanAn(SoBanMoi);

            var dongTest = TimDongTheoSoBan(SoBanMoi);
            Assert.That(dongTest, Is.Not.Null, $"Phải tìm thấy bàn số '{SoBanMoi}'.");

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
                $"Bàn số '{SoBanSua}' phải xuất hiện sau khi sửa.");

            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null,
                $"Số bàn cũ '{SoBanMoi}' không được tồn tại sau khi sửa.");

            Pause();
            Console.WriteLine($"[PASS] Sửa bàn ăn thành công: {SoBanMoi} -> {SoBanSua}");
        }

        // ==================== TEST 7: SỬA BÀN ĂN BỎ TRỐNG SỐ BÀN ====================

        [Test]
        [Order(7)]
        public void Test07_SuaBanAn_BoTrongSoBan()
        {
            NavigateToBanAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phải có bàn ăn để test sửa với số bàn rỗng.");

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
                "SoBan phải bị HTML5 validation khi bỏ trống lúc sửa.");

            Assert.That(_driver.Url, Does.Contain("/Edit/"),
                "Khi bỏ trống số bàn lúc sửa phải ở lại trang Edit.");

            Pause();
            Console.WriteLine("[PASS] Sửa bàn ăn với số bàn rỗng: Bị chặn bởi HTML5 required.");
        }

        // ==================== TEST 8: NÚT HỦY TRANG TẠO ====================

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
                "Sau khi click Hủy phải về trang danh sách.");
            Assert.That(_driver.Url, Does.Not.Contain("/Create"),
                "Không được ở lại trang Create.");

            Assert.That(TimDongTheoSoBan("9999"), Is.Null,
                "Bàn ăn không được lưu khi bấm Hủy.");

            Pause();
            Console.WriteLine("[PASS] Nút Hủy trang Tạo hoạt động đúng.");
        }

        // ==================== TEST 9: NÚT HỦY TRANG SỬA ====================

        [Test]
        [Order(9)]
        public void Test09_NutHuy_TrangSua()
        {
            NavigateToBanAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phải có bàn ăn để test nút Hủy.");

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
                "Sau khi click Hủy phải về trang danh sách.");

            var dongGoc = _driver.FindElements(By.CssSelector("table tbody tr"))
                .FirstOrDefault(r =>
                {
                    var cells = r.FindElements(By.TagName("td"));
                    return cells.Count > 0 && cells[0].Text.Trim() == soBanGocText;
                });
            Assert.That(dongGoc, Is.Not.Null,
                $"Bàn gốc '{soBanGocText}' phải vẫn còn sau khi click Hủy.");

            Pause();
            Console.WriteLine($"[PASS] Nút Hủy trang Sửa: Bàn gốc '{soBanGocText}' vẫn giữ nguyên.");
        }

        // ==================== TEST 10: XÓA BÀN ĂN THÀNH CÔNG ====================

        [Test]
        [Order(10)]
        public void Test10_XoaBanAn_ThanhCong()
        {
            XoaNeuTonTai(SoBanMoi);
            TaoBanAn(SoBanMoi);

            var dongCanXoa = TimDongTheoSoBan(SoBanMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Bàn số '{SoBanMoi}' phải tồn tại trước khi xóa.");
            Pause();

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger.btn-sm"));
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].removeAttribute('onclick');", xoaBtn);
            xoaBtn.Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
            Pause();

            NavigateToBanAn();

            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null,
                $"Bàn số '{SoBanMoi}' phải bị xóa khỏi bảng.");

            Pause();
            Console.WriteLine($"[PASS] Xóa bàn ăn thành công: Số bàn {SoBanMoi}");
        }

        // ==================== TEST 11: XÁC NHẬN DIALOG KHI XÓA ====================

        [Test]
        [Order(11)]
        public void Test11_XoaBanAn_XacNhanDialog()
        {
            XoaNeuTonTai(SoBanMoi);
            TaoBanAn(SoBanMoi);

            var dongCanXoa = TimDongTheoSoBan(SoBanMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Bàn số '{SoBanMoi}' phải tồn tại để test confirm dialog.");
            Pause();

            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("window.confirm = function(){ return true; };");

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger.btn-sm"));
            xoaBtn.Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
            Pause();

            NavigateToBanAn();

            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null,
                "Khi xác nhận 'OK' trong dialog, bàn ăn phải bị xóa.");

            Pause();
            Console.WriteLine("[PASS] Xác nhận dialog xóa: Bàn ăn đã bị xóa sau khi bấm OK.");
        }

        // ==================== TEST 12: DỌN DẸP DỮ LIỆU TEST ====================

        [Test]
        [Order(12)]
        public void Test12_DonDep_XoaDuLieuTest()
        {
            XoaNeuTonTai(SoBanMoi);
            XoaNeuTonTai(SoBanSua);
            XoaNeuTonTai("9999");
            XoaNeuTonTai("8888");

            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null,
                $"Bàn số '{SoBanMoi}' phải được dọn dẹp.");
            Assert.That(TimDongTheoSoBan(SoBanSua), Is.Null,
                $"Bàn số '{SoBanSua}' phải được dọn dẹp.");

            Pause();
            Console.WriteLine("[PASS] Dọn dẹp dữ liệu test bàn ăn thành công.");
        }
    }
}
