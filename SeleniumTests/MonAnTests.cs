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

        // Du lieu test (dung ASCII tranh loi encoding)
        private const string TenMonMoi  = "TEST_MON_Selenium";
        private const string TenMonSua  = "TEST_MON_Selenium_EDITED";
        private const string MoTaMoi    = "Mo ta mon an tu dong tao boi Selenium";
        private const string MoTaSua    = "Mo ta mon an da duoc chinh sua boi Selenium";
        private const long   GiaMoi     = 99000;
        private const long   GiaSua     = 149000;

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

        private void NavigateToMonAn()
        {
            _driver.Navigate().GoToUrl(MonAnUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
        }

        // Tim dong trong bang theo ten mon
        private IWebElement? TimDongTheoTen(string tenMon)
        {
            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            return rows.FirstOrDefault(r =>
                r.FindElements(By.TagName("td")).Count > 0 &&
                r.FindElements(By.TagName("td"))[0].Text.Trim() == tenMon);
        }

        // Xoa mon an theo ten neu ton tai
        private void XoaNeuTonTai(string tenMon)
        {
            NavigateToMonAn();
            // Tim tren tat ca cac trang (toi da 10 trang)
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
                    System.Threading.Thread.Sleep(300);
                    return;
                }

                // Sang trang tiep theo neu co
                var nextBtn = _driver.FindElements(By.CssSelector(".pagination .page-link"))
                    .FirstOrDefault(e => e.Text.Contains("Sau"));
                if (nextBtn == null || !nextBtn.Displayed) break;
                nextBtn.Click();
                _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("table")));
            }
        }

        // Lay danh muc dau tien trong select trang Create/Edit
        private string LayDanhMucDauTien()
        {
            var options = _driver.FindElements(By.CssSelector("select[name='DanhMucId'] option"))
                .Where(o => !string.IsNullOrWhiteSpace(o.GetDomProperty("value")) &&
                            o.GetDomProperty("value") != "0" &&
                            o.GetDomProperty("value") != "")
                .ToList();
            return options.Count > 0 ? (options[0].GetDomProperty("value") ?? "") : "";
        }

        // Helper tao mon an moi va cho redirect
        private void TaoMonAn(string ten, long gia, string moTa)
        {
            _driver.Navigate().GoToUrl(MonAnUrl + "/Create");
            _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));

            _driver.FindElement(By.CssSelector("input[name='TenMon']")).Clear();
            _driver.FindElement(By.CssSelector("input[name='TenMon']")).SendKeys(ten);

            // Chon danh muc dau tien hop le
            string dmId = LayDanhMucDauTien();
            if (!string.IsNullOrEmpty(dmId))
            {
                new SelectElement(_driver.FindElement(
                    By.CssSelector("select[name='DanhMucId']"))).SelectByValue(dmId);
            }

            _driver.FindElement(By.CssSelector("input[name='Gia']")).Clear();
            _driver.FindElement(By.CssSelector("input[name='Gia']")).SendKeys(gia.ToString());

            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).Clear();
            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).SendKeys(moTa);

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            NavigateToMonAn();
        }

        // Tim mon an tren tat ca cac trang
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

        // ==================== TEST 1: XEM DANH SACH MON AN ====================

        [Test]
        [Order(1)]
        public void Test01_XemDanhSachMonAn()
        {
            NavigateToMonAn();

            // Assert - URL dung
            Assert.That(_driver.Url, Does.Contain("/MonAn"),
                "Phai dieu huong den trang /MonAn.");

            // Assert - Co nut Them Mon
            var themBtn = _driver.FindElement(By.CssSelector("a[href='/MonAn/Create']"));
            Assert.That(themBtn.Displayed, Is.True, "Phai co nut 'Them Mon'.");

            // Assert - Co filter danh muc
            var filterSelect = _driver.FindElement(By.CssSelector("select#categoryFilter"));
            Assert.That(filterSelect.Displayed, Is.True, "Phai co dropdown loc theo danh muc.");

            // Assert - Co bang hoac thong bao rong
            bool coNoiDung =
                _driver.FindElements(By.CssSelector("table tbody tr")).Count > 0 ||
                _driver.FindElements(By.CssSelector(".no-data")).Count > 0;
            Assert.That(coNoiDung, Is.True, "Trang phai hien thi bang mon an hoac thong bao rong.");

            // Assert - Bang co 5 cot: Ten Mon, Danh Muc, Gia, Co San, Hanh Dong
            var headers = _driver.FindElements(By.CssSelector("table thead tr th"));
            Assert.That(headers.Count, Is.EqualTo(5), "Bang mon an phai co 5 cot.");

            // Assert - Hien thi thong tin tong so mon
            var infoText = _driver.FindElement(By.CssSelector(".card-body"));
            Assert.That(infoText.Text.Length, Is.GreaterThan(0), "Phai hien thi thong tin tong so mon an.");

            Console.WriteLine($"[PASS] Xem danh sach mon an thanh cong. URL: {_driver.Url}");
        }

        // ==================== TEST 2: LOC THEO DANH MUC ====================

        [Test]
        [Order(2)]
        public void Test02_LocTheoanhMuc()
        {
            NavigateToMonAn();

            // Lay danh sach option trong filter (bo qua "Tat ca")
            var options = _driver.FindElements(By.CssSelector("select#categoryFilter option"))
                .Where(o =>
                {
                    var val = o.GetDomProperty("value") ?? "";
                    return val != "0" && val != "";
                }).ToList();

            if (options.Count == 0)
            {
                Assert.Pass("Khong co danh muc nao de test loc.");
                return;
            }

            // Chon danh muc dau tien
            string dmId    = options[0].GetDomProperty("value") ?? "";
            string dmLabel = options[0].Text.Trim();

            new SelectElement(_driver.FindElement(
                By.CssSelector("select#categoryFilter"))).SelectByValue(dmId);

            // Cho trang reload (onchange tu dong submit form)
            _wait.Until(d => d.Url.Contains($"categoryId={dmId}"));

            // Assert - URL chua categoryId
            Assert.That(_driver.Url, Does.Contain($"categoryId={dmId}"),
                "URL phai chua categoryId sau khi loc.");

            // Assert - Thong tin hien thi co ten danh muc
            var infoSection = _driver.FindElement(By.CssSelector(".card-body"));
            Assert.That(infoSection.Text, Does.Contain(dmLabel),
                "Trang phai hien thi ten danh muc dang loc.");

            // Assert - Neu co mon an, tat ca phai thuoc danh muc da chon
            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count > 0)
            {
                bool tatCaDungDanhMuc = rows.All(r =>
                {
                    var cells = r.FindElements(By.TagName("td"));
                    return cells.Count > 1 && cells[1].Text.Trim() == dmLabel;
                });
                Assert.That(tatCaDungDanhMuc, Is.True,
                    "Tat ca mon an hien thi phai thuoc danh muc da chon.");
            }

            Console.WriteLine($"[PASS] Loc theo danh muc '{dmLabel}' (ID={dmId}). URL: {_driver.Url}");
        }

        // ==================== TEST 3: XOA LOC DANH MUC ====================

        [Test]
        [Order(3)]
        public void Test03_XoaLocDanhMuc()
        {
            NavigateToMonAn();

            // Chon danh muc bat ky de loc
            var options = _driver.FindElements(By.CssSelector("select#categoryFilter option"))
                .Where(o =>
                {
                    var val = o.GetDomProperty("value") ?? "";
                    return val != "0" && val != "";
                }).ToList();

            if (options.Count == 0)
            {
                Assert.Pass("Khong co danh muc de test xoa loc.");
                return;
            }

            string dmId = options[0].GetDomProperty("value") ?? "";
            new SelectElement(_driver.FindElement(
                By.CssSelector("select#categoryFilter"))).SelectByValue(dmId);
            _wait.Until(d => d.Url.Contains($"categoryId={dmId}"));

            // Assert - Co nut "Xoa loc"
            var xoaLocBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light.btn-sm[href='/MonAn']")));
            Assert.That(xoaLocBtn.Displayed, Is.True, "Phai co nut 'Xoa loc' khi dang loc.");

            // Act - Click Xoa loc
            xoaLocBtn.Click();
            _wait.Until(d => !d.Url.Contains("categoryId=") || d.Url.Contains("categoryId=0"));

            // Assert - Ve tat ca mon an
            Assert.That(_driver.Url, Does.Not.Contain($"categoryId={dmId}"),
                "Sau khi xoa loc, URL khong con categoryId cu.");

            var filterSelect = new SelectElement(
                _driver.FindElement(By.CssSelector("select#categoryFilter")));
            string selectedVal = filterSelect.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(selectedVal, Is.EqualTo("0"), "Dropdown filter phai tro ve 'Tat ca danh muc'.");

            Console.WriteLine($"[PASS] Xoa loc danh muc thanh cong. URL: {_driver.Url}");
        }

        // ==================== TEST 4: PHAN TRANG ====================

        [Test]
        [Order(4)]
        public void Test04_PhanTrang()
        {
            NavigateToMonAn();

            var paginationItems = _driver.FindElements(
                By.CssSelector(".pagination .page-item, .pagination li"));

            if (paginationItems.Count == 0)
            {
                Console.WriteLine("[SKIP] Khong du mon an de phan trang (< 12). Test bo qua.");
                Assert.Pass("Khong du du lieu de test phan trang.");
                return;
            }

            // Assert - Trang 1 dang active
            var activePage = _driver.FindElement(
                By.CssSelector(".pagination .page-item.active .page-link, .pagination li.active a"));
            Assert.That(activePage.Text.Trim(), Is.EqualTo("1"),
                "Trang dau tien phai dang active la trang 1.");

            // Act - Click nut Sau
            var nextBtn = _driver.FindElements(By.CssSelector(".pagination .page-link"))
                .FirstOrDefault(e => e.Text.Contains("Sau"));

            if (nextBtn != null && nextBtn.Displayed)
            {
                nextBtn.Click();
                _wait.Until(d => d.Url.Contains("page=2"));

                Assert.That(_driver.Url, Does.Contain("page=2"),
                    "Sau khi click 'Sau', URL phai chua page=2.");

                var activePage2 = _driver.FindElement(
                    By.CssSelector(".pagination .page-item.active .page-link, .pagination li.active a"));
                Assert.That(activePage2.Text.Trim(), Is.EqualTo("2"),
                    "Trang 2 phai dang active.");

                Console.WriteLine($"[PASS] Phan trang: Da chuyen sang trang 2. URL: {_driver.Url}");

                // Act - Click nut Truoc de quay lai trang 1
                var prevBtn = _driver.FindElements(By.CssSelector(".pagination .page-link"))
                    .FirstOrDefault(e => e.Text.Contains("Sau") == false && e.Text.Contains("1") == false &&
                                        (e.Text.Contains("Tr") || e.Text.Contains("chevron-left") ||
                                         e.FindElements(By.CssSelector(".fa-chevron-left")).Count > 0));

                // Tim lai nut Truoc don gian hon
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
                    Console.WriteLine($"[PASS] Phan trang: Da quay ve trang 1. URL: {_driver.Url}");
                }
            }
            else
            {
                Console.WriteLine("[PASS] Chi co 1 trang mon an, khong co nut Sau.");
            }
        }

        // ==================== TEST 5: THEM MON AN THANH CONG ====================

        [Test]
        [Order(5)]
        public void Test05_ThemMonAn_ThanhCong()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);

            NavigateToMonAn();

            // Act - Click nut Them Mon
            var themBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/MonAn/Create']")));
            themBtn.Click();

            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Create"));
            Assert.That(_driver.Url, Does.Contain("/MonAn/Create"),
                "Phai chuyen sang trang tao mon an.");

            // Dien form
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));
            tenInput.Clear();
            tenInput.SendKeys(TenMonMoi);

            string dmId = LayDanhMucDauTien();
            Assert.That(dmId, Is.Not.Empty, "Phai co danh muc de chon khi tao mon an.");
            new SelectElement(_driver.FindElement(
                By.CssSelector("select[name='DanhMucId']"))).SelectByValue(dmId);

            var giaInput = _driver.FindElement(By.CssSelector("input[name='Gia']"));
            giaInput.Clear();
            giaInput.SendKeys(GiaMoi.ToString());

            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.Clear();
            moTaInput.SendKeys(MoTaMoi);

            // Click Luu
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));

            // Reload ve danh sach
            NavigateToMonAn();

            // Assert - Mon an moi xuat hien
            var dongMoi = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongMoi, Is.Not.Null,
                $"Mon an '{TenMonMoi}' phai xuat hien trong bang sau khi them.");

            // Assert - Gia hien thi dung (chi kiem tra co hien thi gia, khong parse)
            var cells = dongMoi!.FindElements(By.TagName("td"));
            string giaCell = cells[2].Text.Trim();
            Assert.That(giaCell.Length, Is.GreaterThan(0),
                "Cot Gia phai co gia tri hien thi.");
            // Kiem tra so trong gia phai khop GiaMoi
            string giaDigits = new string(giaCell.Where(char.IsDigit).ToArray());
            Assert.That(giaDigits, Does.Contain(GiaMoi.ToString()),
                $"Gia mon an phai chua '{GiaMoi}', hien thi: '{giaCell}'");

            Console.WriteLine($"[PASS] Them mon an thanh cong: '{TenMonMoi}', gia: {giaCell}");
        }

        // ==================== TEST 6: THEM MON AN BO TRONG TEN ====================

        [Test]
        [Order(6)]
        public void Test06_ThemMonAn_BoTrongTen()
        {
            NavigateToMonAn();

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/MonAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Create"));

            // Chi nhap gia, khong nhap ten
            string dmId = LayDanhMucDauTien();
            if (!string.IsNullOrEmpty(dmId))
                new SelectElement(_driver.FindElement(
                    By.CssSelector("select[name='DanhMucId']"))).SelectByValue(dmId);

            var giaInput = _driver.FindElement(By.CssSelector("input[name='Gia']"));
            giaInput.Clear();
            giaInput.SendKeys("50000");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            System.Threading.Thread.Sleep(1000);

            // Assert - HTML5 required chan submit
            var tenInput = _driver.FindElement(By.CssSelector("input[name='TenMon']"));
            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;", tenInput);
            Assert.That(isInvalid, Is.True,
                "Input TenMon phai bi HTML5 validation khi bo trong.");

            Assert.That(_driver.Url, Does.Contain("/Create"),
                "Khi bo trong ten phai o lai trang Create.");

            Console.WriteLine("[PASS] Bo trong ten mon an: Bi chan boi HTML5 required.");
        }

        // ==================== TEST 7: THEM MON AN BO TRONG DANH MUC ====================

        [Test]
        [Order(7)]
        public void Test07_ThemMonAn_BoTrongDanhMuc()
        {
            NavigateToMonAn();

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/MonAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Create"));

            // Chi nhap ten va gia, khong chon danh muc
            _driver.FindElement(By.CssSelector("input[name='TenMon']")).SendKeys("Mon khong co danh muc");
            _driver.FindElement(By.CssSelector("input[name='Gia']")).SendKeys("50000");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            System.Threading.Thread.Sleep(1000);

            // Assert - HTML5 required chan submit
            var dmSelect = _driver.FindElement(By.CssSelector("select[name='DanhMucId']"));
            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;", dmSelect);
            Assert.That(isInvalid, Is.True,
                "Select DanhMucId phai bi HTML5 validation khi chua chon.");

            Assert.That(_driver.Url, Does.Contain("/Create"),
                "Khi bo trong danh muc phai o lai trang Create.");

            Console.WriteLine("[PASS] Bo trong danh muc: Bi chan boi HTML5 required.");
        }

        // ==================== TEST 8: XEM TRANG SUA MON AN ====================

        [Test]
        [Order(8)]
        public void Test08_XemTrangSuaMonAn()
        {
            NavigateToMonAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phai co mon an de test trang sua.");

            // Click nut Sua dau tien
            var suaBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-primary.btn-sm")));
            suaBtn.Click();

            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));
            Assert.That(_driver.Url, Does.Contain("/MonAn/Edit/"),
                "Phai chuyen sang trang sua mon an.");

            // Assert - Form co cac truong can thiet
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));
            Assert.That(tenInput.Displayed, Is.True, "Phai co o nhap TenMon.");

            var dmSelect = _driver.FindElement(By.CssSelector("select[name='DanhMucId']"));
            Assert.That(dmSelect.Displayed, Is.True, "Phai co select DanhMucId.");

            var giaInput = _driver.FindElement(By.CssSelector("input[name='Gia']"));
            Assert.That(giaInput.Displayed, Is.True, "Phai co o nhap Gia.");

            var coSanCheck = _driver.FindElement(By.CssSelector("input[name='CoSan']"));
            Assert.That(coSanCheck.Displayed, Is.True, "Phai co checkbox CoSan.");

            // Assert - Du lieu da duoc load san tu DB
            string tenHienTai = tenInput.GetDomProperty("value") ?? "";
            Assert.That(tenHienTai.Length, Is.GreaterThan(0),
                "TenMon phai co gia tri san tu DB.");

            string giaHienTai = giaInput.GetDomProperty("value") ?? "";
            Assert.That(giaHienTai.Length, Is.GreaterThan(0),
                "Gia phai co gia tri san tu DB.");

            // Assert - Co nut Cap Nhat va Huy
            Assert.That(_driver.FindElement(
                By.CssSelector("button[type='submit'].btn-success")).Displayed, Is.True,
                "Phai co nut 'Cap Nhat'.");
            Assert.That(_driver.FindElement(
                By.CssSelector("a.btn-light[href='/MonAn']")).Displayed, Is.True,
                "Phai co nut 'Huy'.");

            Console.WriteLine($"[PASS] Xem trang sua mon an. URL: {_driver.Url}, Ten: '{tenHienTai}'");
        }

        // ==================== TEST 9: SUA MON AN THANH CONG ====================

        [Test]
        [Order(9)]
        public void Test09_SuaMonAn_ThanhCong()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            // Tim va click nut Sua cua mon test
            var dongTest = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongTest, Is.Not.Null, $"Phai tim thay mon an '{TenMonMoi}' de sua.");

            dongTest!.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));

            // Act - Cap nhat ten va gia
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));
            tenInput.Clear();
            tenInput.SendKeys(TenMonSua);

            var giaInput = _driver.FindElement(By.CssSelector("input[name='Gia']"));
            giaInput.Clear();
            giaInput.SendKeys(GiaSua.ToString());

            var moTaInput = _driver.FindElement(By.CssSelector("textarea[name='MoTa']"));
            moTaInput.Clear();
            moTaInput.SendKeys(MoTaSua);

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            NavigateToMonAn();

            Assert.That(_driver.Url, Does.Not.Contain("/Edit/"),
                "Sau khi cap nhat phai redirect ve trang danh sach.");

            // Assert - Mon an voi ten moi xuat hien
            var dongSua = TimMonAnTrenTatCaTrang(TenMonSua);
            Assert.That(dongSua, Is.Not.Null,
                $"Mon an '{TenMonSua}' phai xuat hien sau khi sua.");

            // Assert - Gia moi dung
            var cells = dongSua!.FindElements(By.TagName("td"));
            string giaCell = cells[2].Text.Trim();
            string giaDigits = new string(giaCell.Where(char.IsDigit).ToArray());
            Assert.That(giaDigits, Does.Contain(GiaSua.ToString()),
                $"Gia phai duoc cap nhat thanh {GiaSua}, hien thi: '{giaCell}'");

            // Assert - Ten cu khong con
            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null,
                $"Ten cu '{TenMonMoi}' khong duoc ton tai sau khi sua.");

            Console.WriteLine($"[PASS] Sua mon an thanh cong: '{TenMonMoi}' -> '{TenMonSua}', gia: {GiaSua}");
        }

        // ==================== TEST 10: SUA MON AN BO TRONG TEN ====================

        [Test]
        [Order(10)]
        public void Test10_SuaMonAn_BoTrongTen()
        {
            NavigateToMonAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phai co mon an de test sua voi ten rong.");

            var suaBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-primary.btn-sm")));
            suaBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));

            // Xoa trang ten roi submit
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));
            tenInput.Clear();

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            System.Threading.Thread.Sleep(1000);

            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;",
                    _driver.FindElement(By.CssSelector("input[name='TenMon']")));
            Assert.That(isInvalid, Is.True,
                "TenMon phai bi HTML5 validation khi bo trong luc sua.");

            Assert.That(_driver.Url, Does.Contain("/Edit/"),
                "Khi bo trong ten luc sua phai o lai trang Edit.");

            Console.WriteLine("[PASS] Sua mon an voi ten rong: Bi chan boi HTML5 required.");
        }

        // ==================== TEST 11: CAP NHAT CO SAN ====================

        [Test]
        [Order(11)]
        public void Test11_CapNhat_CoSan()
        {
            XoaNeuTonTai(TenMonMoi);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            var dongTest = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongTest, Is.Not.Null, $"Phai tim thay mon an '{TenMonMoi}'.");

            dongTest!.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));

            // Kiem tra trang thai checkbox CoSan hien tai
            var coSanCheck = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='CoSan']")));
            bool isCheckedBefore = coSanCheck.Selected;

            // Toggle trang thai CoSan
            coSanCheck.Click();
            bool isCheckedAfter = coSanCheck.Selected;
            Assert.That(isCheckedAfter, Is.Not.EqualTo(isCheckedBefore),
                "Trang thai CoSan phai thay doi sau khi click.");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            NavigateToMonAn();

            // Assert - Badge CoSan thay doi tuong ung
            var dongSauSua = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongSauSua, Is.Not.Null, "Mon an phai con sau khi cap nhat CoSan.");

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
            Console.WriteLine($"[PASS] Cap nhat CoSan: {isCheckedBefore} -> {isCheckedAfter}, badge class: '{badgeClass}', text: '{badgeText}'");
        }

        // ==================== TEST 12: NUT HUY TRANG TAO ====================

        [Test]
        [Order(12)]
        public void Test12_NutHuy_TrangTao()
        {
            NavigateToMonAn();

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/MonAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Create"));

            _driver.FindElement(By.CssSelector("input[name='TenMon']")).SendKeys("Ten se bi huy");

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/MonAn']")));
            huyBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Create"));
            Assert.That(_driver.Url, Does.Contain("/MonAn"),
                "Sau khi click Huy phai ve trang danh sach mon an.");
            Assert.That(_driver.Url, Does.Not.Contain("/Create"),
                "Khong duoc o lai trang Create sau khi click Huy.");

            Assert.That(TimDongTheoTen("Ten se bi huy"), Is.Null,
                "Mon an khong duoc luu khi bam Huy.");

            Console.WriteLine($"[PASS] Nut Huy trang Tao: Ve lai danh sach. URL: {_driver.Url}");
        }

        // ==================== TEST 13: NUT HUY TRANG SUA ====================

        [Test]
        [Order(13)]
        public void Test13_NutHuy_TrangSua()
        {
            NavigateToMonAn();

            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count,
                Is.GreaterThan(0), "Phai co mon an de test nut Huy trang Sua.");

            // Lay ten hien tai cua mon dau tien
            var firstRow = _driver.FindElement(By.CssSelector("table tbody tr"));
            string tenGoc = firstRow.FindElements(By.TagName("td"))[0].Text.Trim();

            firstRow.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));

            // Nhap ten moi nhung KHONG submit
            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='TenMon']")));
            tenInput.Clear();
            tenInput.SendKeys("Ten tam thoi se khong luu");

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/MonAn']")));
            huyBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Assert.That(_driver.Url, Does.Contain("/MonAn"),
                "Sau khi click Huy phai ve trang danh sach.");
            Assert.That(_driver.Url, Does.Not.Contain("/Edit/"),
                "Khong duoc o lai trang Edit sau khi click Huy.");

            // Assert - Ten goc van con
            var dongGoc = TimDongTheoTen(tenGoc);
            Assert.That(dongGoc, Is.Not.Null,
                $"Ten goc '{tenGoc}' phai van con sau khi click Huy.");

            Console.WriteLine($"[PASS] Nut Huy trang Sua: Ten goc '{tenGoc}' van duoc giu nguyen.");
        }

        // ==================== TEST 14: XOA MON AN THANH CONG ====================

        [Test]
        [Order(14)]
        public void Test14_XoaMonAn_ThanhCong()
        {
            XoaNeuTonTai(TenMonMoi);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            var dongCanXoa = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Mon an '{TenMonMoi}' phai ton tai truoc khi xoa.");

            // Dem so dong truoc khi xoa (tren trang hien tai)
            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            // Act - Click nut Xoa (bo qua confirm bang JS)
            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].removeAttribute('onclick');", xoaBtn);
            xoaBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Delete/"));
            NavigateToMonAn();

            // Assert - Mon an da bi xoa
            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null,
                $"Mon an '{TenMonMoi}' phai bi xoa khoi bang.");

            Console.WriteLine($"[PASS] Xoa mon an thanh cong: '{TenMonMoi}'");
        }

        // ==================== TEST 15: XAC NHAN DIALOG KHI XOA ====================

        [Test]
        [Order(15)]
        public void Test15_XoaMonAn_XacNhanDialog()
        {
            XoaNeuTonTai(TenMonMoi);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            var dongCanXoa = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongCanXoa, Is.Not.Null,
                $"Mon an '{TenMonMoi}' phai ton tai de test confirm dialog.");

            // Inject JS tu dong confirm = true
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("window.confirm = function(){ return true; };");

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            xoaBtn.Click();

            _wait.Until(d => !d.Url.Contains("/Delete/"));
            NavigateToMonAn();

            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null,
                "Khi xac nhan 'OK' trong dialog, mon an phai bi xoa.");

            Console.WriteLine("[PASS] Xac nhan dialog xoa: Mon an da bi xoa sau khi bam OK.");
        }

        // ==================== TEST 16: DON DEP DU LIEU TEST ====================

        [Test]
        [Order(16)]
        public void Test16_DonDep_XoaDuLieuTest()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);

            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null,
                $"'{TenMonMoi}' phai duoc don dep.");
            Assert.That(TimMonAnTrenTatCaTrang(TenMonSua), Is.Null,
                $"'{TenMonSua}' phai duoc don dep.");

            Console.WriteLine("[PASS] Don dep du lieu test mon an thanh cong.");
        }
    }
}
