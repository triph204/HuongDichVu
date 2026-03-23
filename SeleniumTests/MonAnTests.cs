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
        private const string TenMonMoi     = "TEST_MON_Selenium";
        private const string TenMonSua     = "TEST_MON_Selenium_EDITED";
        private const string MoTaMoi       = "Mo ta mon an tu dong tao boi Selenium";
        private const string MoTaSua       = "Mo ta mon an da chinh sua boi Selenium";
        private const long   GiaMoi        = 99000;
        private const long   GiaSua        = 149000;
        private const int    Short         = 500;
        private const int    Medium        = 1000;

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

        private void Sleep(int ms) => System.Threading.Thread.Sleep(ms);

        private void DangNhap()
        {
            _driver.Navigate().GoToUrl(LoginUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Name("username")));
            Sleep(Short);
            _driver.FindElement(By.Name("username")).SendKeys(AdminUsername);
            Sleep(Short);
            _driver.FindElement(By.Name("password")).SendKeys(AdminPassword);
            Sleep(Short);
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang"));
            Sleep(Short);
        }

        private void NavigateToMonAn()
        {
            _driver.Navigate().GoToUrl(MonAnUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
            Sleep(Short);
        }

        private IWebElement? TimDongTheoTen(string tenMon)
        {
            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            return rows.FirstOrDefault(r =>
            {
                var cells = r.FindElements(By.TagName("td"));
                return cells.Count > 0 && cells[0].Text.Trim() == tenMon;
            });
        }

        private IWebElement? TimMonAnTrenTatCaTrang(string tenMon)
        {
            NavigateToMonAn();
            for (int i = 0; i < 10; i++)
            {
                var row = TimDongTheoTen(tenMon);
                if (row != null) return row;
                var next = _driver.FindElements(By.CssSelector(".pagination .page-link"))
                    .FirstOrDefault(e => e.Text.Contains("Sau"));
                if (next == null || !next.Displayed) break;
                next.Click();
                _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("table")));
            }
            return null;
        }

        private void XoaNeuTonTai(string tenMon)
        {
            NavigateToMonAn();
            for (int i = 0; i < 10; i++)
            {
                var row = TimDongTheoTen(tenMon);
                if (row != null)
                {
                    var xoaBtn = row.FindElement(By.CssSelector("a.btn-danger"));
                    ((IJavaScriptExecutor)_driver).ExecuteScript(
                        "arguments[0].removeAttribute('onclick');", xoaBtn);
                    xoaBtn.Click();
                    _wait.Until(d => !d.Url.Contains("/Delete/") && d.Url.Contains("/MonAn"));
                    _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
                    Sleep(Short);
                    return;
                }
                var next = _driver.FindElements(By.CssSelector(".pagination .page-link"))
                    .FirstOrDefault(e => e.Text.Contains("Sau"));
                if (next == null || !next.Displayed) break;
                next.Click();
                _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("table")));
            }
        }

        private string LayDanhMucDauTien()
        {
            var opts = _driver.FindElements(By.CssSelector("select[name='DanhMucId'] option"))
                .Where(o => { string v = o.GetDomProperty("value") ?? ""; return v != "" && v != "0"; })
                .ToList();
            return opts.Count > 0 ? (opts[0].GetDomProperty("value") ?? "") : "";
        }

        private void TaoMonAn(string ten, long gia, string moTa)
        {
            _driver.Navigate().GoToUrl(MonAnUrl + "/Create");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='TenMon']")));
            Sleep(Short);

            _driver.FindElement(By.CssSelector("input[name='TenMon']")).Clear();
            _driver.FindElement(By.CssSelector("input[name='TenMon']")).SendKeys(ten);
            Sleep(Short);

            string dmId = LayDanhMucDauTien();
            if (!string.IsNullOrEmpty(dmId))
                new SelectElement(_driver.FindElement(By.CssSelector("select[name='DanhMucId']"))).SelectByValue(dmId);
            Sleep(Short);

            _driver.FindElement(By.CssSelector("input[name='Gia']")).Clear();
            _driver.FindElement(By.CssSelector("input[name='Gia']")).SendKeys(gia.ToString());
            Sleep(Short);

            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).Clear();
            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).SendKeys(moTa);
            Sleep(Short);

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Sleep(Short);
        }

        // ==================== NHOM 1: THEM MON AN ====================

        [Test]
        [Order(1)]
        public void Test01_ThemMonAn_ThanhCong()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);
            NavigateToMonAn();

            var themBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/MonAn/Create']")));
            Assert.That(themBtn.Displayed, Is.True);
            themBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Create"));
            Sleep(Short);

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='TenMon']")));
            Assert.That(tenInput.Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("select[name='DanhMucId']")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("input[name='Gia']")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("input[name='CoSan']")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("a.btn-light[href='/MonAn']")).Displayed, Is.True);

            tenInput.Clear();
            tenInput.SendKeys(TenMonMoi);
            Sleep(Short);

            string dmId = LayDanhMucDauTien();
            Assert.That(dmId, Is.Not.Empty, "Phai co danh muc de chon.");
            new SelectElement(_driver.FindElement(By.CssSelector("select[name='DanhMucId']"))).SelectByValue(dmId);
            Sleep(Short);

            var giaInput = _driver.FindElement(By.CssSelector("input[name='Gia']"));
            giaInput.Clear();
            giaInput.SendKeys(GiaMoi.ToString());
            Sleep(Short);

            _driver.FindElement(By.CssSelector("textarea[name='MoTa']")).SendKeys(MoTaMoi);
            Sleep(Short);

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Sleep(Short);

            var dongMoi = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongMoi, Is.Not.Null, $"Mon an '{TenMonMoi}' phai xuat hien sau khi them.");

            string giaDigits = new string(dongMoi!.FindElements(By.TagName("td"))[2].Text.Where(char.IsDigit).ToArray());
            Assert.That(giaDigits, Does.Contain(GiaMoi.ToString()));

            Sleep(Short);
            Console.WriteLine($"[PASS] Them mon an thanh cong: '{TenMonMoi}'");
        }

        // ==================== NHOM 2: THEM MON AN > HUY ====================

        [Test]
        [Order(2)]
        public void Test02_ThemMonAn_NutHuy()
        {
            NavigateToMonAn();
            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/MonAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Create"));
            Sleep(Short);

            _driver.FindElement(By.CssSelector("input[name='TenMon']")).SendKeys("Ten se bi huy khong luu");
            Sleep(Short);
            _driver.FindElement(By.CssSelector("input[name='Gia']")).SendKeys("50000");
            Sleep(Short);

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/MonAn']")));
            huyBtn.Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Sleep(Short);

            Assert.That(_driver.Url, Does.Contain("/MonAn"));
            Assert.That(_driver.Url, Does.Not.Contain("/Create"));
            Assert.That(TimDongTheoTen("Ten se bi huy khong luu"), Is.Null);

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc));

            Sleep(Short);
            Console.WriteLine("[PASS] Nut Huy trang Them: Ve danh sach, khong luu du lieu.");
        }

        // ==================== NHOM 3: LOC THEO DANH MUC ====================

        [Test]
        [Order(3)]
        public void Test03_LocTheoDanhMuc()
        {
            NavigateToMonAn();

            var filterSelect = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("select#categoryFilter")));
            Assert.That(filterSelect.Displayed, Is.True);

            var options = _driver.FindElements(By.CssSelector("select#categoryFilter option"))
                .Where(o => { string v = o.GetDomProperty("value") ?? ""; return v != "0" && v != ""; })
                .ToList();

            if (options.Count == 0) { Assert.Pass("Khong co danh muc de test loc."); return; }

            string dmId    = options[0].GetDomProperty("value") ?? "";
            string dmLabel = options[0].Text.Trim();

            new SelectElement(filterSelect).SelectByValue(dmId);
            Sleep(Short);
            _wait.Until(d => d.Url.Contains($"categoryId={dmId}"));

            Assert.That(_driver.Url, Does.Contain($"categoryId={dmId}"));

            var xoaLocBtn = _driver.FindElements(By.CssSelector("a.btn-light.btn-sm[href='/MonAn']"));
            Assert.That(xoaLocBtn.Count, Is.GreaterThan(0), "Phai co nut 'Xoa loc'.");

            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count > 0)
            {
                bool tatCaDung = rows.All(r =>
                {
                    var cells = r.FindElements(By.TagName("td"));
                    return cells.Count > 1 && cells[1].Text.Trim() == dmLabel;
                });
                Assert.That(tatCaDung, Is.True, "Tat ca mon an phai thuoc danh muc da chon.");
            }

            new SelectElement(_driver.FindElement(By.CssSelector("select#categoryFilter"))).SelectByValue("0");
            Sleep(Short);

            var xoaLocSau = _driver.FindElements(By.CssSelector("a.btn-light.btn-sm[href='/MonAn']"));
            Assert.That(xoaLocSau.Count, Is.EqualTo(0), "Nut 'Xoa loc' phai bien mat.");

            Sleep(Short);
            Console.WriteLine($"[PASS] Loc theo danh muc '{dmLabel}' thanh cong.");
        }

        // ==================== NHOM 4: SUA MON AN ====================

        [Test]
        [Order(4)]
        public void Test04_SuaMonAn_ThanhCong()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            var dongTest = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongTest, Is.Not.Null);

            dongTest!.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));
            Sleep(Short);

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='TenMon']")));
            Assert.That(tenInput.GetDomProperty("value"), Is.EqualTo(TenMonMoi));
            Assert.That(_driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("a.btn-light[href='/MonAn']")).Displayed, Is.True);

            tenInput.Clear();
            tenInput.SendKeys(TenMonSua);
            Sleep(Short);

            var giaInput = _driver.FindElement(By.CssSelector("input[name='Gia']"));
            giaInput.Clear();
            giaInput.SendKeys(GiaSua.ToString());
            Sleep(Short);

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Sleep(Short);

            var dongSua = TimMonAnTrenTatCaTrang(TenMonSua);
            Assert.That(dongSua, Is.Not.Null, $"Mon an '{TenMonSua}' phai xuat hien sau khi sua.");

            string giaDigits = new string(dongSua!.FindElements(By.TagName("td"))[2].Text.Where(char.IsDigit).ToArray());
            Assert.That(giaDigits, Does.Contain(GiaSua.ToString()));
            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null);

            Sleep(Short);
            Console.WriteLine($"[PASS] Sua mon an thanh cong: '{TenMonMoi}' -> '{TenMonSua}'");
        }

        // ==================== NHOM 5: SUA MON AN > HUY ====================

        [Test]
        [Order(5)]
        public void Test05_SuaMonAn_NutHuy()
        {
            NavigateToMonAn();
            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count, Is.GreaterThan(0));

            var firstRow = _driver.FindElement(By.CssSelector("table tbody tr"));
            string tenGoc = firstRow.FindElements(By.TagName("td"))[0].Text.Trim();

            firstRow.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));
            Sleep(Short);

            var tenInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='TenMon']")));
            tenInput.Clear();
            tenInput.SendKeys("Ten tam thoi se khong luu");
            Sleep(Short);

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/MonAn']")));
            huyBtn.Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Sleep(Short);

            Assert.That(_driver.Url, Does.Contain("/MonAn"));
            Assert.That(TimDongTheoTen(tenGoc), Is.Not.Null, $"Ten goc '{tenGoc}' phai van con.");
            Assert.That(TimDongTheoTen("Ten tam thoi se khong luu"), Is.Null);

            Sleep(Short);
            Console.WriteLine($"[PASS] Nut Huy trang Sua: Ten goc '{tenGoc}' van giu nguyen.");
        }

        // ==================== NHOM 6: XOA MON AN ====================

        [Test]
        [Order(6)]
        public void Test06_XoaMonAn_ThanhCong()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            var dongCanXoa = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongCanXoa, Is.Not.Null);

            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].removeAttribute('onclick');", xoaBtn);
            xoaBtn.Click();
            _wait.Until(d => !d.Url.Contains("/Delete/"));
            Sleep(Short);

            NavigateToMonAn();
            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null);

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc - 1));

            Sleep(Short);
            Console.WriteLine($"[PASS] Xoa mon an thanh cong: '{TenMonMoi}'");
        }

        // ==================== NHOM 7: DIALOG XOA - OK ====================

        [Test]
        [Order(7)]
        public void Test07_XoaMonAn_Dialog_Ok()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            var dongCanXoa = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongCanXoa, Is.Not.Null);

            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            Assert.That((xoaBtn.GetDomAttribute("onclick") ?? ""), Does.Contain("confirm"));

            ((IJavaScriptExecutor)_driver).ExecuteScript("window.confirm = function(){ return true; };");
            xoaBtn.Click();
            _wait.Until(d => !d.Url.Contains("/Delete/"));
            Sleep(Short);

            NavigateToMonAn();
            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null);

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc - 1));

            Sleep(Short);
            Console.WriteLine("[PASS] Dialog Xoa - OK: Mon an da bi xoa.");
        }

        // ==================== NHOM 8: DIALOG XOA - HUY ====================

        [Test]
        [Order(8)]
        public void Test08_XoaMonAn_Dialog_Huy()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            var dongCanXoa = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongCanXoa, Is.Not.Null);

            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger"));
            Assert.That((xoaBtn.GetDomAttribute("onclick") ?? ""), Does.Contain("confirm"));

            ((IJavaScriptExecutor)_driver).ExecuteScript("window.confirm = function(){ return false; };");
            xoaBtn.Click();
            Sleep(Short);

            Assert.That(_driver.Url, Does.Not.Contain("/Delete/"));
            Assert.That(_driver.Url, Does.Contain("/MonAn"));
            Assert.That(TimDongTheoTen(TenMonMoi), Is.Not.Null);

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc));

            Sleep(Short);
            Console.WriteLine($"[PASS] Dialog Xoa - Huy: Mon an '{TenMonMoi}' van con.");
        }

        // ==================== NHOM 9: CAP NHAT TRANG THAI CO SAN ====================

        [Test]
        [Order(9)]
        public void Test09_CapNhat_CoSan()
        {
            XoaNeuTonTai(TenMonMoi);
            TaoMonAn(TenMonMoi, GiaMoi, MoTaMoi);

            var dongTest = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongTest, Is.Not.Null);

            string classTruoc = dongTest!.FindElements(By.TagName("td"))[3]
                .FindElement(By.CssSelector("span.badge")).GetDomProperty("className") ?? "";
            bool coSanTruoc = classTruoc.Contains("badge-success");

            dongTest.FindElement(By.CssSelector("a.btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/MonAn/Edit/"));
            Sleep(Short);

            var coSanCheck = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='CoSan']")));
            Assert.That(coSanCheck.Displayed, Is.True);

            bool isCheckedTruoc = coSanCheck.Selected;
            coSanCheck.Click();
            Sleep(Short);
            bool isCheckedSau = coSanCheck.Selected;
            Assert.That(isCheckedSau, Is.Not.EqualTo(isCheckedTruoc));

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Sleep(Short);

            var dongSauSua = TimMonAnTrenTatCaTrang(TenMonMoi);
            Assert.That(dongSauSua, Is.Not.Null);

            string classSau = dongSauSua!.FindElements(By.TagName("td"))[3]
                .FindElement(By.CssSelector("span.badge")).GetDomProperty("className") ?? "";

            if (isCheckedSau)
                Assert.That(classSau, Does.Contain("badge-success"));
            else
                Assert.That(classSau, Does.Contain("badge-danger"));

            Sleep(Short);
            Console.WriteLine($"[PASS] Cap nhat CoSan: {coSanTruoc} -> {isCheckedSau}");
        }

        // ==================== NHOM 10: DON DEP ====================

        [Test]
        [Order(10)]
        public void Test10_DonDep_XoaDuLieuTest()
        {
            XoaNeuTonTai(TenMonMoi);
            XoaNeuTonTai(TenMonSua);

            Assert.That(TimMonAnTrenTatCaTrang(TenMonMoi), Is.Null);
            Assert.That(TimMonAnTrenTatCaTrang(TenMonSua), Is.Null);

            Sleep(Short);
            Console.WriteLine("[PASS] Don dep du lieu test mon an thanh cong.");
        }
    }
}
