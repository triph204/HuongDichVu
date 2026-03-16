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
    public class MonAnTests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        private const string BaseUrl       = "http://localhost:5192";
        private const string MonAnUrl      = BaseUrl + "/MonAn";
        private const string LoginUrl      = BaseUrl + "/Dangnhap/Login";
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";

        // Dữ liệu test
        private const string TenMonMoi  = "TEST_MON_Selenium";
        private const string TenMonSua  = "TEST_MON_Selenium_EDITED";
        private const string MoTaMoi    = "Mô tả món ăn tự động tạo bởi Selenium";
        private const string MoTaSua    = "Mô tả món ăn đã được chỉnh sửa bởi Selenium";
        private const long   GiaMoi     = 99000;
        private const long   GiaSua     = 149000;
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

        private void NavigateToMonAn()
        {
            _driver.Navigate().GoToUrl(MonAnUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
            Pause();
        }

        // Tìm dòng trong bảng theo tên món
        private IWebElement? TimDongTheoTen(string tenMon)
        {
            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            return rows.FirstOrDefault(r =>
                r.FindElements(By.TagName("td")).Count > 0 &&
                r.FindElements(By.TagName("td"))[0].Text.Trim() == tenMon);
        }

        // Xóa món ăn theo tên nếu tồn tại
        private void XoaNeuTonTai(string tenMon)
        {
            NavigateToMonAn();
            // Tìm trên tất cả các trang (tối đa 10 trang)
            for (int attempt = 0; attempt < 10; attempt++)
            {
                var row = TimDongTheoTen(tenMon);
                if (row != null)
                {
                    var xoaBtn = row.FindElement(By.CssSelector("a.btn-danger"));
                    ((IJavaScriptExecutor)_driver).ExecuteScript(
                        "arguments[0].removeAttribute('onclick');", xoaBtn);
                    xoaBtn.Click();
                    _wait.Until(d => !d.Url.Contains("/Delete/"));
                    _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
                    Pause();
                    return;
                }

                // Sang trang tiếp theo nếu có
                var nextBtn = _driver.FindElements(By.CssSelector(".pagination .page-link"))
                    .FirstOrDefault(e => e.Text.Contains("Sau"));
                if (nextBtn == null || !nextBtn.Displayed) break;
                nextBtn.Click();
                _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("table")));
            }
        }

        // Lấy danh mục đầu tiên trong select trang Create/Edit
        private string LayDanhMucDauTien()
        {
            var options = _driver.FindElements(By.CssSelector("select[name='DanhMucId'] option"))
                .Where(o => !string.IsNullOrWhiteSpace(o.GetDomProperty("value")) &&
                            o.GetDomProperty("value") != "0" &&
                            o.GetDomProperty("value") != "")
                .ToList();
            return options.Count > 0 ? (options[0].GetDomProperty("value") ?? "") : "";
        }

        // Helper tạo món ăn mới và chờ redirect
        private void TaoMonAn(string ten, long gia, string moTa)
        {
            _driver.Navigate().GoToUrl(MonAnUrl + "/Create");
            _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));
            Pause();

            _driver.FindElement(By.CssSelector("input[name='TenMon']")).Clear();
            _driver.FindElement(By.CssSelector("input[name='TenMon']")).SendKeys(ten);
            Pause();

            // Chọn danh mục đầu tiên hợp lệ
            string dmId = LayDanhMucDauTien();
            if (!string.IsNullOrEmpty(dmId))
            {
                new SelectElement(_driver.FindElement(
                    By.CssSelector("select[name='DanhMucId']"))).SelectByValue(dmId);
            }
            Pause();

            _driver.FindElement(By.CssSelector("input[name='Gia']")).Clear();
            _driver.FindElement(By.CssSelector("input[name='Gia']")).SendKeys(gia.ToString());
            Pause();

            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).Clear();
            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).SendKeys(moTa);
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();
            NavigateToMonAn();
        }

        // Tìm món ăn trên tất cả các trang
        private IWebElement? TimMonAnTrenTatCaTrang(string tenMon)
        {
            NavigateToMonAn();
            for (int attempt = 0; attempt < 10; attempt++)
            {
                var row = TimDongTheoTen(tenMon);
                if (row != null) return row;

                var nextBtn = _driver.FindElements(By.CssSelector(".pagination .page-link"))
                    .FirstOrDefault(e => e.Text.Contains("Sau"));
                if (nextBtn == null || !nextBtn.Displayed) break;
                nextBtn.Click();
                _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("table")));
            }
            return null;
        }

        // ==================== TEST 1: XEM DANH SÁCH MÓN ĂN ====================

        [Test]
        [Order(1)]
        public void Test01_XemDanhSachMonAn()
        {
            NavigateToMonAn();

            Assert.That(_driver.Url, Does.Contain("/MonAn"),
                "Phải điều hướng đến trang /MonAn.");

            var themBtn = _driver.FindElement(By.CssSelector("a[href='/MonAn/Create']"));
            Assert.That(themBtn.Displayed, Is.True, "Phải có nút 'Thêm Món'.");

            var filterSelect = _driver.FindElement(By.CssSelector("select#categoryFilter"));
            Assert.That(filterSelect.Displayed, Is.True, "Phải có dropdown lọc theo danh mục.");

            bool coNoiDung =
                _driver.FindElements(By.CssSelector("table tbody tr")).Count > 0 ||
                _driver.FindElements(By.CssSelector(".no-data")).Count > 0;
            Assert.That(coNoiDung, Is.True, "Trang phải hiển thị bảng món ăn hoặc thông báo rỗng.");

            var headers = _driver.FindElements(By.CssSelector("table thead tr th"));
            Assert.That(headers.Count, Is.EqualTo(5), "Bảng món ăn phải có 5 cột.");

            var infoText = _driver.FindElement(By.CssSelector(".card-body"));
            Assert.That(infoText.Text.Length, Is.GreaterThan(0), "Phải hiển thị thông tin tổng số món ăn.");

            Pause();
            Console.WriteLine($"[PASS] Xem danh sách món ăn thành công. URL: {_driver.Url}");
        }

        // ==================== TEST 2: LỌC THEO DANH MỤC ====================

        [Test]
        [Order(2)]
        public void Test02_LocTheoanhMuc()
        {
            NavigateToMonAn();

            // Lấy danh sách option trong filter (bỏ qua "Tất cả")
            var options = _driver.FindElements(By.CssSelector("select#categoryFilter option"))
                .Where(o =>
                {
                    var val = o.GetDomProperty("value") ?? "";
                    return val != "0" && val != "";
                }).ToList();

            if (options.Count == 0)
            {
                Assert.Pass("Không có danh mục nào để test lọc.");
                return;
            }

            // Chọn danh mục đầu tiên
            string dmId    = options[0].GetDomProperty("value") ?? "";
            string dmLabel = options[0].Text.Trim();

            new SelectElement(_driver.FindElement(
                By.CssSelector("select#categoryFilter"))).SelectByValue(dmId);
            Pause();

            _wait.Until(d => d.Url.Contains($"categoryId={dmId}"));

            Assert.That(_driver.Url, Does.Contain($"categoryId={dmId}"),
                "URL phải chứa categoryId sau khi lọc.");

            var infoSection = _driver.FindElement(By.CssSelector(".card-body"));
            Assert.That(infoSection.Text, Does.Contain(dmLabel),
                "Trang phải hiển thị tên danh mục đang lọc.");

            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count > 0)
            {
                bool tatCaDungDanhMuc = rows.All(r =>
                {
                    var cells = r.FindElements(By.TagName("td"));
                    return cells.Count > 1 && cells[1].Text.Trim() == dmLabel;
                });
                Assert.That(tatCaDungDanhMuc, Is.True,
                    "Tất cả món ăn hiển thị phải thuộc danh mục đã chọn.");
            }

            Pause();
            Console.WriteLine($"[PASS] Lọc theo danh mục '{dmLabel}' (ID={dmId}). URL: {_driver.Url}");
        }

        // ==================== TEST 3: XÓA LỌC DANH MỤC ====================

        [Test]
        [Order(3)]
        public void Test03_XoaLocDanhMuc()
        {
            NavigateToMonAn();

            // Chọn danh mục bất kỳ để lọc
            var options = _driver.FindElements(By.CssSelector("select#categoryFilter option"))
                .Where(o =>
                {
                    var val = o.GetDomProperty("value") ?? "";
                    return val != "0" && val != "";
                }).ToList();

            if (options.Count == 0)
            {
                Assert.Pass("Không có danh mục để test xóa lọc.");
                return;
            }

            string dmId = options[0].GetDomProperty("value") ?? "";
            new SelectElement(_driver.FindElement(
                By.CssSelector("select#categoryFilter"))).SelectByValue(dmId);
            _wait.Until(d => d.Url.Contains($"categoryId={dmId}"));

            var xoaLocBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light.btn-sm[href='/MonAn']")));
            Assert.That(xoaLocBtn.Displayed, Is.True, "Phải có nút 'Xóa lọc' khi đang lọc.");

            xoaLocBtn.Click();
            _wait.Until(d => !d.Url.Contains("categoryId=") || d.Url.Contains("categoryId=0"));
            Pause();

            Assert.That(_driver.Url, Does.Not.Contain($"categoryId={dmId}"),
                "Sau khi xóa lọc, URL không còn categoryId cũ.");

            var filterSelect = new SelectElement(
                _driver.FindElement(By.CssSelector("select#categoryFilter")));
            string selectedVal = filterSelect.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(selectedVal, Is.EqualTo("0"), "Dropdown filter phải trở về 'Tất cả danh mục'.");

            Pause();
            Console.WriteLine($"[PASS] Xóa lọc danh mục thành công. URL: {_driver.Url}");
        }

        // ==================== TEST 4: PHÂN TRANG ====================

        [Test]
        [Order(4)]
        public void Test04_PhanTrang()
        {
            NavigateToMonAn();

            var paginationItems = _driver.FindElements(
                By.CssSelector(".pagination .page-item, .pagination li"));

            if (paginationItems.Count == 0)
            {
                Console.WriteLine("[SKIP] Không đủ món ăn để phân trang (< 12). Test bỏ qua.");
                Assert.Pass("Không đủ dữ liệu để test phân trang.");
                return;
            }

            var activePage = _driver.FindElement(
                By.CssSelector(".pagination .page-item.active .page-link, .pagination li.active a"));
            Assert.That(activePage.Text.Trim(), Is.EqualTo("1"),
                "Trang đầu tiên phải đang active là trang 1.");

            var nextBtn = _driver.FindElements(By.CssSelector(".pagination .page-link"))
                .FirstOrDefault(e => e.Text.Contains("Sau"));

            if (nextBtn != null && nextBtn.Displayed)
            {
                nextBtn.Click();
                _wait.Until(d => d.Url.Contains("page=2"));
                Pause();

                Assert.That(_driver.Url, Does.Contain("page=2"),
                    "Sau khi click 'Sau', URL phải chứa page=2.");

                var activePage2 = _driver.FindElement(
                    By.CssSelector(".pagination .page-item.active .page-link, .pagination li.active a"));
                Assert.That(activePage2.Text.Trim(), Is.EqualTo("2"),
                    "Trang 2 phải đang active.");

                Console.WriteLine($"[PASS] Phân trang: Đã chuyển sang trang 2. URL: {_driver.Url}");

                // Tìm nút Trước để quay lại trang 1
                var prevBtn = _driver.FindElements(By.CssSelector(".pagination .page-link"))
                    .FirstOrDefault(e => e.Text.Contains("Sau") == false && e.Text.Contains("1") == false &&
                                        (e.Text.Contains("Tr") || e.Text.Contains("chevron-left") ||
                                         e.FindElements(By.CssSelector(".fa-chevron-left")).Count > 0));

                // Tìm lại nút Trước đơn giản hơn
                var allPageLinks = _driver.FindElements(By.CssSelector(".pagination .page-link"));
                var prevLink = allPageLinks.FirstOrDefault(e =>
                {
                    string text = e.Text.Trim();
                    string href = e.GetDomProperty("href") ?? "";
                    return href.Contains("page=1") && !text.Contains("Sau");
                });

                if (prevLink != null)
                {
                    prevLink.Click();
                    _wait.Until(d => !d.Url.Contains("page=2") || d.Url.Contains("page=1"));
                    Pause();
                    Console.WriteLine($"[PASS] Phân trang: Đã quay về trang 1. URL: {_driver.Url}");
                }
            }
            else
            {
                Console.WriteLine("[PASS] Chỉ có 1 trang món ăn, không có nút Sau.");
            }
        }

        // ==================== TEST 5: THÊM MÓN ĂN THÀNH CÔNG ====================

        [Test]
        [Order(5)]
        public void Test05_ThemMonAn_ThanhCong()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);

            NavigateToMonAn();

            var themBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/MonAn/Create']")));
            themBtn.Click();

            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Create"));
            Pause();
            Assert.That(_driver.Url, Does.Contain("/MonAn/Create"),
                "Phải chuyển sang trang tạo món ăn.");

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));
            tenInput.Clear();
            tenInput.SendKeys(TenMonMoi);
            Pause();

            string dmId = LayDanhMucDauTien();
            Assert.That(dmId, Is.Not.Empty, "Phải có danh mục để chọn khi tạo món ăn.");
            new SelectElement(_driver.FindElement(
                By.CssSelector("select[name='DanhMucId']"))).SelectByValue(dmId);
            Pause();

            var giaInput = _driver.FindElement(By.CssSelector("input[name='Gia']"));
            giaInput.Clear();
            giaInput.SendKeys(GiaMoi.ToString());
            Pause();

            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.Clear();
            moTaInput.SendKeys(MoTaMoi);
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();

            NavigateToMonAn();

            var dongMoi = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongMoi, Is.Not.Null,
                $"Món ăn '{TenMonMoi}' phải xuất hiện trong bảng sau khi thêm.");

            var cells = dongMoi!.FindElements(By.TagName("td"));
            string giaCell = cells[2].Text.Trim();
            Assert.That(giaCell.Length, Is.GreaterThan(0),
                "Cột Giá phải có giá trị hiển thị.");
            string giaDigits = new string(giaCell.Where(char.IsDigit).ToArray());
            Assert.That(giaDigits, Does.Contain(GiaMoi.ToString()),
                $"Giá món ăn phải chứa '{GiaMoi}', hiển thị: '{giaCell}'");

            Pause();
            Console.WriteLine($"[PASS] Thêm món ăn thành công: '{TenMonMoi}', giá: {giaCell}");
        }

        // ==================== TEST 6: THÊM MÓN ĂN BỎ TRỐNG TÊN ====================

        [Test]
        [Order(6)]
        public void Test06_ThemMonAn_BoTrongTen()
        {
            NavigateToMonAn();

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/MonAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Create"));
            Pause();

            // Chỉ nhập giá, không nhập tên
            string dmId = LayDanhMucDauTien();
            if (!string.IsNullOrEmpty(dmId))
                new SelectElement(_driver.FindElement(
                    By.CssSelector("select[name='DanhMucId']"))).SelectByValue(dmId);

            var giaInput = _driver.FindElement(By.CssSelector("input[name='Gia']"));
            giaInput.Clear();
            giaInput.SendKeys("50000");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            Pause();

            // Assert - HTML5 required chan submit
            var tenInput = _driver.FindElement(By.CssSelector("input[name='TenMon']"));
            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;", tenInput);
            Assert.That(isInvalid, Is.True,
                "Input TenMon phải bị HTML5 validation khi bỏ trống.");

            Assert.That(_driver.Url, Does.Contain("/Create"),
                "Khi bỏ trống tên phải ở lại trang Create.");

            Pause();
            Console.WriteLine("[PASS] Bỏ trống tên món ăn: Bị chặn bởi HTML5 required.");
        }

        // ==================== TEST 7: THÊM MÓN ĂN BỎ TRỐNG DANH MỤC ====================

        [Test]
        [Order(7)]
        public void Test07_ThemMonAn_BoTrongDanhMuc()
        {
            NavigateToMonAn();

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/MonAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Create"));
            Pause();

            // Chỉ nhập tên và giá, không chọn danh mục
            _driver.FindElement(By.CssSelector("input[name='TenMon']")).SendKeys("Món không có danh mục");
            _driver.FindElement(By.CssSelector("input[name='Gia']")).SendKeys("50000");
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            Pause();

            // Assert - HTML5 required chan submit
            var dmSelect = _driver.FindElement(By.CssSelector("select[name='DanhMucId']"));
            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;", dmSelect);
            Assert.That(isInvalid, Is.True,
                "Select DanhMucId phải bị HTML5 validation khi chưa chọn.");

            Assert.That(_driver.Url, Does.Contain("/Create"),
                "Khi bỏ trống danh mục phải ở lại trang Create.");

            Pause();
            Console.WriteLine("[PASS] Bỏ trống danh mục: Bị chặn bởi HTML5 required.");
        }

        // ==================== TEST 8: XEM TRANG SỬA MÓN ĂN ====================

        [Test]
        [Order(8)]
        public void Test08_XemTrangSuaMonAn()
        {
            NavigateToMonAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phải có món ăn để test trang sửa.");

            // Click nút Sửa đầu tiên
            var suaBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-primary.btn-sm")));
            suaBtn.Click();

            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));
            Pause();

            // Assert - Form có các trường cần thiết
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));
            Assert.That(tenInput.Displayed, Is.True, "Phải có ô nhập TenMon.");

            var dmSelect = _driver.FindElement(By.CssSelector("select[name='DanhMucId']"));
            Assert.That(dmSelect.Displayed, Is.True, "Phải có select DanhMucId.");

            var giaInput = _driver.FindElement(By.CssSelector("input[name='Gia']"));
            Assert.That(giaInput.Displayed, Is.True, "Phải có ô nhập Giá.");

            var coSanCheck = _driver.FindElement(By.CssSelector("input[name='CoSan']"));
            Assert.That(coSanCheck.Displayed, Is.True, "Phải có checkbox CoSan.");

            // Assert - Dữ liệu đã được load sẵn từ DB
            string tenHienTai = tenInput.GetDomProperty("value") ?? "";
            Assert.That(tenHienTai.Length, Is.GreaterThan(0),
                "TenMon phải có giá trị sẵn từ DB.");

            string giaHienTai = giaInput.GetDomProperty("value") ?? "";
            Assert.That(giaHienTai.Length, Is.GreaterThan(0),
                "Giá phải có giá trị sẵn từ DB.");

            // Assert - Có nút Cập Nhật và Hủy
            Assert.That(_driver.FindElement(
                By.CssSelector("button[type='submit'].btn-success")).Displayed, Is.True,
                "Phải có nút 'Cập Nhật'.");
            Assert.That(_driver.FindElement(
                By.CssSelector("a.btn-light[href='/MonAn']")).Displayed, Is.True,
                "Phải có nút 'Hủy'.");

            Console.WriteLine($"[PASS] Xem trang sửa món ăn. URL: {_driver.Url}, Tên: '{tenHienTai}'");
        }

        // ==================== TEST 9: SỬA MÓN ĂN THÀNH CÔNG ====================

        [Test]
        [Order(9)]
        public void Test09_SuaMonAn_ThanhCong()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            // Tìm và click nút Sửa của món test
            var dongTest = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongTest, Is.Not.Null, $"Phải tìm thấy món ăn '{TenMonMoi}' để sửa.");

            dongTest!.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));
            Pause();

            // Act - Cập nhật tên và giá
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));
            tenInput.Clear();
            tenInput.SendKeys(TenMonSua);
            Pause();

            var giaInput = _driver.FindElement(By.CssSelector("input[name='Gia']"));
            giaInput.Clear();
            giaInput.SendKeys(GiaSua.ToString());
            Pause();

            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.Clear();
            moTaInput.SendKeys(MoTaSua);
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Pause();
            NavigateToMonAn();

            Assert.That(_driver.Url, Does.Not.Contain("/Edit/"),
                "Sau khi cập nhật phải redirect về trang danh sách.");

            // Assert - Món ăn với tên mới xuất hiện
            var dongSua = TimMonAnTrenTatCaTrang(TenMonSua);
            Assert.That(dongSua, Is.Not.Null,
                $"Món ăn '{TenMonSua}' phải xuất hiện sau khi sửa.");

            // Assert - Giá mới đúng
            var cells = dongSua!.FindElements(By.TagName("td"));
            string giaCell = cells[2].Text.Trim();
            string giaDigits = new string(giaCell.Where(char.IsDigit).ToArray());
            Assert.That(giaDigits, Does.Contain(GiaSua.ToString()),
                $"Giá phải được cập nhật thành {GiaSua}, hiển thị: '{giaCell}'");

            // Assert - Tên cũ không còn
            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null,
                $"Tên cũ '{TenMonMoi}' không được tồn tại sau khi sửa.");

            Pause();
            Console.WriteLine($"[PASS] Sửa món ăn thành công: '{TenMonMoi}' -> '{TenMonSua}', giá: {GiaSua}");
        }

        // ==================== TEST 10: SỬA MÓN ĂN BỎ TRỐNG TÊN ====================

        [Test]
        [Order(10)]
        public void Test10_SuaMonAn_BoTrongTen()
        {
            NavigateToMonAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phải có món ăn để test sửa với tên rỗng.");

            var suaBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-primary.btn-sm")));
            suaBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));
            Pause();

            // Xóa trắng tên rồi submit
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));
            tenInput.Clear();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            Pause();

            // Assert - HTML5 required chan submit
            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;",
                    _driver.FindElement(By.CssSelector("input[name='TenMon']")));
            Assert.That(isInvalid, Is.True,
                "TenMon phải bị HTML5 validation khi bỏ trống lúc sửa.");

            Assert.That(_driver.Url, Does.Contain("/Edit/"),
                "Khi bỏ trống tên lúc sửa phải ở lại trang Edit.");

            Pause();
            Console.WriteLine("[PASS] Sửa món ăn với tên rỗng: Bị chặn bởi HTML5 required.");
        }

        // ==================== TEST 11: CẬP NHẬT CÓ SẴN ====================

        [Test]
        [Order(11)]
        public void Test11_CapNhat_CoSan()
        {
            XoaNeuTonTai(TenMonMoi);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            var dongTest = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongTest, Is.Not.Null, $"Phải tìm thấy món ăn '{TenMonMoi}'.");

            dongTest!.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));
            Pause();

            // Kiểm tra trạng thái checkbox CoSan hiện tại
            var coSanCheck = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='CoSan']")));
            bool isCheckedBefore = coSanCheck.Selected;

            // Toggle trạng thái CoSan
            coSanCheck.Click();
            Pause();
            bool isCheckedAfter = coSanCheck.Selected;
            Assert.That(isCheckedAfter, Is.Not.EqualTo(isCheckedBefore),
                "Trạng thái CoSan phải thay đổi sau khi click.");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Pause();
            NavigateToMonAn();

            // Assert - Badge CoSan thay doi tuong ung
            var dongSauSua = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongSauSua, Is.Not.Null, "Món ăn phải còn sau khi cập nhật CoSan.");

            var badgeCell = dongSauSua!.FindElements(By.TagName("td"))[3];
            string badgeClass = badgeCell.FindElement(By.CssSelector("span.badge")).GetDomProperty("className") ?? "";
            // isCheckedAfter = true => badge-success (Co san), false => badge-danger (Het)
            if (isCheckedAfter)
            {
                Assert.That(badgeClass, Does.Contain("badge-success"),
                    "Khi CoSan=true, badge phai la badge-success.");
            }
            else
            {
                Assert.That(badgeClass, Does.Contain("badge-danger"),
                    "Khi CoSan=false, badge phai la badge-danger.");
            }

            string badgeText = badgeCell.FindElement(By.CssSelector("span.badge")).Text;
            Console.WriteLine($"[PASS] Cập nhật CoSan: {isCheckedBefore} -> {isCheckedAfter}, badge class: '{badgeClass}', text: '{badgeText}'");
        }

        // ==================== TEST 12: NÚT HỦY TRANG TẠO ====================

        [Test]
        [Order(12)]
        public void Test12_NutHuy_TrangTao()
        {
            NavigateToMonAn();

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/MonAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Create"));
            Pause();

            _driver.FindElement(By.CssSelector("input[name='TenMon']")).SendKeys("Tên sẽ bị hủy");
            Pause();

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/MonAn']")));
            huyBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Create"));
            Pause();

            Assert.That(_driver.Url, Does.Contain("/MonAn"),
                "Sau khi click Hủy phải về trang danh sách món ăn.");
            Assert.That(_driver.Url, Does.Not.Contain("/Create"),
                "Không được ở lại trang Create sau khi click Hủy.");

            Assert.That(TimDongTheoTen("Tên sẽ bị hủy"), Is.Null,
                "Món ăn không được lưu khi bấm Hủy.");

            Console.WriteLine($"[PASS] Nút Hủy trang Tạo: Về lại danh sách. URL: {_driver.Url}");
        }

        // ==================== TEST 13: NÚT HỦY TRANG SỬA ====================

        [Test]
        [Order(13)]
        public void Test13_NutHuy_TrangSua()
        {
            NavigateToMonAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phải có món ăn để test nút Hủy trang Sửa.");

            // Lấy tên hiện tại của món đầu tiên
            var firstRow = _driver.FindElement(By.CssSelector("table tbody tr"));
            string tenGoc = firstRow.FindElements(By.TagName("td"))[0].Text.Trim();

            firstRow.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));
            Pause();

            // Nhập tên mới nhưng KHÔNG submit
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));
            tenInput.Clear();
            tenInput.SendKeys("Tên tạm thời sẽ không lưu");
            Pause();

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/MonAn']")));
            huyBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Pause();

            Assert.That(_driver.Url, Does.Contain("/MonAn"),
                "Sau khi click Hủy phải về trang danh sách.");
            Assert.That(_driver.Url, Does.Not.Contain("/Edit/"),
                "Không được ở lại trang Edit sau khi click Hủy.");

            var dongGoc = TimDongTheoTen(tenGoc);
            Assert.That(dongGoc, Is.Not.Null,
                $"Tên gốc '{tenGoc}' phải vẫn còn sau khi click Hủy.");

            Console.WriteLine($"[PASS] Nút Hủy trang Sửa: Tên gốc '{tenGoc}' vẫn được giữ nguyên.");
        }

        // ==================== TEST 14: XÓA MÓN ĂN THÀNH CÔNG ====================

        [Test]
        [Order(14)]
        public void Test14_XoaMonAn_ThanhCong()
        {
            XoaNeuTonTai(TenMonMoi);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            var dongCanXoa = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Món ăn '{TenMonMoi}' phải tồn tại trước khi xóa.");

            // Đếm số dòng trước khi xóa (trên trang hiện tại)
            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            // Act - Click nút Xóa (bỏ qua confirm bang JS)
            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].removeAttribute('onclick');", xoaBtn);
            xoaBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Delete/"));
            Pause();
            NavigateToMonAn();

            // Assert - Món ăn da bi xoa
            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null,
                $"Món ăn '{TenMonMoi}' phải bị xóa khỏi bảng.");

            // Assert - So dong sau khi xoa
            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc - 1),
                "So dong trong bang phai giam di 1 sau khi xoa mon an.");

            Console.WriteLine($"[PASS] Xóa món ăn thành công: '{TenMonMoi}'");
        }

        // ==================== TEST 15: XÁC NHẬN DIALOG KHI XÓA ====================

        [Test]
        [Order(15)]
        public void Test15_XoaMonAn_XacNhanDialog()
        {
            XoaNeuTonTai(TenMonMoi);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            var dongCanXoa = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Món ăn '{TenMonMoi}' phải tồn tại để test confirm dialog.");
            Pause();

            // Inject JS tu dong confirm = true
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("window.confirm = function(){ return true; };");

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            xoaBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Delete/"));
            Pause();
            NavigateToMonAn();

            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null,
                "Khi xác nhận 'OK' trong dialog, món ăn phải bị xóa.");

            Console.WriteLine("[PASS] Xác nhận dialog xóa: Món ăn đã bị xóa sau khi bấm OK.");
        }

        // ==================== TEST 16: DỌN DẸP DỮ LIỆU TEST ====================

        [Test]
        [Order(16)]
        public void Test16_DonDep_XoaDuLieuTest()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);

            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null,
                $"'{TenMonMoi}' phải được dọn dẹp.");
            Assert.That(TimMonAnTrenTatCaTrang(TenMonSua), Is.Null,
                $"'{TenMonSua}' phải được dọn dẹp.");

            Console.WriteLine("[PASS] Dọn dẹp dữ liệu test món ăn thành công.");
        }
    }
}
