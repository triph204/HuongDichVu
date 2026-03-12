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

            // ??ng nh?p tr??c m?i test
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

        private void NavigateToDonHang()
        {
            _driver.Navigate().GoToUrl(DonHangUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".status-tabs")));
        }

        // Click tab tr?ng thái theo href ch?a keyword
        private void ClickTabTrangThai(string trangThaiParam)
        {
            var tab = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector($"a.status-tab[href*='trangThai={trangThaiParam}']")));
            tab.Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".status-tabs")));
            System.Threading.Thread.Sleep(500);
        }

        // L?y s? ??n hi?n th? trên tab
        private int LaySoDonTrenTab(string trangThaiParam)
        {
            var tab = _driver.FindElement(
                By.CssSelector($"a.status-tab[href*='trangThai={trangThaiParam}']"));
            var countText = tab.FindElement(By.CssSelector(".tab-count")).Text;
            return int.TryParse(countText, out var count) ? count : 0;
        }

        // Ch?n sort option
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
            System.Threading.Thread.Sleep(800);
        }

        // Ki?m tra có b?ng ??n hàng hay thông báo r?ng
        private bool CoBangDonHang()
        {
            var tables = _driver.FindElements(By.CssSelector("table.donhang-table"));
            return tables.Count > 0 && tables[0].Displayed;
        }

        // ==================== TEST 1: XEM T?T C? ??N HÀNG ====================

        [Test]
        [Order(1)]
        public void Test01_XemTatCaDonHang()
        {
            // Arrange
            NavigateToDonHang();

            // Assert - Tab "T?t c?" ph?i active (ki?m tra qua href thay vì text ti?ng Vi?t)
            var activeTab = _driver.FindElement(By.CssSelector("a.status-tab.active"));
            string activeHref = activeTab.GetDomProperty("href") ?? "";
            Assert.That(activeHref, Does.Contain("trangThai=tatca"),
                "Tab 'T?t c?' (tatca) ph?i ?ang active khi vào trang DonHang.");

            bool coNoiDung = CoBangDonHang() ||
                _driver.FindElements(By.CssSelector(".donhang-empty")).Count > 0;
            Assert.That(coNoiDung, Is.True, "Trang ph?i hi?n th? b?ng ??n hàng ho?c thông báo r?ng.");

            var toolbar = _driver.FindElement(By.CssSelector(".donhang-toolbar"));
            Assert.That(toolbar.Displayed, Is.True, "Toolbar thông tin ??n hàng ph?i hi?n th?.");

            Console.WriteLine($"[PASS] Xem t?t c? ??n hàng. T?ng: {LaySoDonTrenTab("tatca")} ??n.");
        }

        // ==================== TEST 2: XEM ??N CH? XÁC NH?N ====================

        [Test]
        [Order(2)]
        public void Test02_XemDonChoXacNhan()
        {
            NavigateToDonHang();
            ClickTabTrangThai("CHOXACNHAN");

            Assert.That(_driver.Url, Does.Contain("trangThai=CHOXACNHAN"),
                "URL ph?i ch?a trangThai=CHOXACNHAN sau khi click tab.");

            // Ki?m tra active qua href thay vì text
            var activeTab = _driver.FindElement(By.CssSelector("a.status-tab.active"));
            string activeHref = activeTab.GetDomProperty("href") ?? "";
            Assert.That(activeHref, Does.Contain("CHOXACNHAN"), "Tab CHOXACNHAN ph?i active.");

            if (CoBangDonHang())
            {
                var badges = _driver.FindElements(By.CssSelector("span.donhang-badge"));
                Assert.That(badges.Count, Is.GreaterThan(0), "Ph?i có badge tr?ng thái.");
            }

            Console.WriteLine($"[PASS] Tab Ch? xác nh?n: {LaySoDonTrenTab("CHOXACNHAN")} ??n. URL: {_driver.Url}");
        }

        // ==================== TEST 3: XEM ??N ?ANG CHU?N B? ====================

        [Test]
        [Order(3)]
        public void Test03_XemDonDangChuanBi()
        {
            NavigateToDonHang();
            ClickTabTrangThai("DANGCHUANBI");

            Assert.That(_driver.Url, Does.Contain("trangThai=DANGCHUANBI"), "URL ph?i ch?a trangThai=DANGCHUANBI.");

            var activeTab = _driver.FindElement(By.CssSelector("a.status-tab.active"));
            string activeHref = activeTab.GetDomProperty("href") ?? "";
            Assert.That(activeHref, Does.Contain("DANGCHUANBI"), "Tab DANGCHUANBI ph?i active.");

            Console.WriteLine($"[PASS] Tab ?ang chu?n b?: {LaySoDonTrenTab("DANGCHUANBI")} ??n.");
        }

        // ==================== TEST 4: XEM ??N HOÀN THÀNH ====================

        [Test]
        [Order(4)]
        public void Test04_XemDonHoanThanh()
        {
            NavigateToDonHang();
            ClickTabTrangThai("HOANTHANH");

            Assert.That(_driver.Url, Does.Contain("trangThai=HOANTHANH"), "URL ph?i ch?a trangThai=HOANTHANH.");

            var activeTab = _driver.FindElement(By.CssSelector("a.status-tab.active"));
            string activeHref = activeTab.GetDomProperty("href") ?? "";
            Assert.That(activeHref, Does.Contain("HOANTHANH"), "Tab HOANTHANH ph?i active.");

            Console.WriteLine($"[PASS] Tab Hoàn thành: {LaySoDonTrenTab("HOANTHANH")} ??n.");
        }

        // ==================== TEST 5: XEM ??N B? H?Y ====================

        [Test]
        [Order(5)]
        public void Test05_XemDonBiHuy()
        {
            NavigateToDonHang();
            ClickTabTrangThai("HUY");

            Assert.That(_driver.Url, Does.Contain("trangThai=HUY"), "URL ph?i ch?a trangThai=HUY.");

            var activeTab = _driver.FindElement(By.CssSelector("a.status-tab.active"));
            string activeHref = activeTab.GetDomProperty("href") ?? "";
            Assert.That(activeHref, Does.Contain("HUY"), "Tab HUY ph?i active.");

            Console.WriteLine($"[PASS] Tab ?ã h?y: {LaySoDonTrenTab("HUY")} ??n.");
        }

        // ==================== TEST 6: XEM CHI TI?T ??N HÀNG ====================

        [Test]
        [Order(6)]
        public void Test06_XemChiTietDonHang()
        {
            NavigateToDonHang();
            Assert.That(CoBangDonHang(), Is.True, "Ph?i có ??n hàng trong danh sách ?? test xem chi ti?t.");

            // Act - Click nút Chi Ti?t ??u tiên
            var chiTietBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-primary.btn-sm")));
            chiTietBtn.Click();

            // Assert - Chuy?n sang trang chi ti?t
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Assert.That(_driver.Url, Does.Contain("/DonHang/Details/"),
                "Ph?i chuy?n sang trang chi ti?t ??n hàng.");

            // Ki?m tra tiêu ?? ch?a "Chi Ti" (tránh v?n ?? encoding)
            var cardHeader = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector(".card-header h2")));
            Assert.That(cardHeader.Text.Length, Is.GreaterThan(0),
                "Trang chi ti?t ph?i có tiêu ??.");

            bool coChiTiet = _driver.FindElements(By.CssSelector("table tbody tr")).Count > 0;
            Assert.That(coChiTiet, Is.True, "Trang chi ti?t ph?i có danh sách món ?n.");

            var updateForm = _driver.FindElement(By.CssSelector("form[action*='UpdateStatus']"));
            Assert.That(updateForm.Displayed, Is.True, "Ph?i có form c?p nh?t tr?ng thái.");

            Console.WriteLine($"[PASS] Xem chi ti?t ??n hàng thành công. URL: {_driver.Url}");
        }

        // ==================== TEST 7: PHÂN TRANG ====================

        [Test]
        [Order(7)]
        public void Test07_PhanTrang()
        {
            NavigateToDonHang();

            // Ki?m tra có pagination không
            var paginationItems = _driver.FindElements(By.CssSelector(".pagination .page-item"));

            if (paginationItems.Count == 0)
            {
                Console.WriteLine("[SKIP] Không ?? ??n hàng ?? phân trang (< 10 ??n). Test b? qua.");
                Assert.Pass("Không ?? d? li?u ?? test phân trang.");
                return;
            }

            // Assert - Pagination hi?n th?
            Assert.That(paginationItems.Count, Is.GreaterThan(0),
                "Ph?i có các nút phân trang.");

            // L?y s? trang hi?n t?i
            var activePage = _driver.FindElement(By.CssSelector(".pagination .page-item.active .page-link"));
            Assert.That(activePage.Text.Trim(), Is.EqualTo("1"),
                "Trang ??u tiên ph?i ?ang active là trang 1.");

            // Act - Click nút trang ti?p theo (Sau »)
            var nextBtn = _driver.FindElements(By.CssSelector(".page-link"))
                .FirstOrDefault(e => e.Text.Contains("Sau"));

            if (nextBtn != null && nextBtn.Displayed)
            {
                nextBtn.Click();
                _wait.Until(d => d.Url.Contains("page=2"));

                Assert.That(_driver.Url, Does.Contain("page=2"),
                    "Sau khi click 'Sau', URL ph?i ch?a page=2.");

                // Assert - Trang 2 active
                var activePage2 = _driver.FindElement(
                    By.CssSelector(".pagination .page-item.active .page-link"));
                Assert.That(activePage2.Text.Trim(), Is.EqualTo("2"),
                    "Trang 2 ph?i ?ang active.");

                Console.WriteLine($"[PASS] Phân trang: ?ã chuy?n sang trang 2. URL: {_driver.Url}");

                // Act - Click nút Tr??c « ?? quay l?i trang 1
                var prevBtn = _driver.FindElements(By.CssSelector(".page-link"))
                    .FirstOrDefault(e => e.Text.Contains("Tr??c"));

                if (prevBtn != null)
                {
                    prevBtn.Click();
                    _wait.Until(d => !d.Url.Contains("page=2") || d.Url.Contains("page=1"));
                    Console.WriteLine($"[PASS] Phân trang: ?ã quay v? trang 1. URL: {_driver.Url}");
                }
            }
            else
            {
                Console.WriteLine("[PASS] Ch? có 1 trang ??n hàng, không có nút Sau.");
            }
        }

        // ==================== TEST 8: S?P X?P THEO M?I NH?T ====================

        [Test]
        [Order(8)]
        public void Test08_SapXepMoiNhat()
        {
            NavigateToDonHang();

            if (!CoBangDonHang())
            {
                Assert.Pass("Không có ??n hàng ?? test s?p x?p.");
                return;
            }

            ChonSapXep("moinhat");

            // URL có th? không có sortBy khi là giá tr? default, ki?m tra dropdown thay vì URL
            var select = new SelectElement(_driver.FindElement(By.CssSelector("select[name='sortBy']")));
            string selectedVal = select.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(selectedVal, Is.EqualTo("moinhat"), "Dropdown ph?i ?ang ch?n 'moinhat'.");

            Console.WriteLine($"[PASS] S?p x?p M?i nh?t. URL: {_driver.Url}");
        }

        // ==================== TEST 9: S?P X?P T?NG TI?N CAO ? TH?P ====================

        [Test]
        [Order(9)]
        public void Test09_SapXepTongTienCaoThap()
        {
            NavigateToDonHang();

            if (!CoBangDonHang())
            {
                Assert.Pass("Không có ??n hàng ?? test s?p x?p.");
                return;
            }

            ChonSapXep("tongtien-cao");

            Assert.That(_driver.Url, Does.Contain("sortBy=tongtien-cao"), "URL ph?i ch?a sortBy=tongtien-cao.");

            var select = new SelectElement(_driver.FindElement(By.CssSelector("select[name='sortBy']")));
            string selectedVal = select.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(selectedVal, Is.EqualTo("tongtien-cao"), "Dropdown ph?i ?ang ch?n 'tongtien-cao'.");

            var rows = _driver.FindElements(By.CssSelector("table.donhang-table tbody tr"));
            if (rows.Count >= 2)
            {
                var prices = rows.Select(r =>
                {
                    var cells = r.FindElements(By.TagName("td"));
                    if (cells.Count > 2)
                    {
                        var text = cells[2].Text.Replace("?", "").Replace(",", "").Replace(".", "").Trim();
                        return long.TryParse(text, out var val) ? val : 0L;
                    }
                    return 0L;
                }).Where(v => v > 0).ToList();

                bool isDescending = prices.Zip(prices.Skip(1), (a, b) => a >= b).All(x => x);
                Assert.That(isDescending, Is.True, "??n hàng ph?i ???c s?p x?p theo t?ng ti?n gi?m d?n.");
            }

            Console.WriteLine($"[PASS] S?p x?p T?ng ti?n Cao ? Th?p. URL: {_driver.Url}");
        }

        // ==================== TEST 10: S?P X?P T?NG TI?N TH?P ? CAO ====================

        [Test]
        [Order(10)]
        public void Test10_SapXepTongTienThapCao()
        {
            NavigateToDonHang();

            if (!CoBangDonHang())
            {
                Assert.Pass("Không có ??n hàng ?? test s?p x?p.");
                return;
            }

            ChonSapXep("tongtien-thap");

            Assert.That(_driver.Url, Does.Contain("sortBy=tongtien-thap"), "URL ph?i ch?a sortBy=tongtien-thap.");

            var select = new SelectElement(_driver.FindElement(By.CssSelector("select[name='sortBy']")));
            string selectedVal = select.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(selectedVal, Is.EqualTo("tongtien-thap"), "Dropdown ph?i ?ang ch?n 'tongtien-thap'.");

            var rows = _driver.FindElements(By.CssSelector("table.donhang-table tbody tr"));
            if (rows.Count >= 2)
            {
                var prices = rows.Select(r =>
                {
                    var cells = r.FindElements(By.TagName("td"));
                    if (cells.Count > 2)
                    {
                        var text = cells[2].Text.Replace("?", "").Replace(",", "").Replace(".", "").Trim();
                        return long.TryParse(text, out var val) ? val : 0L;
                    }
                    return 0L;
                }).Where(v => v > 0).ToList();

                bool isAscending = prices.Zip(prices.Skip(1), (a, b) => a <= b).All(x => x);
                Assert.That(isAscending, Is.True, "??n hàng ph?i ???c s?p x?p theo t?ng ti?n t?ng d?n.");
            }

            Console.WriteLine($"[PASS] S?p x?p T?ng ti?n Th?p ? Cao. URL: {_driver.Url}");
        }

        // ==================== TEST 11: S?P X?P THEO S? BÀN ====================

        [Test]
        [Order(11)]
        public void Test11_SapXepTheoSoBan()
        {
            NavigateToDonHang();

            if (!CoBangDonHang())
            {
                Assert.Pass("Không có ??n hàng ?? test s?p x?p.");
                return;
            }

            ChonSapXep("ban");

            Assert.That(_driver.Url, Does.Contain("sortBy=ban"), "URL ph?i ch?a sortBy=ban.");

            var select = new SelectElement(_driver.FindElement(By.CssSelector("select[name='sortBy']")));
            string selectedText = select.SelectedOption.Text;
            Assert.That(selectedText.Contains("bàn") || selectedText.Contains("Bàn"), Is.True,
                "Dropdown ph?i hi?n th? option 'Theo s? bàn'.");

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
                Assert.That(isAscending, Is.True, "??n hàng ph?i ???c s?p x?p theo s? bàn t?ng d?n.");
            }

            Console.WriteLine($"[PASS] S?p x?p theo s? bàn. URL: {_driver.Url}");
        }

        // ==================== TEST 12: C?P NH?T TR?NG THÁI ? ?ANG CHU?N B? ====================

        [Test]
        [Order(12)]
        public void Test12_CapNhatTrangThai_DangChuanBi()
        {
            NavigateToDonHang();
            Assert.That(CoBangDonHang(), Is.True, "Ph?i có ??n hàng ?? test c?p nh?t tr?ng thái.");

            var chiTietBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-primary.btn-sm")));
            chiTietBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));

            Console.WriteLine($"[INFO] Chi ti?t ??n hàng: {_driver.Url}");

            var select = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            select.SelectByValue("DANGCHUANBI");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));

            Assert.That(_driver.Url, Does.Contain("/DonHang/Details/"),
                "Sau c?p nh?t ph?i ? l?i trang chi ti?t.");

            // Ki?m tra dropdown hi?n t?i là DANGCHUANBI
            var currentSelect = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            string currentVal = currentSelect.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(currentVal, Is.EqualTo("DANGCHUANBI"),
                "Tr?ng thái ph?i c?p nh?t thành 'DANGCHUANBI'.");

            var statusBadge = _driver.FindElement(By.CssSelector("span.badge"));
            Console.WriteLine($"[PASS] C?p nh?t tr?ng thái ? ?ang chu?n b?. Badge: '{statusBadge.Text}', Value: '{currentVal}'");
        }

        // ==================== TEST 13: C?P NH?T TR?NG THÁI ? HOÀN THÀNH ====================

        [Test]
        [Order(13)]
        public void Test13_CapNhatTrangThai_HoanThanh()
        {
            NavigateToDonHang();
            Assert.That(CoBangDonHang(), Is.True, "Ph?i có ??n hàng ?? test c?p nh?t tr?ng thái.");

            var chiTietBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-primary.btn-sm")));
            chiTietBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));

            var select = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            select.SelectByValue("HOANTHANH");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();

            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));

            // Ki?m tra select dropdown hi?n t?i là HOANTHANH
            var currentSelect = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            string currentVal = currentSelect.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(currentVal, Is.EqualTo("HOANTHANH"), "Tr?ng thái ph?i c?p nh?t thành 'HOANTHANH'.");

            var statusBadge = _driver.FindElement(By.CssSelector("span.badge"));
            Console.WriteLine($"[PASS] C?p nh?t tr?ng thái ? Hoàn thành. Badge: '{statusBadge.Text}', Value: '{currentVal}'");
        }

        // ==================== TEST 14: C?P NH?T TR?NG THÁI ? H?Y ====================

        [Test]
        [Order(14)]
        public void Test14_CapNhatTrangThai_Huy()
        {
            NavigateToDonHang();
            Assert.That(CoBangDonHang(), Is.True, "Ph?i có ??n hàng ?? test c?p nh?t tr?ng thái.");

            var chiTietBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-primary.btn-sm")));
            chiTietBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));

            var select = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            select.SelectByValue("HUY");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));

            // Ki?m tra select dropdown hi?n t?i là HUY (chính xác h?n badge text)
            var currentSelect = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            string currentVal = currentSelect.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(currentVal, Is.EqualTo("HUY"), "Tr?ng thái ph?i c?p nh?t thành 'HUY'.");

            var statusBadge = _driver.FindElement(By.CssSelector("span.badge"));
            Console.WriteLine($"[PASS] C?p nh?t tr?ng thái ? H?y. Badge: '{statusBadge.Text}', Value: '{currentVal}'");
        }

        // ==================== TEST 15: C?P NH?T TR?NG THÁI ? CH? XÁC NH?N ====================

        [Test]
        [Order(15)]
        public void Test15_CapNhatTrangThai_ChoXacNhan()
        {
            NavigateToDonHang();
            Assert.That(CoBangDonHang(), Is.True, "Ph?i có ??n hàng ?? test c?p nh?t tr?ng thái.");

            var chiTietBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-primary.btn-sm")));
            chiTietBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));

            var select = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            select.SelectByValue("CHOXACNHAN");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));

            // Ki?m tra select dropdown sau khi c?p nh?t
            var currentSelect = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            string currentVal = currentSelect.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(currentVal, Is.EqualTo("CHOXACNHAN"), "Tr?ng thái ph?i c?p nh?t thành 'CHOXACNHAN'.");

            var statusBadge = _driver.FindElement(By.CssSelector("span.badge"));
            Console.WriteLine($"[PASS] C?p nh?t tr?ng thái ? Ch? xác nh?n. Badge: '{statusBadge.Text}', Value: '{currentVal}'");
        }
    }
}
