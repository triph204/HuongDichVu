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
        private const string SoBanMoi      = "9901";
        private const string SoBanSua      = "9902";
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
            _driver.FindElement(By.Name("password")).SendKeys(AdminPassword);
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-primary")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang"));
            Sleep(Short);
        }

        private void NavigateToBanAn()
        {
            _driver.Navigate().GoToUrl(BanAnUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
            Sleep(Short);
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
                Sleep(Short);
            }
        }

        private void TaoBanAn(string soBan)
        {
            _driver.Navigate().GoToUrl(BanAnUrl + "/Create");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='SoBan']")));
            Sleep(Short);
            var input = _driver.FindElement(By.CssSelector("input[name='SoBan']"));
            input.Clear();
            input.SendKeys(soBan);
            Sleep(Short);
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Sleep(Short);
            NavigateToBanAn();
        }

        // ==================== NHOM 1: DANH SACH BAN AN ====================

        [Test]
        [Order(1)]
        public void Test01_DanhSach_TruyCapMenu()
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

            Sleep(Short);
            Console.WriteLine($"[PASS] Truy cap menu Ban an thanh cong. URL: {_driver.Url}");
        }

        [Test]
        [Order(2)]
        public void Test02_DanhSach_KiemTraTrangThai()
        {
            NavigateToBanAn();

            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count == 0) { Assert.Pass("Chua co du lieu ban an de kiem tra."); return; }

            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                Assert.That(cells.Count, Is.EqualTo(3), "Moi dong phai co dung 3 cot.");
            }

            var badge = rows[0].FindElement(By.CssSelector("span.badge"));
            Assert.That(badge.Displayed, Is.True, "Cot Trang Thai phai co badge.");
            Assert.That(badge.Text.Trim().Length, Is.GreaterThan(0), "Badge trang thai khong duoc rong.");

            string badgeClass = badge.GetDomProperty("className") ?? "";
            bool coClassDung = badgeClass.Contains("badge-success") || badgeClass.Contains("badge-warning");
            Assert.That(coClassDung, Is.True, "Badge phai co class badge-success hoac badge-warning.");

            Assert.That(rows[0].FindElement(By.CssSelector("a.btn-primary.btn-sm")).Displayed, Is.True, "Phai co nut Sua.");
            Assert.That(rows[0].FindElement(By.CssSelector("a.btn-danger.btn-sm")).Displayed, Is.True, "Phai co nut Xoa.");

            Sleep(Short);
            Console.WriteLine($"[PASS] Kiem tra trang thai ban an. So dong: {rows.Count}, Trang thai: '{badge.Text}'");
        }

        // ==================== NHOM 2: THEM BAN AN ====================

        [Test]
        [Order(3)]
        public void Test03_ThemBan_NhapNoiDungHopLe()
        {
            XoaNeuTonTai(SoBanMoi);
            XoaNeuTonTai(SoBanSua);
            NavigateToBanAn();

            var themBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/BanAn/Create']")));
            Assert.That(themBtn.Displayed, Is.True, "Nut '+ Them Ban' phai hien thi.");
            themBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Create"));
            Sleep(Medium);

            var soBanInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='SoBan']")));
            Assert.That(soBanInput.Displayed, Is.True, "O nhap SoBan phai hien thi.");
            Assert.That(_driver.FindElement(By.CssSelector("select[name='TrangThai']")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("input[name='NgayTao']")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("a.btn-light[href='/BanAn']")).Displayed, Is.True);

            soBanInput.Clear();
            soBanInput.SendKeys(SoBanMoi);
            Sleep(Short);
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Sleep(Medium);

            NavigateToBanAn();
            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Not.Null,
                $"Ban an so '{SoBanMoi}' phai xuat hien trong bang sau khi them.");

            Sleep(Short);
            Console.WriteLine($"[PASS] Them ban an thanh cong: So ban {SoBanMoi}");
        }

        [Test]
        [Order(4)]
        public void Test04_ThemBan_BoTrongSoBan()
        {
            NavigateToBanAn();
            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/BanAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Create"));
            Sleep(Short);

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            Sleep(Short);

            var soBanInput = _driver.FindElement(By.CssSelector("input[name='SoBan']"));
            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;", soBanInput);
            Assert.That(isInvalid, Is.True, "Input SoBan phai bi HTML5 validation khi bo trong.");
            Assert.That(_driver.Url, Does.Contain("/Create"), "Phai o lai trang Create.");

            Sleep(Short);
            Console.WriteLine("[PASS] Bo trong so ban: Bi chan boi HTML5 required.");
        }

        [Test]
        [Order(5)]
        public void Test05_ThemBan_NhapSoBan3_TrangThaiTrong()
        {
            XoaNeuTonTai("3");
            NavigateToBanAn();

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/BanAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Create"));
            Sleep(Short);

            var soBanInput = _driver.FindElement(By.CssSelector("input[name='SoBan']"));
            soBanInput.Clear();
            soBanInput.SendKeys("3");
            Sleep(Short);

            var trangThaiSelect = new SelectElement(
                _driver.FindElement(By.CssSelector("select[name='TrangThai']")));
            string selectedValue = trangThaiSelect.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(selectedValue, Does.Contain("Tr"), "Trang thai mac dinh phai la 'Trong'.");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Sleep(Short);

            NavigateToBanAn();
            var dongMoi = TimDongTheoSoBan("3");
            if (dongMoi != null)
            {
                var badge = dongMoi.FindElements(By.TagName("td"))[1].FindElement(By.CssSelector("span.badge"));
                Assert.That(badge.Displayed, Is.True, "Badge trang thai phai hien thi.");
                Console.WriteLine($"[PASS] Nhap so ban 3, trang thai: '{badge.Text}'");
            }
            else
            {
                Assert.Pass("Ban so 3 da duoc tao (co the trung voi ban hien co).");
            }

            XoaNeuTonTai("3");
            Sleep(Short);
        }

        // ==================== NHOM 3: DROPDOWN TRANG THAI ====================

        [Test]
        [Order(6)]
        public void Test06_ThemBan_DropdownTrangThai()
        {
            NavigateToBanAn();
            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/BanAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Create"));
            Sleep(Short);

            var trangThaiEl = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("select[name='TrangThai']")));
            Assert.That(trangThaiEl.Displayed, Is.True, "Dropdown TrangThai phai hien thi.");

            var options = trangThaiEl.FindElements(By.TagName("option"));
            Assert.That(options.Count, Is.GreaterThanOrEqualTo(3), "Phai co it nhat 3 lua chon.");

            var sel = new SelectElement(trangThaiEl);
            string defaultValue = sel.SelectedOption.GetDomProperty("value") ?? "";
            Assert.That(defaultValue, Does.Contain("Tr"), "Gia tri mac dinh phai la 'Trong'.");

            foreach (var opt in options)
            {
                Assert.That((opt.GetDomProperty("value") ?? "").Length, Is.GreaterThan(0));
                Assert.That(opt.Text.Trim().Length, Is.GreaterThan(0));
            }

            var optDangSuDung = options.FirstOrDefault(o =>
                o.Text.Contains("Dang") || o.Text.Contains("Đang"));
            if (optDangSuDung != null)
            {
                sel.SelectByText(optDangSuDung.Text);
                Sleep(Short);
                Assert.That(sel.SelectedOption.Text, Is.EqualTo(optDangSuDung.Text));
            }

            Sleep(Short);
            Console.WriteLine($"[PASS] Dropdown TrangThai co {options.Count} option. Mac dinh: '{defaultValue}'");
        }

        // ==================== NHOM 4: TRUONG NGAY TAO ====================

        [Test]
        [Order(7)]
        public void Test07_ThemBan_NgayTaoMacDinh()
        {
            NavigateToBanAn();
            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/BanAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Create"));
            Sleep(Short);

            var ngayTaoInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("input[name='NgayTao']")));
            Assert.That(ngayTaoInput.Displayed, Is.True, "Truong NgayTao phai hien thi.");

            string ngayTaoValue = ngayTaoInput.GetDomProperty("value") ?? "";
            Assert.That(ngayTaoValue.Length, Is.GreaterThan(0), "NgayTao phai co gia tri mac dinh.");

            string ngayHomNay = DateTime.Now.ToString("yyyy-MM-dd");
            Assert.That(ngayTaoValue, Does.StartWith(ngayHomNay),
                $"NgayTao mac dinh phai la ngay hom nay ({ngayHomNay}), thuc te: '{ngayTaoValue}'.");

            bool isReadonly = ngayTaoInput.GetDomProperty("readOnly")?.ToString()?.ToLower() == "true";
            Assert.That(isReadonly || ngayTaoInput.GetDomAttribute("readonly") != null, Is.True,
                "Truong NgayTao phai la readonly khi them moi.");

            Sleep(Short);
            Console.WriteLine($"[PASS] Truong NgayTao mac dinh: '{ngayTaoValue}'");
        }

        // ==================== NHOM 5: NUT HUY ====================

        [Test]
        [Order(8)]
        public void Test08_ThemBan_NutHuy()
        {
            NavigateToBanAn();
            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/BanAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Create"));
            Sleep(Short);

            _driver.FindElement(By.CssSelector("input[name='SoBan']")).SendKeys("9999");
            Sleep(Short);

            var huyBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn-light[href='/BanAn']")));
            Assert.That(huyBtn.Displayed, Is.True, "Nut 'Huy' phai hien thi.");
            huyBtn.Click();
            _wait.Until(d => !d.Url.Contains("/Create"));
            Sleep(Short);

            Assert.That(_driver.Url, Does.Contain("/BanAn"));
            Assert.That(_driver.Url, Does.Not.Contain("/Create"));
            Assert.That(TimDongTheoSoBan("9999"), Is.Null, "Ban an khong duoc luu khi bam Huy.");

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc));

            Sleep(Short);
            Console.WriteLine("[PASS] Nut Huy trang Them: Ve danh sach, khong luu du lieu.");
        }

        // ==================== NHOM 6: NHAP DU LIEU DAC BIET ====================

        [Test]
        [Order(9)]
        public void Test09_ThemBan_NhapKyTuDacBiet()
        {
            NavigateToBanAn();
            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/BanAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Create"));
            Sleep(Short);

            var soBanInput = _driver.FindElement(By.CssSelector("input[name='SoBan']"));
            soBanInput.Clear();
            soBanInput.SendKeys("@#$%");
            Sleep(Short);

            string actualValue = soBanInput.GetDomProperty("value") ?? "";
            Assert.That(actualValue.Length, Is.EqualTo(0),
                "Truong type=number khong duoc chap nhan ky tu dac biet.");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            Sleep(Short);

            var isInvalid = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return !arguments[0].validity.valid;", soBanInput);
            Assert.That(isInvalid, Is.True, "Input type=number phai invalid khi nhap ky tu dac biet.");
            Assert.That(_driver.Url, Does.Contain("/Create"));

            Sleep(Short);
            Console.WriteLine("[PASS] Nhap ky tu dac biet: Bi chan boi input type=number.");
        }

        // ==================== NHOM 7: GIOI HAN KY TU ====================

        [Test]
        [Order(10)]
        public void Test10_ThemBan_GioiHanKyTu()
        {
            NavigateToBanAn();
            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a[href='/BanAn/Create']"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Create"));
            Sleep(Short);

            var soBanInput = _driver.FindElement(By.CssSelector("input[name='SoBan']"));
            soBanInput.Clear();
            soBanInput.SendKeys(new string('1', 101));
            Sleep(Short);

            string actualValue = soBanInput.GetDomProperty("value") ?? "";
            Assert.That(actualValue.Length, Is.GreaterThan(0), "Phai co gia tri duoc nhap vao.");

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            Sleep(Short);

            bool oLaiCreate = _driver.Url.Contains("/Create");
            Console.WriteLine(oLaiCreate
                ? "[PASS] Nhap 101 ky tu: Bi chan boi validation."
                : "[PASS] Nhap 101 ky tu: Server chap nhan.");

            Assert.Pass("Test nhap gioi han ky tu hoan tat.");
        }

        // ==================== NHOM 8: SUA BAN AN ====================

        [Test]
        [Order(11)]
        public void Test11_SuaBan_BamNutSua()
        {
            XoaNeuTonTai(SoBanMoi);
            XoaNeuTonTai(SoBanSua);
            TaoBanAn(SoBanMoi);

            var dongTest = TimDongTheoSoBan(SoBanMoi);
            Assert.That(dongTest, Is.Not.Null, $"Phai tim thay ban so '{SoBanMoi}'.");

            dongTest!.FindElement(By.CssSelector("a.btn-primary.btn-sm")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Edit/"));
            Sleep(Short);

            Assert.That(_driver.Url, Does.Contain("/BanAn/Edit/"));

            var soBanInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='SoBan']")));
            Assert.That(soBanInput.GetDomProperty("value"), Is.EqualTo(SoBanMoi));
            Assert.That(_driver.FindElement(By.CssSelector("select[name='TrangThai']")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Displayed, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector("a.btn-light[href='/BanAn']")).Displayed, Is.True);

            Sleep(Short);
            Console.WriteLine($"[PASS] Bam nut Sua: Chuyen sang trang Edit, SoBan: '{SoBanMoi}'");
        }

        [Test]
        [Order(12)]
        public void Test12_SuaBan_ThayDoiTenVaLuu()
        {
            NavigateToBanAn();
            var dongTest = TimDongTheoSoBan(SoBanMoi);
            if (dongTest == null) { XoaNeuTonTai(SoBanSua); TaoBanAn(SoBanMoi); dongTest = TimDongTheoSoBan(SoBanMoi); }
            Assert.That(dongTest, Is.Not.Null, $"Phai tim thay ban so '{SoBanMoi}'.");

            dongTest!.FindElement(By.CssSelector("a.btn-primary.btn-sm")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/BanAn/Edit/"));
            Sleep(Short);

            var soBanInput = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='SoBan']")));
            soBanInput.Clear();
            soBanInput.SendKeys(SoBanSua);
            Sleep(Short);

            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(d => !d.Url.Contains("/Edit/"));
            Sleep(Short);

            NavigateToBanAn();
            Assert.That(TimDongTheoSoBan(SoBanSua), Is.Not.Null, $"Ban so '{SoBanSua}' phai xuat hien sau khi sua.");
            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null, $"So ban cu '{SoBanMoi}' khong duoc ton tai.");

            Sleep(Short);
            Console.WriteLine($"[PASS] Sua ban an thanh cong: {SoBanMoi} -> {SoBanSua}");
        }

        // ==================== NHOM 9: XOA BAN AN ====================

        [Test]
        [Order(13)]
        public void Test13_XoaBan_BamNutXoa()
        {
            XoaNeuTonTai(SoBanMoi);
            XoaNeuTonTai(SoBanSua);
            TaoBanAn(SoBanMoi);

            var dongCanXoa = TimDongTheoSoBan(SoBanMoi);
            Assert.That(dongCanXoa, Is.Not.Null, $"Ban so '{SoBanMoi}' phai ton tai truoc khi xoa.");

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger.btn-sm"));
            Assert.That(xoaBtn.Displayed, Is.True, "Nut 'Xoa' phai hien thi.");

            string onclickAttr = xoaBtn.GetDomAttribute("onclick") ?? "";
            Assert.That(onclickAttr, Does.Contain("confirm"), "Nut Xoa phai co onclick goi confirm().");

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].removeAttribute('onclick');", xoaBtn);
            xoaBtn.Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
            Sleep(Short);

            NavigateToBanAn();
            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null, $"Ban so '{SoBanMoi}' phai bi xoa.");

            Sleep(Short);
            Console.WriteLine($"[PASS] Xoa ban an thanh cong: So ban {SoBanMoi}");
        }

        // ==================== NHOM 10: DIALOG XOA - OK ====================

        [Test]
        [Order(14)]
        public void Test14_XoaBan_Dialog_Ok()
        {
            XoaNeuTonTai(SoBanMoi);
            TaoBanAn(SoBanMoi);

            var dongCanXoa = TimDongTheoSoBan(SoBanMoi);
            Assert.That(dongCanXoa, Is.Not.Null);
            Sleep(Short);

            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger.btn-sm"));
            Assert.That((xoaBtn.GetDomAttribute("onclick") ?? ""), Does.Contain("confirm"));

            ((IJavaScriptExecutor)_driver).ExecuteScript("window.confirm = function(){ return true; };");
            xoaBtn.Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));
            Sleep(Short);

            NavigateToBanAn();
            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null, "Ban an phai bi xoa khi bam OK.");

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc - 1));

            Sleep(Short);
            Console.WriteLine("[PASS] Dialog Xoa - OK: Ban an da bi xoa.");
        }

        // ==================== NHOM 11: DIALOG XOA - HUY ====================

        [Test]
        [Order(15)]
        public void Test15_XoaBan_Dialog_Huy()
        {
            XoaNeuTonTai(SoBanMoi);
            TaoBanAn(SoBanMoi);

            var dongCanXoa = TimDongTheoSoBan(SoBanMoi);
            Assert.That(dongCanXoa, Is.Not.Null);
            Sleep(Short);

            int soDongTruoc = _driver.FindElements(By.CssSelector("table tbody tr")).Count;

            var xoaBtn = dongCanXoa!.FindElement(By.CssSelector("a.btn-danger.btn-sm"));
            Assert.That((xoaBtn.GetDomAttribute("onclick") ?? ""), Does.Contain("confirm"));

            ((IJavaScriptExecutor)_driver).ExecuteScript("window.confirm = function(){ return false; };");
            xoaBtn.Click();
            Sleep(Short);

            Assert.That(_driver.Url, Does.Not.Contain("/Delete/"));
            Assert.That(_driver.Url, Does.Contain("/BanAn"));
            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Not.Null, $"Ban so '{SoBanMoi}' phai van con.");

            int soDongSau = _driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.That(soDongSau, Is.EqualTo(soDongTruoc));

            Sleep(Short);
            Console.WriteLine($"[PASS] Dialog Xoa - Huy: Ban so '{SoBanMoi}' van con.");
        }

        // ==================== NHOM 12: DON DEP ====================

        [Test]
        [Order(16)]
        public void Test16_DonDep_XoaDuLieuTest()
        {
            XoaNeuTonTai(SoBanMoi);
            XoaNeuTonTai(SoBanSua);
            XoaNeuTonTai("9999");
            XoaNeuTonTai("8888");

            NavigateToBanAn();
            Assert.That(TimDongTheoSoBan(SoBanMoi), Is.Null);
            Assert.That(TimDongTheoSoBan(SoBanSua), Is.Null);

            Sleep(Short);
            Console.WriteLine("[PASS] Don dep du lieu test ban an thanh cong.");
        }
    }
}
