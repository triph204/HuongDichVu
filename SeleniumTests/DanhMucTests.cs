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

        // Tên danh mục dùng xuyên suốt các test
        private const string TenDanhMucMoi  = "TEST_DM_Selenium";
        private const string TenDanhMucSua  = "TEST_DM_Selenium_EDITED";
        private const string MoTaMoi        = "Mô tả tự động tạo bởi Selenium";
        private const string MoTaSua        = "Mô tả đã được chỉnh sửa bởi Selenium";
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

        // Tìm dòng trong bảng theo tên danh mục
        private IWebElement? TimDongTheoTen(string tenDanhMuc)
        {
            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            return rows.FirstOrDefault(r => r.FindElements(By.TagName("td")).Count > 0
                                         && r.FindElements(By.TagName("td"))[0].Text.Trim() == tenDanhMuc);
        }

        // Xóa danh mục theo tên nếu tồn tại (dọn dẹp dữ liệu test)
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

        // Helper tạo danh mục mới và chờ redirect thành công
        private void TaoDanhMuc(string ten, string moTa)
        {
            _driver.Navigate().GoToUrl(DanhMucUrl + "/Create");
            _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
            Pause();
            _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']")).Clear();
            _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']")).SendKeys(ten);
            Pause();
            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).Clear();
            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).SendKeys(moTa);
            Pause();
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();
            NavigateToDanhMuc();
        }

        // ==================== TEST 1: XEM DANH SÁCH DANH MỤC ====================

        [Test]
        [Order(1)]
        public void Test01_XemDanhSachDanhMuc()
        {
            // Arrange
            NavigateToDanhMuc();

            // Assert - URL đúng
            Assert.That(_driver.Url, Does.Contain("/DanhMuc"),
                "Phải điều hướng đến trang /DanhMuc.");

            // Assert - Có nút Thêm Danh Mục
            var themBtn = _driver.FindElement(By.CssSelector("a[href='/DanhMuc/Create']"));
            Assert.That(themBtn.Displayed, Is.True, "Phải có nút 'Thêm Danh Mục'.");

            // Assert - Có bảng hoặc thông báo rỗng
            bool coNoiDung =
                _driver.FindElements(By.CssSelector("table tbody tr")).Count > 0 ||
                _driver.FindElements(By.CssSelector(".no-data")).Count > 0;
            Assert.That(coNoiDung, Is.True, "Trang phải hiển thị bảng danh mục hoặc thông báo rỗng.");

            // Assert - Bảng có đủ 3 cột: Tên, Mô tả, Hành động
            if (_driver.FindElements(By.CssSelector("table thead tr th")).Count > 0)
            {
                var headers = _driver.FindElements(By.CssSelector("table thead tr th"));
                Assert.That(headers.Count, Is.EqualTo(3), "Bảng phải có 3 cột.");
            }

            Pause();
            Console.WriteLine($"[PASS] Xem danh sách danh mục. URL: {_driver.Url}");
        }

        // ==================== TEST 2: THÊM DANH MỤC THÀNH CÔNG ====================

        [Test]
        [Order(2)]
        public void Test02_ThemDanhMuc_ThanhCong()
        {
            // Dọn dữ liệu cũ nếu còn
            XoaNeuTonTai(TenDanhMucMoi);
            XoaNeuTonTai(TenDanhMucSua);

            // Arrange
            NavigateToDanhMuc();

            // Act - Click nút Thêm Danh Mục
            var themBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/DanhMuc/Create']")));
            themBtn.Click();

            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Create"));
            Pause();
            Assert.That(_driver.Url, Does.Contain("/DanhMuc/Create"),
                "Phải chuyển sang trang tạo danh mục.");

            // Nhập dữ liệu
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
            tenInput.Clear();
            tenInput.SendKeys(TenDanhMucMoi);
            Pause();

            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.Clear();
            moTaInput.SendKeys(MoTaMoi);
            Pause();

            // Click Lưu
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();

            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();

            // Assert - Đã rời trang Create
            Assert.That(_driver.Url, Does.Not.Contain("/Create"),
                "Sau khi lưu phải redirect về trang danh sách.");

            // Reload lại để chắc chắn dữ liệu mới nhất
            NavigateToDanhMuc();

            // Assert - Danh mục mới xuất hiện trong bảng
            var dongMoi = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongMoi, Is.Not.Null,
                $"Danh mục '{TenDanhMucMoi}' phải xuất hiện trong bảng sau khi thêm.");

            // Assert - Mô tả đúng
            var cells = dongMoi!.FindElements(By.TagName("td"));
            Assert.That(cells[1].Text.Trim(), Is.EqualTo(MoTaMoi),
                "Mô tả danh mục phải khớp với giá trị đã nhập.");

            Pause();
            Console.WriteLine($"[PASS] Thêm danh mục thành công: '{TenDanhMucMoi}'");
        }

        // ==================== TEST 3: THÊM DANH MỤC BỎ TRỐNG TÊN ====================

        [Test]
        [Order(3)]
        public void Test03_ThemDanhMuc_BoTrongTen()
        {
            NavigateToDanhMuc();

            var themBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/DanhMuc/Create']")));
            themBtn.Click();

            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Create"));
            Pause();

            // Act - Không nhập tên, chỉ nhập mô tả rồi submit
            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.SendKeys("Mô tả không có tên");
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();

            Pause();

            // Assert - Không được redirect về danh sách
            Assert.That(_driver.Url, Does.Not.Contain("/DanhMuc").Or.Contain("/Create"),
                "Khi bỏ trống tên phải ở lại trang Create.");

            // Kiểm tra HTML5 validation trên input required
            var tenInput = _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']"));
            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;", tenInput);
            Assert.That(isInvalid, Is.True, "Input TenDanhMuc phải bị HTML5 validation khi bỏ trống.");

            Pause();
            Console.WriteLine("[PASS] Bỏ trống tên: Không thể thêm danh mục, bị chặn bởi HTML5 required.");
        }

        // ==================== TEST 4: XEM TRANG SỬA DANH MỤC ====================

        [Test]
        [Order(4)]
        public void Test04_XemTrangSuaDanhMuc()
        {
            NavigateToDanhMuc();

            // Phải có ít nhất 1 danh mục
            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phải có danh mục để test trang sửa.");

            // Act - Click nút Sửa đầu tiên
            var suaBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-primary.btn-sm")));
            string editHref = suaBtn.GetDomProperty("href") ?? "";
            suaBtn.Click();

            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Edit/"));
            Pause();

            // Assert - Chuyển sang trang Edit
            Assert.That(_driver.Url, Does.Contain("/DanhMuc/Edit/"),
                "Phải chuyển sang trang sửa danh mục.");

            // Assert - Form có input TenDanhMuc và textarea MoTa
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
            Assert.That(tenInput.Displayed, Is.True, "Phải có ô nhập TenDanhMuc.");

            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            Assert.That(moTaInput.Displayed, Is.True, "Phải có ô nhập MoTa.");

            // Assert - Input đã có giá trị sẵn (load từ DB)
            string tenHienTai = tenInput.GetDomProperty("value") ?? "";
            Assert.That(tenHienTai.Length, Is.GreaterThan(0),
                "Ô nhập TenDanhMuc phải có giá trị sẵn từ DB.");

            // Assert - Có nút Cập Nhật và Hủy
            var capNhatBtn = _driver.FindElement(By.CssSelector("button[type='submit'].btn-success"));
            Assert.That(capNhatBtn.Displayed, Is.True, "Phải có nút 'Cập Nhật'.");

            var huyBtn = _driver.FindElement(By.CssSelector("a.btn-light[href='/DanhMuc']"));
            Assert.That(huyBtn.Displayed, Is.True, "Phải có nút 'Hủy'.");

            Pause();
            Console.WriteLine($"[PASS] Xem trang sửa danh mục. URL: {_driver.Url}, Tên hiện tại: '{tenHienTai}'");
        }

        // ==================== TEST 5: SỬA DANH MỤC THÀNH CÔNG ====================

        [Test]
        [Order(5)]
        public void Test05_SuaDanhMuc_ThanhCong()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            XoaNeuTonTai(TenDanhMucSua);
            TaoDanhMuc(TenDanhMucMoi, MoTaMoi);

            // Tìm và click nút Sửa của danh mục test
            var dongTest = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongTest, Is.Not.Null, $"Phải tìm thấy danh mục '{TenDanhMucMoi}' để sửa.");

            dongTest!.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Edit/"));
            Pause();

            // Act - Xóa tên cũ, nhập tên mới
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
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

            Assert.That(_driver.Url, Does.Not.Contain("/Edit/"),
                "Sau khi cập nhật phải redirect về trang danh sách.");

            NavigateToDanhMuc();

            var dongSua = TimDongTheoTen(TenDanhMucSua);
            Assert.That(dongSua, Is.Not.Null,
                $"Danh mục '{TenDanhMucSua}' phải xuất hiện sau khi sửa.");

            var cells = dongSua!.FindElements(By.TagName("td"));
            Assert.That(cells[1].Text.Trim(), Is.EqualTo(MoTaSua),
                "Mô tả danh mục phải được cập nhật đúng.");

            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null,
                $"Tên cũ '{TenDanhMucMoi}' không được tồn tại sau khi sửa.");

            Pause();
            Console.WriteLine($"[PASS] Sửa danh mục thành công: '{TenDanhMucMoi}' -> '{TenDanhMucSua}'");
        }

        // ==================== TEST 6: SỬA DANH MỤC BỎ TRỐNG TÊN ====================

        [Test]
        [Order(6)]
        public void Test06_SuaDanhMuc_BoTrongTen()
        {
            NavigateToDanhMuc();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phải có danh mục để test sửa với tên rỗng.");

            var suaBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-primary.btn-sm")));
            suaBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Edit/"));
            Pause();

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
            tenInput.Clear();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            Pause();

            Assert.That(_driver.Url, Does.Contain("/Edit/"),
                "Khi bỏ trống tên khi sửa phải ở lại trang Edit.");

            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;",
                    _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']")));
            Assert.That(isInvalid, Is.True,
                "Input TenDanhMuc phải bị HTML5 validation khi bỏ trống khi sửa.");

            Pause();
            Console.WriteLine("[PASS] Sửa danh mục với tên rỗng: Bị chặn bởi HTML5 required.");
        }

        // ==================== TEST 7: NÚT HỦY TRÊN TRANG TẠO ====================

        [Test]
        [Order(7)]
        public void Test07_NutHuy_TrangTao()
        {
            NavigateToDanhMuc();

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/DanhMuc/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Create"));
            Pause();

            _driver.FindElement(By.CssSelector("input[name='TenDanhMuc']")).SendKeys("Tên sẽ bị hủy");
            Pause();

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/DanhMuc']")));
            huyBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();

            Assert.That(_driver.Url, Does.Contain("/DanhMuc"),
                "Sau khi click Hủy phải về trang danh sách danh mục.");
            Assert.That(_driver.Url, Does.Not.Contain("/Create"),
                "Không được ở lại trang Create sau khi click Hủy.");

            Assert.That(TimDongTheoTen("Tên sẽ bị hủy"), Is.Null,
                "Danh mục không được lưu khi bấm Hủy.");

            Pause();
            Console.WriteLine($"[PASS] Nút Hủy trang Tạo: Về lại danh sách. URL: {_driver.Url}");
        }

        // ==================== TEST 8: NÚT HỦY TRÊN TRANG SỬA ====================

        [Test]
        [Order(8)]
        public void Test08_NutHuy_TrangSua()
        {
            NavigateToDanhMuc();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phải có danh mục để test nút Hủy trang Sửa.");

            var firstRow = _driver.FindElement(By.CssSelector("table tbody tr"));
            string tenGoc = firstRow.FindElements(By.TagName("td"))[0].Text.Trim();

            firstRow.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DanhMuc/Edit/"));
            Pause();

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenDanhMuc']")));
            tenInput.Clear();
            tenInput.SendKeys("Tên tạm thời không lưu");
            Pause();

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/DanhMuc']")));
            huyBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Pause();

            Assert.That(_driver.Url, Does.Contain("/DanhMuc"),
                "Sau khi click Hủy phải về trang danh sách.");
            Assert.That(_driver.Url, Does.Not.Contain("/Edit/"),
                "Không được ở lại trang Edit sau khi click Hủy.");

            var dongGoc = TimDongTheoTen(tenGoc);
            Assert.That(dongGoc, Is.Not.Null,
                $"Tên gốc '{tenGoc}' phải vẫn còn sau khi click Hủy.");

            Pause();
            Console.WriteLine($"[PASS] Nút Hủy trang Sửa: Tên gốc '{tenGoc}' vẫn được giữ nguyên.");
        }

        // ==================== TEST 9: XÓA DANH MỤC THÀNH CÔNG ====================

        [Test]
        [Order(9)]
        public void Test09_XoaDanhMuc_ThanhCong()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            TaoDanhMuc(TenDanhMucMoi, MoTaMoi);

            var dongCanXoa = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Danh mục '{TenDanhMucMoi}' phải tồn tại trước khi xóa.");
            Pause();

            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].removeAttribute('onclick');", xoaBtn);
            xoaBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Delete/"));
            Pause();
            NavigateToDanhMuc();

            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null,
                $"Danh mục '{TenDanhMucMoi}' phải bị xóa khỏi bảng.");

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc - 1),
                "Số lượng danh mục phải giảm 1 sau khi xóa.");

            Pause();
            Console.WriteLine($"[PASS] Xóa danh mục thành công: '{TenDanhMucMoi}'. Số dòng: {soDongTruoc} -> {soDongSau}");
        }

        // ==================== TEST 10: XÁC NHẬN DIALOG KHI XÓA ====================

        [Test]
        [Order(10)]
        public void Test10_XoaDanhMuc_XacNhanDialog()
        {
            XoaNeuTonTai(TenDanhMucMoi);
            TaoDanhMuc(TenDanhMucMoi, MoTaMoi);

            var dongCanXoa = TimDongTheoTen(TenDanhMucMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Danh mục '{TenDanhMucMoi}' phải tồn tại để test confirm dialog.");
            Pause();

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("window.confirm = function(){ return true; };");
            xoaBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Delete/"));
            Pause();
            NavigateToDanhMuc();

            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null,
                "Khi xác nhận 'OK' trong dialog, danh mục phải bị xóa.");

            Pause();
            Console.WriteLine("[PASS] Xác nhận dialog xóa: Danh mục đã bị xóa sau khi bấm OK.");
        }

        // ==================== TEST 11: DỌN DẸP - XÓA DỮ LIỆU TEST ====================

        [Test]
        [Order(11)]
        public void Test11_DonDep_XoaDuLieuTest()
        {
            // Xóa tất cả dữ liệu test còn sót lại
            XoaNeuTonTai(TenDanhMucMoi);
            XoaNeuTonTai(TenDanhMucSua);

            NavigateToDanhMuc();

            // Assert - Không còn danh mục test nào
            Assert.That(TimDongTheoTen(TenDanhMucMoi), Is.Null,
                $"'{TenDanhMucMoi}' phải được dọn dẹp.");
            Assert.That(TimDongTheoTen(TenDanhMucSua), Is.Null,
                $"'{TenDanhMucSua}' phải được dọn dẹp.");

            Pause();
            Console.WriteLine("[PASS] Dọn dẹp dữ liệu test thành công.");
        }
    }
}
