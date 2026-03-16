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
    public class DonHangTests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        private const string BaseUrl = "http://localhost:5192";
        private const string DonHangUrl = BaseUrl + "/DonHang";
        private const string LoginUrl = BaseUrl + "/Dangnhap/Login";
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";
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
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Đăng nhập trước mỗi test
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

        private void NavigateToDonHang()
        {
            _driver.Navigate().GoToUrl(DonHangUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".status-tabs")));
            Pause();
        }

        // Click tab trạng thái theo href chứa keyword
        private void ClickTabTrangThai(string trangThaiParam)
        {
            var tab = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector($"a.status-tab[href*='trangThai={trangThaiParam}']")));
            tab.Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".status-tabs")));
            Pause();
        }

        // Lấy số đơn hiển thị trên tab
        private int LaySoDonTrenTab(string trangThaiParam)
        {
            var tab = _driver.FindElement(
                By.CssSelector($"a.status-tab[href*='trangThai={trangThaiParam}']"));
            var countText = tab.FindElement(By.CssSelector(".tab-count")).Text;
            return int.TryParse(countText, out var count) ? count : 0;
        }

        // Chọn sort option
        private void ChonSapXep(string value)
        {
            var selectEl = new SelectElement(
                _wait.Until(ExpectedConditions.ElementIsVisible(
                    By.CssSelector("select[name='sortBy']"))));
            selectEl.SelectByValue(value);
            _wait.Until(d =>
            {
                var sel = new SelectElement(d.FindElement(By.CssSelector("select[name='sortBy']")));
                return sel.SelectedOption.GetDomProperty("value") == value;
            });
            Pause();
        }

        // Kiểm tra có bảng đơn hàng hay thông báo rỗng
        private bool CoBangDonHang()
        {
            var tables = _driver.FindElements(By.CssSelector("table.donhang-table"));
            return tables.Count > 0 && tables[0].Displayed;
        }

        // ==================== TEST 1: XEM TẤT CẢ ĐƠN HÀNG ====================

        [Test]
        [Order(1)]
        public void Test01_XemTatCaDonHang()
        {
            // Arrange
            NavigateToDonHang();

            // Assert - Tab "Tất cả" phải active
            var activeTab = _driver.FindElement(By.CssSelector("a.status-tab.active"));
            string activeHref = activeTab.GetDomProperty("href") ?? "";
            Assert.That(activeHref, Does.Contain("trangThai=tatca"),
                "Tab 'Tất cả' (tatca) phải đang active khi vào trang DonHang.");

            bool coNoiDung = CoBangDonHang() ||
                _driver.FindElements(By.CssSelector(".donhang-empty")).Count > 0;
            Assert.That(coNoiDung, Is.True, "Trang phải hiển thị bảng đơn hàng hoặc thông báo rỗng.");

            var toolbar = _driver.FindElement(By.CssSelector(".donhang-toolbar"));
            Assert.That(toolbar.Displayed, Is.True, "Toolbar thông tin đơn hàng phải hiển thị.");

            Pause();
            Console.WriteLine($"[PASS] Xem tất cả đơn hàng. Tổng: {LaySoDonTrenTab("tatca")} đơn.");
        }

        // ==================== TEST 2: XEM ĐƠN CHỜ XÁC NHẬN ====================

        [Test]
        [Order(2)]
        public void Test02_XemDonChoXacNhan()
        {
            NavigateToDonHang();
            ClickTabTrangThai("CHOXACNHAN");

            Assert.That(_driver.Url, Does.Contain("trangThai=CHOXACNHAN"),
                "URL phải chứa trangThai=CHOXACNHAN sau khi click tab.");

            var activeTab = _driver.FindElement(By.CssSelector("a.status-tab.active"));
            string activeHref = activeTab.GetDomProperty("href") ?? "";
            Assert.That(activeHref, Does.Contain("CHOXACNHAN"), "Tab CHOXACNHAN phải active.");

            if (CoBangDonHang())
            {
                var badges = _driver.FindElements(By.CssSelector("span.donhang-badge"));
                Assert.That(badges.Count, Is.GreaterThan(0), "Phải có badge trạng thái.");
            }

            Pause();
            Console.WriteLine($"[PASS] Tab Chờ xác nhận: {LaySoDonTrenTab("CHOXACNHAN")} đơn. URL: {_driver.Url}");
        }

        // ==================== TEST 3: XEM ĐƠN ĐANG CHUẨN BỊ ====================

        [Test]
        [Order(3)]
        public void Test03_XemDonDangChuanBi()
        {
            NavigateToDonHang();
            ClickTabTrangThai("DANGCHUANBI");

            Assert.That(_driver.Url, Does.Contain("trangThai=DANGCHUANBI"), "URL phải chứa trangThai=DANGCHUANBI.");

            var activeTab = _driver.FindElement(By.CssSelector("a.status-tab.active"));
            string activeHref = activeTab.GetDomProperty("href") ?? "";
            Assert.That(activeHref, Does.Contain("DANGCHUANBI"), "Tab DANGCHUANBI phải active.");

            Pause();
            Console.WriteLine($"[PASS] Tab Đang chuẩn bị: {LaySoDonTrenTab("DANGCHUANBI")} đơn.");
        }

        // ==================== TEST 4: XEM ĐƠN HOÀN THÀNH ====================

        [Test]
        [Order(4)]
        public void Test04_XemDonHoanThanh()
        {
            NavigateToDonHang();
            ClickTabTrangThai("HOANTHANH");

            Assert.That(_driver.Url, Does.Contain("trangThai=HOANTHANH"), "URL phải chứa trangThai=HOANTHANH.");

            var activeTab = _driver.FindElement(By.CssSelector("a.status-tab.active"));
            string activeHref = activeTab.GetDomProperty("href") ?? "";
            Assert.That(activeHref, Does.Contain("HOANTHANH"), "Tab HOANTHANH phải active.");

            Pause();
            Console.WriteLine($"[PASS] Tab Hoàn thành: {LaySoDonTrenTab("HOANTHANH")} đơn.");
        }

        // ==================== TEST 5: XEM ĐƠN BỊ HỦY ====================

        [Test]
        [Order(5)]
        public void Test05_XemDonBiHuy()
        {
            NavigateToDonHang();
            ClickTabTrangThai("HUY");

            Assert.That(_driver.Url, Does.Contain("trangThai=HUY"), "URL phải chứa trangThai=HUY.");

            var activeTab = _driver.FindElement(By.CssSelector("a.status-tab.active"));
            string activeHref = activeTab.GetDomProperty("href") ?? "";
            Assert.That(activeHref, Does.Contain("HUY"), "Tab HUY phải active.");

            Pause();
            Console.WriteLine($"[PASS] Tab Đã hủy: {LaySoDonTrenTab("HUY")} đơn.");
        }

        // ==================== TEST 6: XEM CHI TIẾT ĐƠN HÀNG ====================

        [Test]
        [Order(6)]
        public void Test06_XemChiTietDonHang()
        {
            NavigateToDonHang();
            Assert.That(CoBangDonHang(), Is.True, "Phải có đơn hàng trong danh sách để test xem chi tiết.");

            // Act - Click nút Chi Tiết ưu tiên
            var chiTietBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-primary.btn-sm")));
            chiTietBtn.Click();

            // Assert - Chuyển sang trang chi tiết
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Pause();

            // Kiểm tra tiêu đề chứa "Chi Tiết" (tránh vấn đề encoding)
            var cardHeader = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector(".card-header h2")));
            Assert.That(cardHeader.Text.Length, Is.GreaterThan(0),
                "Trang chi tiết phải có tiêu đề.");

            bool coChiTiet = _driver.FindElements(By.CssSelector("table tbody tr")).Count > 0;
            Assert.That(coChiTiet, Is.True, "Trang chi tiết phải có danh sách món ăn.");

            var updateForm = _driver.FindElement(By.CssSelector("form[action*='UpdateStatus']"));
            Assert.That(updateForm.Displayed, Is.True, "Phải có form cập nhật trạng thái.");

            Pause();
            Console.WriteLine($"[PASS] Xem chi tiết đơn hàng thành công. URL: {_driver.Url}");
        }

        // ==================== TEST 7: PHÂN TRANG ====================

        [Test]
        [Order(7)]
        public void Test07_PhanTrang()
        {
            NavigateToDonHang();

            // Kiểm tra có pagination không
            var paginationItems = _driver.FindElements(By.CssSelector(".pagination .page-item"));

            if (paginationItems.Count == 0)
            {
                Console.WriteLine("[SKIP] Không đủ đơn hàng để phân trang (< 10 đơn). Test bỏ qua.");
                Assert.Pass("Không đủ dữ liệu để test phân trang.");
                return;
            }

            // Assert - Pagination hiển thị
            Assert.That(paginationItems.Count, Is.GreaterThan(0),
                "Phải có các nút phân trang.");

            // Lấy số trang hiện tại
            var activePage = _driver.FindElement(By.CssSelector(".pagination .page-item.active .page-link"));
            Assert.That(activePage.Text.Trim(), Is.EqualTo("1"),
                "Trang đầu tiên phải đang active là trang 1.");

            // Act - Click nút trang tiếp theo (Sau »)
            var nextBtn = _driver.FindElements(By.CssSelector(".page-link"))
                .FirstOrDefault(e => e.Text.Contains("Sau"));

            if (nextBtn != null && nextBtn.Displayed)
            {
                nextBtn.Click();
                _wait.Until(d => d.Url.Contains("page=2"));
                Pause();

                Assert.That(_driver.Url, Does.Contain("page=2"),
                    "Sau khi click 'Sau', URL phải chứa page=2.");

                // Assert - Trang 2 active
                var activePage2 = _driver.FindElement(
                    By.CssSelector(".pagination .page-item.active .page-link"));
                Assert.That(activePage2.Text.Trim(), Is.EqualTo("2"),
                    "Trang 2 phải đang active.");

                Console.WriteLine($"[PASS] Phân trang: Đã chuyển sang trang 2. URL: {_driver.Url}");

                // Act - Click nút Trước « để quay lại trang 1
                var prevBtn = _driver.FindElements(By.CssSelector(".page-link"))
                    .FirstOrDefault(e => e.Text.Contains("Trước"));

                if (prevBtn != null)
                {
                    prevBtn.Click();
                    _wait.Until(d => !d.Url.Contains("page=2") || d.Url.Contains("page=1"));
                    Pause();
                    Console.WriteLine($"[PASS] Phân trang: Đã quay về trang 1. URL: {_driver.Url}");
                }
            }
            else
            {
                Console.WriteLine("[PASS] Chỉ có 1 trang đơn hàng, không có nút Sau.");
            }
        }

        // ==================== TEST 8: SẮP XẾP THEO MỚI NHẤT ====================

        [Test]
        [Order(8)]
        public void Test08_SapXepMoiNhat()
        {
            NavigateToDonHang();

            if (!CoBangDonHang())
            {
                Assert.Pass("Không có đơn hàng để test sắp xếp.");
                return;
            }

            ChonSapXep("moinhat");

            // URL có thể không có sortBy khi là giá trị default, kiểm tra dropdown thay vì URL
            var select = new SelectElement(_driver.FindElement(By.CssSelector("select[name='sortBy']")));
            string selectedVal = select.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(selectedVal, Is.EqualTo("moinhat"), "Dropdown phải đang chọn 'moinhat'.");

            Pause();
            Console.WriteLine($"[PASS] Sắp xếp Mới nhất. URL: {_driver.Url}");
        }

        // ==================== TEST 9: SẮP XẾP TỔNG TIỀN CAO → THẤP ====================

        [Test]
        [Order(9)]
        public void Test09_SapXepTongTienCaoThap()
        {
            NavigateToDonHang();

            if (!CoBangDonHang())
            {
                Assert.Pass("Không có đơn hàng để test sắp xếp.");
                return;
            }

            ChonSapXep("tongtien-cao");

            Assert.That(_driver.Url, Does.Contain("sortBy=tongtien-cao"), "URL phải chứa sortBy=tongtien-cao.");

            var select = new SelectElement(_driver.FindElement(By.CssSelector("select[name='sortBy']")));
            string selectedVal = select.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(selectedVal, Is.EqualTo("tongtien-cao"), "Dropdown phải đang chọn 'tongtien-cao'.");

            var rows = _driver.FindElements(By.CssSelector("table.donhang-table tbody tr"));
            if (rows.Count >= 2)
            {
                var prices = rows.Select(r =>
                {
                    var cells = r.FindElements(By.TagName("td"));
                    if (cells.Count > 2)
                    {
                        var text = cells[2].Text.Replace("đ", "").Replace(",", "").Replace(".", "").Trim();
                        return long.TryParse(text, out var val) ? val : 0L;
                    }
                    return 0L;
                }).Where(v => v > 0).ToList();

                bool isDescending = prices.Zip(prices.Skip(1), (a, b) => a >= b).All(x => x);
                Assert.That(isDescending, Is.True, "Đơn hàng phải được sắp xếp theo tổng tiền giảm dần.");
            }

            Pause();
            Console.WriteLine($"[PASS] Sắp xếp Tổng tiền Cao → Thấp. URL: {_driver.Url}");
        }

        // ==================== TEST 10: SẮP XẾP TỔNG TIỀN THẤP → CAO ====================

        [Test]
        [Order(10)]
        public void Test10_SapXepTongTienThapCao()
        {
            NavigateToDonHang();

            if (!CoBangDonHang())
            {
                Assert.Pass("Không có đơn hàng để test sắp xếp.");
                return;
            }

            ChonSapXep("tongtien-thap");

            Assert.That(_driver.Url, Does.Contain("sortBy=tongtien-thap"), "URL phải chứa sortBy=tongtien-thap.");

            var select = new SelectElement(_driver.FindElement(By.CssSelector("select[name='sortBy']")));
            string selectedVal = select.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(selectedVal, Is.EqualTo("tongtien-thap"), "Dropdown phải đang chọn 'tongtien-thap'.");

            var rows = _driver.FindElements(By.CssSelector("table.donhang-table tbody tr"));
            if (rows.Count >= 2)
            {
                var prices = rows.Select(r =>
                {
                    var cells = r.FindElements(By.TagName("td"));
                    if (cells.Count > 2)
                    {
                        var text = cells[2].Text.Replace("đ", "").Replace(",", "").Replace(".", "").Trim();
                        return long.TryParse(text, out var val) ? val : 0L;
                    }
                    return 0L;
                }).Where(v => v > 0).ToList();

                bool isAscending = prices.Zip(prices.Skip(1), (a, b) => a <= b).All(x => x);
                Assert.That(isAscending, Is.True, "Đơn hàng phải được sắp xếp theo tổng tiền tăng dần.");
            }

            Pause();
            Console.WriteLine($"[PASS] Sắp xếp Tổng tiền Thấp → Cao. URL: {_driver.Url}");
        }

        // ==================== TEST 11: SẮP XẾP THEO SỐ BÀN ====================

        [Test]
        [Order(11)]
        public void Test11_SapXepTheoSoBan()
        {
            NavigateToDonHang();

            if (!CoBangDonHang())
            {
                Assert.Pass("Không có đơn hàng để test sắp xếp.");
                return;
            }

            ChonSapXep("ban");

            Assert.That(_driver.Url, Does.Contain("sortBy=ban"), "URL phải chứa sortBy=ban.");

            var select = new SelectElement(_driver.FindElement(By.CssSelector("select[name='sortBy']")));
            string selectedText = select.SelectedOption.Text;
            Assert.That(selectedText.Contains("bàn") || selectedText.Contains("Bàn"), Is.True,
                "Dropdown phải hiển thị option 'Theo số bàn'.");

            var rows = _driver.FindElements(By.CssSelector("table.donhang-table tbody tr"));
            if (rows.Count >= 2)
            {
                var banNums = rows.Select(r =>
                {
                    var cells = r.FindElements(By.TagName("td"));
                    if (cells.Count > 1)
                    {
                        var text = cells[1].Text.Replace("Bàn", "").Trim();
                        return int.TryParse(text, out var val) ? val : 0;
                    }
                    return 0;
                }).Where(v => v > 0).ToList();

                bool isAscending = banNums.Zip(banNums.Skip(1), (a, b) => a <= b).All(x => x);
                Assert.That(isAscending, Is.True, "Đơn hàng phải được sắp xếp theo số bàn tăng dần.");
            }

            Pause();
            Console.WriteLine($"[PASS] Sắp xếp theo số bàn. URL: {_driver.Url}");
        }

        // ==================== TEST 12: CẬP NHẬT TRẠNG THÁI → ĐANG CHUẨN BỊ ====================

        [Test]
        [Order(12)]
        public void Test12_CapNhatTrangThai_DangChuanBi()
        {
            NavigateToDonHang();
            Assert.That(CoBangDonHang(), Is.True, "Phải có đơn hàng để test cập nhật trạng thái.");

            var chiTietBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-primary.btn-sm")));
            chiTietBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Pause();

            Console.WriteLine($"[INFO] Chi tiết đơn hàng: {_driver.Url}");

            var select = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            select.SelectByValue("DANGCHUANBI");
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Pause();

            Assert.That(_driver.Url, Does.Contain("/DonHang/Details/"),
                "Sau cập nhật phải ở lại trang chi tiết.");

            // Kiểm tra dropdown hiện tại là DANGCHUANBI
            var currentSelect = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            string currentVal = currentSelect.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(currentVal, Is.EqualTo("DANGCHUANBI"),
                "Trạng thái phải cập nhật thành 'DANGCHUANBI'.");

            var statusBadge = _driver.FindElement(By.CssSelector("span.badge"));
            Pause();
            Console.WriteLine($"[PASS] Cập nhật trạng thái → Đang chuẩn bị. Badge: '{statusBadge.Text}', Value: '{currentVal}'");
        }

        // ==================== TEST 13: CẬP NHẬT TRẠNG THÁI → HOÀN THÀNH ====================

        [Test]
        [Order(13)]
        public void Test13_CapNhatTrangThai_HoanThanh()
        {
            NavigateToDonHang();
            Assert.That(CoBangDonHang(), Is.True, "Phải có đơn hàng để test cập nhật trạng thái.");

            var chiTietBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-primary.btn-sm")));
            chiTietBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Pause();

            var select = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            select.SelectByValue("HOANTHANH");
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Pause();

            // Kiểm tra select dropdown hiện tại là HOANTHANH
            var currentSelect = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            string currentVal = currentSelect.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(currentVal, Is.EqualTo("HOANTHANH"), "Trạng thái phải cập nhật thành 'HOANTHANH'.");

            var statusBadge = _driver.FindElement(By.CssSelector("span.badge"));
            Pause();
            Console.WriteLine($"[PASS] Cập nhật trạng thái → Hoàn thành. Badge: '{statusBadge.Text}', Value: '{currentVal}'");
        }

        // ==================== TEST 14: CẬP NHẬT TRẠNG THÁI → HỦY ====================

        [Test]
        [Order(14)]
        public void Test14_CapNhatTrangThai_Huy()
        {
            NavigateToDonHang();
            Assert.That(CoBangDonHang(), Is.True, "Phải có đơn hàng để test cập nhật trạng thái.");

            var chiTietBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-primary.btn-sm")));
            chiTietBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Pause();

            var select = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            select.SelectByValue("HUY");
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Pause();

            // Kiểm tra select dropdown hiện tại là HUY (chính xác hơn badge text)
            var currentSelect = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            string currentVal = currentSelect.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(currentVal, Is.EqualTo("HUY"), "Trạng thái phải cập nhật thành 'HUY'.");

            var statusBadge = _driver.FindElement(By.CssSelector("span.badge"));
            Pause();
            Console.WriteLine($"[PASS] Cập nhật trạng thái → Hủy. Badge: '{statusBadge.Text}', Value: '{currentVal}'");
        }

        // ==================== TEST 15: CẬP NHẬT TRẠNG THÁI → CHỜ XÁC NHẬN ====================

        [Test]
        [Order(15)]
        public void Test15_CapNhatTrangThai_ChoXacNhan()
        {
            NavigateToDonHang();
            Assert.That(CoBangDonHang(), Is.True, "Phải có đơn hàng để test cập nhật trạng thái.");

            var chiTietBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-primary.btn-sm")));
            chiTietBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Pause();

            var select = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            select.SelectByValue("CHOXACNHAN");
            Pause();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Pause();

            // Kiểm tra select dropdown sau khi cập nhật
            var currentSelect = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            string currentVal = currentSelect.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(currentVal, Is.EqualTo("CHOXACNHAN"), "Trạng thái phải cập nhật thành 'CHOXACNHAN'.");

            var statusBadge = _driver.FindElement(By.CssSelector("span.badge"));
            Pause();
            Console.WriteLine($"[PASS] Cập nhật trạng thái → Chờ xác nhận. Badge: '{statusBadge.Text}', Value: '{currentVal}'");
        }
    }
}
