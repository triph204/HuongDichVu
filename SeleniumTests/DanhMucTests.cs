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

        private const string BaseUrl       = "http://localhost:5192";
        private const string DanhMucUrl    = BaseUrl + "/DanhMuc";
        private const string LoginUrl      = BaseUrl + "/Dangnhap/Login";
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";

        // Tên danh m?c dùng xuyên su?t các test
        private const string TenDanhMucMoi  = "TEST_DM_Selenium";
        private const string TenDanhMucSua  = "TEST_DM_Selenium_EDITED";
        private const string MoTaMoi        = "Mo ta tu dong tao boi Selenium";
        private const string MoTaSua        = "Mo ta da duoc chinh sua boi Selenium";

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

        private void DangNhap()
        {
            _driver.Navigate().GoToUrl(LoginUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Name("username")));
            _driver.FindElement(By.Name("username")).SendKeys(AdminUsername);
            _driver.FindElement(By.Name("password")).SendKeys(AdminPassword);
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang"));
        }

        private void NavigateToDanhMuc()
        {
            _driver.Navigate().GoToUrl(DanhMucUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
        }

        // Tìm dòng trong b?ng theo tên danh m?c
        private IWebElement? TimDongTheoTen(string tenDanhMuc)
        {
            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            return rows.FirstOrDefault(r => r.FindElements(By.TagName("td")).Count > 0
                                         && r.FindElements(By.TagName("td"))[0].Text.Trim() == tenDanhMuc);
        }

        // Xóa danh m?c theo tên n?u t?n t?i (d?n d?p d? li?u test)
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
                // Ch? redirect v? danh sách và trang load xong
                _wait.Until(d => !d.Url.Contains("/Delete/"));
                _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
                System.Threading.Thread.Sleep(300);
            }
        }

        // Helper t?o danh m?c m?i và ch? redirect thành công
        private void TaoDanhMuc(string ten, string moTa)
        {
            _driver.Navigate().GoToUrl(DanhMucUrl + "/Create");
            _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
            _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']")).Clear();
            _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']")).SendKeys(ten);
            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).Clear();
            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).SendKeys(moTa);
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            NavigateToDanhMuc();
        }

        // ==================== TEST 1: XEM DANH SÁCH DANH M?C ====================

        [Test]
        [Order(1)]
        public void Test01_XemDanhSachDanhMuc()
        {
            // Arrange
            NavigateToDanhMuc();

            // Assert - URL ?úng
            Assert.That(_driver.Url, Does.Contain("/DanhMuc"),
                "Phai dieu huong den trang /DanhMuc.");

            // Assert - Có nút Thêm Danh M?c
            var themBtn = _driver.FindElement(By.CssSelector("a[href='/DanhMuc/Create']"));
            Assert.That(themBtn.Displayed, Is.True, "Phai co nut 'Them Danh Muc'.");

            // Assert - Có b?ng ho?c thông báo r?ng
            bool coNoiDung =
                _driver.FindElements(By.CssSelector("table tbody tr")).Count > 0 ||
                _driver.FindElements(By.CssSelector(".no-data")).Count > 0;
            Assert.That(coNoiDung, Is.True, "Trang phai hien thi bang danh muc hoac thong bao rong.");

            // Assert - B?ng có ?? 3 c?t: Tên, Mô t?, Hành ??ng
            if (_driver.FindElements(By.CssSelector("table thead tr th")).Count > 0)
            {
                var headers = _driver.FindElements(By.CssSelector("table thead tr th"));
                Assert.That(headers.Count, Is.EqualTo(3), "Bang phai co 3 cot.");
            }

            Console.WriteLine($"[PASS] Xem danh sach danh muc. URL: {_driver.Url}");
        }

        // ==================== TEST 2: THÊM DANH M?C THÀNH CÔNG ====================

        [Test]
        [Order(2)]
        public void Test02_ThemDanhMuc_ThanhCong()
        {
            // D?n d? li?u c? n?u còn
            XoaNeuTonTai(TenDanhMucMoi);
            XoaNeuTonTai(TenDanhMucSua);

            // Arrange
            NavigateToDanhMuc();

            // Act - Click nút Thêm Danh M?c
            var themBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/DanhMuc/Create']")));
            themBtn.Click();

            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Create"));
            Assert.That(_driver.Url, Does.Contain("/DanhMuc/Create"),
                "Phai chuyen sang trang tao danh muc.");

            // Nh?p d? li?u
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
            tenInput.Clear();
            tenInput.SendKeys(TenDanhMucMoi);

            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.Clear();
            moTaInput.SendKeys(MoTaMoi);

            // Click L?u
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();

            // Ch? redirect v? danh sách (t?i ?a 10 giây)
            _wait.Until(d => !d.Url.Contains("/Create"));

            // Assert - ?ã r?i trang Create
            Assert.That(_driver.Url, Does.Not.Contain("/Create"),
                "Sau khi luu phai redirect ve trang danh sach.");

            // Reload l?i ?? ch?c ch?n d? li?u m?i nh?t
            NavigateToDanhMuc();

            // Assert - Danh m?c m?i xu?t hi?n trong b?ng
            var dongMoi = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongMoi, Is.Not.Null,
                $"Danh muc '{TenDanhMucMoi}' phai xuat hien trong bang sau khi them.");

            // Assert - Mô t? ?úng
            var cells = dongMoi!.FindElements(By.TagName("td"));
            Assert.That(cells[1].Text.Trim(), Is.EqualTo(MoTaMoi),
                "Mo ta danh muc phai khop voi gia tri da nhap.");

            Console.WriteLine($"[PASS] Them danh muc thanh cong: '{TenDanhMucMoi}'");
        }

        // ==================== TEST 3: THÊM DANH M?C B? TR?NG TÊN ====================

        [Test]
        [Order(3)]
        public void Test03_ThemDanhMuc_BoTrongTen()
        {
            NavigateToDanhMuc();

            var themBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/DanhMuc/Create']")));
            themBtn.Click();

            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Create"));

            // Act - Không nh?p tên, ch? nh?p mô t? r?i submit
            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.SendKeys("Mo ta khong co ten");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();

            System.Threading.Thread.Sleep(1000);

            // Assert - Không ???c redirect v? danh sách (HTML5 required ch?n ho?c v?n ? trang Create)
            Assert.That(_driver.Url, Does.Not.Contain("/DanhMuc").Or.Contain("/Create"),
                "Khi bo trong ten phai o lai trang Create.");

            // Ki?m tra HTML5 validation trên input required
            var tenInput = _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']"));
            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;", tenInput);
            Assert.That(isInvalid, Is.True, "Input TenDanhMuc phai bi HTML5 validation khi bo trong.");

            Console.WriteLine("[PASS] Bo trong ten: Khong the them danh muc, bi ch?n boi HTML5 required.");
        }

        // ==================== TEST 4: XEM TRANG S?A DANH M?C ====================

        [Test]
        [Order(4)]
        public void Test04_XemTrangSuaDanhMuc()
        {
            NavigateToDanhMuc();

            // Ph?i có ít nh?t 1 danh m?c
            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phai co danh muc de test trang sua.");

            // Act - Click nút S?a ??u tiên
            var suaBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-primary.btn-sm")));
            string editHref = suaBtn.GetDomProperty("href") ?? "";
            suaBtn.Click();

            // Assert - Chuy?n sang trang Edit
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Edit/"));
            Assert.That(_driver.Url, Does.Contain("/DanhMuc/Edit/"),
                "Phai chuyen sang trang sua danh muc.");

            // Assert - Form có input TenDanhMuc và textarea MoTa
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
            Assert.That(tenInput.Displayed, Is.True, "Phai co o nhap TenDanhMuc.");

            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            Assert.That(moTaInput.Displayed, Is.True, "Phai co o nhap MoTa.");

            // Assert - Input ?ã có giá tr? s?n (load t? DB)
            string tenHienTai = tenInput.GetDomProperty("value") ?? "";
            Assert.That(tenHienTai.Length, Is.GreaterThan(0),
                "O nhap TenDanhMuc phai co gia tri san tu DB.");

            // Assert - Có nút C?p Nh?t và H?y
            var capNhatBtn = _driver.FindElement(By.CssSelector("button[type='submit'].btn-success"));
            Assert.That(capNhatBtn.Displayed, Is.True, "Phai co nut 'Cap Nhat'.");

            var huyBtn = _driver.FindElement(By.CssSelector("a.btn-light[href='/DanhMuc']"));
            Assert.That(huyBtn.Displayed, Is.True, "Phai co nut 'Huy'.");

            Console.WriteLine($"[PASS] Xem trang sua danh muc. URL: {_driver.Url}, Ten hien tai: '{tenHienTai}'");
        }

        // ==================== TEST 5: S?A DANH M?C THÀNH CÔNG ====================

        [Test]
        [Order(5)]
        public void Test05_SuaDanhMuc_ThanhCong()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            XoaNeuTonTai(TenDanhMucSua);
            TaoDanhMuc(TenDanhMucMoi, MoTaMoi);

            // Tìm và click nút S?a c?a danh m?c test
            var dongTest = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongTest, Is.Not.Null, $"Phai tim thay danh muc '{TenDanhMucMoi}' de sua.");

            dongTest!.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Edit/"));

            // Act - Xóa tên c?, nh?p tên m?i
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
            tenInput.Clear();
            tenInput.SendKeys(TenDanhMucSua);

            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.Clear();
            moTaInput.SendKeys(MoTaSua);

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();

            // Ch? redirect v? danh sách
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            NavigateToDanhMuc();

            Assert.That(_driver.Url, Does.Not.Contain("/Edit/"),
                "Sau khi cap nhat phai redirect ve trang danh sach.");

            var dongSua = TimDongTheoTen(TenDanhMucSua);
            Assert.That(dongSua, Is.Not.Null,
                $"Danh muc '{TenDanhMucSua}' phai xuat hien sau khi sua.");

            var cells = dongSua!.FindElements(By.TagName("td"));
            Assert.That(cells[1].Text.Trim(), Is.EqualTo(MoTaSua),
                "Mo ta danh muc phai duoc cap nhat dung.");

            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null,
                $"Ten cu '{TenDanhMucMoi}' khong duoc ton tai sau khi sua.");

            Console.WriteLine($"[PASS] Sua danh muc thanh cong: '{TenDanhMucMoi}' -> '{TenDanhMucSua}'");
        }

        // ==================== TEST 6: S?A DANH M?C B? TR?NG TÊN ====================

        [Test]
        [Order(6)]
        public void Test06_SuaDanhMuc_BoTrongTen()
        {
            NavigateToDanhMuc();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phai co danh muc de test sua voi ten rong.");

            var suaBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-primary.btn-sm")));
            suaBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Edit/"));

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
            tenInput.Clear();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            System.Threading.Thread.Sleep(1000);

            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;",
                    _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']")));
            Assert.That(isInvalid, Is.True,
                "Input TenDanhMuc phai bi HTML5 validation khi bo trong khi sua.");

            Assert.That(_driver.Url, Does.Contain("/Edit/"),
                "Khi bo trong ten khi sua phai o lai trang Edit.");

            Console.WriteLine("[PASS] Sua danh muc voi ten rong: Bi chan boi HTML5 required.");
        }

        // ==================== TEST 7: NÚT H?Y TRÊN TRANG T?O ====================

        [Test]
        [Order(7)]
        public void Test07_NutHuy_TrangTao()
        {
            NavigateToDanhMuc();

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/DanhMuc/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Create"));

            _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']")).SendKeys("Ten se bi huy");

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/DanhMuc']")));
            huyBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Create"));
            Assert.That(_driver.Url, Does.Contain("/DanhMuc"),
                "Sau khi click Huy phai ve trang danh sach danh muc.");
            Assert.That(_driver.Url, Does.Not.Contain("/Create"),
                "Khong duoc o lai trang Create sau khi click Huy.");

            Assert.That(TimDongTheoTen("Ten se bi huy"), Is.Null,
                "Danh muc khong duoc luu khi bam Huy.");

            Console.WriteLine($"[PASS] Nut Huy trang Tao: Ve lai danh sach. URL: {_driver.Url}");
        }

        // ==================== TEST 8: NÚT H?Y TRÊN TRANG S?A ====================

        [Test]
        [Order(8)]
        public void Test08_NutHuy_TrangSua()
        {
            NavigateToDanhMuc();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phai co danh muc de test nut Huy trang Sua.");

            var firstRow = _driver.FindElement(By.CssSelector("table tbody tr"));
            string tenGoc = firstRow.FindElements(By.TagName("td"))[0].Text.Trim();

            firstRow.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Edit/"));

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
            tenInput.Clear();
            tenInput.SendKeys("Ten tam thoi khong luu");

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/DanhMuc']")));
            huyBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Assert.That(_driver.Url, Does.Contain("/DanhMuc"),
                "Sau khi click Huy phai ve trang danh sach.");
            Assert.That(_driver.Url, Does.Not.Contain("/Edit/"),
                "Khong duoc o lai trang Edit sau khi click Huy.");

            var dongGoc = TimDongTheoTen(tenGoc);
            Assert.That(dongGoc, Is.Not.Null,
                $"Ten goc '{tenGoc}' phai van con sau khi click Huy.");

            Console.WriteLine($"[PASS] Nut Huy trang Sua: Ten goc '{tenGoc}' van duoc giu nguyen.");
        }

        // ==================== TEST 9: XÓA DANH M?C THÀNH CÔNG ====================

        [Test]
        [Order(9)]
        public void Test09_XoaDanhMuc_ThanhCong()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            TaoDanhMuc(TenDanhMucMoi, MoTaMoi);

            var dongCanXoa = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Danh muc '{TenDanhMucMoi}' phai ton tai truoc khi xoa.");

            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].removeAttribute('onclick');", xoaBtn);
            xoaBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Delete/"));
            NavigateToDanhMuc();

            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null,
                $"Danh muc '{TenDanhMucMoi}' phai bi xoa khoi bang.");

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc - 1),
                "So luong danh muc phai giam 1 sau khi xoa.");

            Console.WriteLine($"[PASS] Xoa danh muc thanh cong: '{TenDanhMucMoi}'. So dong: {soDongTruoc} -> {soDongSau}");
        }

        // ==================== TEST 10: XÁC NH?N DIALOG KHI XÓA ====================

        [Test]
        [Order(10)]
        public void Test10_XoaDanhMuc_XacNhanDialog()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            TaoDanhMuc(TenDanhMucMoi, MoTaMoi);

            var dongCanXoa = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Danh muc '{TenDanhMucMoi}' phai ton tai de test confirm dialog.");

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("window.confirm = function(){ return true; };");
            xoaBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Delete/"));
            NavigateToDanhMuc();

            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null,
                "Khi xac nhan 'OK' trong dialog, danh muc phai bi xoa.");

            Console.WriteLine("[PASS] Xac nhan dialog xoa: Danh muc da bi xoa sau khi bam OK.");
        }

        // ==================== TEST 11: D?N D?P - XÓA D? LI?U TEST ====================

        [Test]
        [Order(11)]
        public void Test11_DonDep_XoaDuLieuTest()
        {
            // Xóa t?t c? d? li?u test còn sót l?i
            XoaNeuTonTai(TenDanhMucMoi);
            XoaNeuTonTai(TenDanhMucSua);

            NavigateToDanhMuc();

            // Assert - Không còn danh m?c test nào
            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null,
                $"'{TenDanhMucMoi}' phai duoc don dep.");
            Assert.That(TimDongTheoTen(TenDanhMucSua), Is.Null,
                $"'{TenDanhMucSua}' phai duoc don dep.");

            Console.WriteLine("[PASS] Don dep du lieu test thanh cong.");
        }
    }
}
