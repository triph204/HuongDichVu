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

        private const string BaseUrl       = "http://localhost:5192";
        private const string DonHangUrl    = BaseUrl + "/DonHang";
        private const string LoginUrl      = BaseUrl + "/Dangnhap/Login";
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";
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

        private void NavigateToDonHang(string trangThai = "tatca", string sortBy = "moinhat")
        {
            _driver.Navigate().GoToUrl($"{DonHangUrl}?trangThai={trangThai}&sortBy={sortBy}");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".status-tabs")));
            Sleep(Short);
        }

        private void ClickTab(string trangThaiParam)
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector($"a.status-tab[href*='trangThai={trangThaiParam}']"))).Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".status-tabs")));
            Sleep(Short);
        }

        private bool TabDangActive(string trangThaiParam)
        {
            var activeTab = _driver.FindElement(By.CssSelector("a.status-tab.active"));
            return (activeTab.GetDomProperty("href") ?? "").Contains(trangThaiParam);
        }

        private int LaySoDonTrenTab(string trangThaiParam)
        {
            var tab = _driver.FindElement(By.CssSelector($"a.status-tab[href*='trangThai={trangThaiParam}']"));
            string text = tab.FindElement(By.CssSelector(".tab-count")).Text;
            return int.TryParse(text, out int count) ? count : 0;
        }

        private bool CoBangDonHang()
        {
            var tables = _driver.FindElements(By.CssSelector("table.donhang-table"));
            return tables.Count > 0 && tables[0].Displayed;
        }

        private void ChonSapXep(string value)
        {
            var select = new SelectElement(_wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector("select[name='sortBy']"))));
            select.SelectByValue(value);
            _wait.Until(d =>
            {
                var sel = new SelectElement(d.FindElement(By.CssSelector("select[name='sortBy']")));
                return sel.SelectedOption.GetDomProperty("value") == value;
            });
            Sleep(Short);
        }

        private void VaoChiTietDonDauTien()
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-primary.btn-sm"))).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Sleep(Short);
        }

        private void CapNhatTrangThai(string value)
        {
            var select = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            select.SelectByValue(value);
            Sleep(Short);
            _driver.FindElement(By.CssSelector("button[type='submit'].btn-success")).Click();
            _wait.Until(ExpectedConditions.UrlContains("/DonHang/Details/"));
            Sleep(Short);
        }

        private void KiemTraPhanTrang(string tabLabel)
        {
            var pageItems = _driver.FindElements(By.CssSelector(".pagination .page-item"));
            if (pageItems.Count == 0) { Assert.Pass($"Khong du don [{tabLabel}] de phan trang."); return; }

            var activePage = _driver.FindElement(By.CssSelector(".pagination .page-item.active .page-link"));
            Assert.That(activePage.Text.Trim(), Is.EqualTo("1"));

            var nextBtn = _driver.FindElements(By.CssSelector(".page-link"))
                .FirstOrDefault(e => e.Text.Contains("Sau"));

            if (nextBtn != null && nextBtn.Displayed)
            {
                nextBtn.Click();
                _wait.Until(d => d.Url.Contains("page=2"));
                Sleep(Short);
                Assert.That(_driver.Url, Does.Contain("page=2"));
                var activePage2 = _driver.FindElement(By.CssSelector(".pagination .page-item.active .page-link"));
                Assert.That(activePage2.Text.Trim(), Is.EqualTo("2"));
                Console.WriteLine($"[PASS] Phan trang [{tabLabel}]: trang 2. URL: {_driver.Url}");
            }
            else
            {
                Console.WriteLine($"[PASS] [{tabLabel}] chi co 1 trang.");
            }
        }

        private void TestChiTiet(string tab)
        {
            Assert.That(_driver.Url, Does.Contain("/DonHang/Details/"));
            Assert.That(_driver.FindElements(By.CssSelector("table tbody tr")).Count, Is.GreaterThan(0));
            Assert.That(_driver.FindElement(By.CssSelector("form[action*='UpdateStatus']")).Displayed, Is.True);
            Console.WriteLine($"[PASS] Chi tiet don hang (tab {tab}). URL: {_driver.Url}");
        }

        private void TestQuayLai(string tab)
        {
            var quayLaiBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector("a.btn.btn-light[href='/DonHang']")));
            quayLaiBtn.Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".status-tabs")));
            Sleep(Short);

            Assert.That(_driver.Url, Does.Contain("/DonHang"));
            Assert.That(_driver.Url, Does.Not.Contain("/Details/"));
            Console.WriteLine($"[PASS] Quay lai (tab {tab}). URL: {_driver.Url}");
        }

        // ==================== NHOM 1: TAT CA ====================

        [Test]
        [Order(1)]
        public void Test01_TatCa_TabActive()
        {
            NavigateToDonHang("tatca");

            Assert.That(TabDangActive("tatca"), Is.True);

            bool coNoiDung = CoBangDonHang() ||
                _driver.FindElements(By.CssSelector(".donhang-empty")).Count > 0;
            Assert.That(coNoiDung, Is.True);
            Assert.That(_driver.FindElement(By.CssSelector(".donhang-toolbar")).Displayed, Is.True);

            Console.WriteLine($"[PASS] Tab Tat ca active. Tong don: {LaySoDonTrenTab("tatca")}");
        }

        [Test]
        [Order(2)]
        public void Test02_TatCa_SapXep()
        {
            NavigateToDonHang("tatca");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don hang de test sap xep."); return; }

            ChonSapXep("tongtien-cao");
            Assert.That(_driver.Url, Does.Contain("sortBy=tongtien-cao"));
            Assert.That(_driver.Url, Does.Contain("trangThai=tatca"));

            var rows = _driver.FindElements(By.CssSelector("table.donhang-table tbody tr"));
            if (rows.Count >= 2)
            {
                var prices = rows.Select(r =>
                {
                    var cells = r.FindElements(By.TagName("td"));
                    if (cells.Count > 2)
                    {
                        string t = cells[2].Text.Replace("đ", "").Replace(",", "").Replace(".", "").Trim();
                        return long.TryParse(t, out long v) ? v : 0L;
                    }
                    return 0L;
                }).Where(v => v > 0).ToList();
                bool isDesc = prices.Zip(prices.Skip(1), (a, b) => a >= b).All(x => x);
                Assert.That(isDesc, Is.True, "Don hang phai giam dan theo tong tien.");
            }

            ChonSapXep("tongtien-thap");
            Assert.That(_driver.Url, Does.Contain("sortBy=tongtien-thap"));

            ChonSapXep("ban");
            Assert.That(_driver.Url, Does.Contain("sortBy=ban"));

            Console.WriteLine($"[PASS] Sap xep Tab Tat ca. URL: {_driver.Url}");
        }

        [Test]
        [Order(3)]
        public void Test03_TatCa_ChiTiet()
        {
            NavigateToDonHang("tatca");
            Assert.That(CoBangDonHang(), Is.True);
            VaoChiTietDonDauTien();

            Assert.That(_driver.FindElement(By.CssSelector(".card-header h2")).Text.Length, Is.GreaterThan(0));
            TestChiTiet("Tat ca");
        }

        [Test]
        [Order(4)]
        public void Test04_TatCa_ChiTiet_CapNhatTrangThai()
        {
            NavigateToDonHang("tatca");
            Assert.That(CoBangDonHang(), Is.True);
            VaoChiTietDonDauTien();
            CapNhatTrangThai("DANGCHUANBI");

            var sel = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            Assert.That(sel.SelectedOption.GetDomProperty("value"), Is.EqualTo("DANGCHUANBI"));

            Console.WriteLine("[PASS] Cap nhat trang thai -> Dang chuan bi (tab Tat ca).");
        }

        [Test]
        [Order(5)]
        public void Test05_TatCa_ChiTiet_QuayLai()
        {
            NavigateToDonHang("tatca");
            Assert.That(CoBangDonHang(), Is.True);
            VaoChiTietDonDauTien();
            TestQuayLai("Tat ca");
        }

        [Test]
        [Order(6)]
        public void Test06_TatCa_PhanTrang()
        {
            NavigateToDonHang("tatca");
            KiemTraPhanTrang("Tat ca");
        }

        // ==================== NHOM 2: CHO XAC NHAN ====================

        [Test]
        [Order(7)]
        public void Test07_ChoXacNhan_TabActive()
        {
            NavigateToDonHang("tatca");
            ClickTab("CHOXACNHAN");

            Assert.That(_driver.Url, Does.Contain("trangThai=CHOXACNHAN"));
            Assert.That(TabDangActive("CHOXACNHAN"), Is.True);

            Console.WriteLine($"[PASS] Tab Cho xac nhan active. So don: {LaySoDonTrenTab("CHOXACNHAN")}");
        }

        [Test]
        [Order(8)]
        public void Test08_ChoXacNhan_SapXep()
        {
            NavigateToDonHang("CHOXACNHAN");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don CHOXACNHAN de test sap xep."); return; }

            ChonSapXep("tongtien-cao");
            Assert.That(_driver.Url, Does.Contain("sortBy=tongtien-cao"));
            Assert.That(_driver.Url, Does.Contain("trangThai=CHOXACNHAN"));

            ChonSapXep("ban");
            Assert.That(_driver.Url, Does.Contain("sortBy=ban"));

            Console.WriteLine($"[PASS] Sap xep Tab Cho xac nhan. URL: {_driver.Url}");
        }

        [Test]
        [Order(9)]
        public void Test09_ChoXacNhan_ChiTiet()
        {
            NavigateToDonHang("CHOXACNHAN");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don CHOXACNHAN de test chi tiet."); return; }
            VaoChiTietDonDauTien();
            TestChiTiet("Cho xac nhan");
        }

        [Test]
        [Order(10)]
        public void Test10_ChoXacNhan_ChiTiet_CapNhatTrangThai()
        {
            NavigateToDonHang("CHOXACNHAN");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don CHOXACNHAN de test cap nhat."); return; }
            VaoChiTietDonDauTien();
            CapNhatTrangThai("CHOXACNHAN");

            var sel = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            Assert.That(sel.SelectedOption.GetDomProperty("value"), Is.EqualTo("CHOXACNHAN"));

            Console.WriteLine("[PASS] Cap nhat trang thai -> Cho xac nhan.");
        }

        [Test]
        [Order(11)]
        public void Test11_ChoXacNhan_ChiTiet_QuayLai()
        {
            NavigateToDonHang("CHOXACNHAN");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don CHOXACNHAN de test quay lai."); return; }
            VaoChiTietDonDauTien();
            TestQuayLai("Cho xac nhan");
        }

        [Test]
        [Order(12)]
        public void Test12_ChoXacNhan_PhanTrang()
        {
            NavigateToDonHang("CHOXACNHAN");
            KiemTraPhanTrang("Cho xac nhan");
        }

        // ==================== NHOM 3: DANG CHUAN BI ====================

        [Test]
        [Order(13)]
        public void Test13_DangChuanBi_TabActive()
        {
            NavigateToDonHang("tatca");
            ClickTab("DANGCHUANBI");

            Assert.That(_driver.Url, Does.Contain("trangThai=DANGCHUANBI"));
            Assert.That(TabDangActive("DANGCHUANBI"), Is.True);

            Console.WriteLine($"[PASS] Tab Dang chuan bi active. So don: {LaySoDonTrenTab("DANGCHUANBI")}");
        }

        [Test]
        [Order(14)]
        public void Test14_DangChuanBi_SapXep()
        {
            NavigateToDonHang("DANGCHUANBI");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don DANGCHUANBI de test sap xep."); return; }

            ChonSapXep("tongtien-thap");
            Assert.That(_driver.Url, Does.Contain("sortBy=tongtien-thap"));
            Assert.That(_driver.Url, Does.Contain("trangThai=DANGCHUANBI"));

            ChonSapXep("ban");
            Assert.That(_driver.Url, Does.Contain("sortBy=ban"));

            Console.WriteLine($"[PASS] Sap xep Tab Dang chuan bi. URL: {_driver.Url}");
        }

        [Test]
        [Order(15)]
        public void Test15_DangChuanBi_ChiTiet()
        {
            NavigateToDonHang("DANGCHUANBI");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don DANGCHUANBI de test chi tiet."); return; }
            VaoChiTietDonDauTien();
            TestChiTiet("Dang chuan bi");
        }

        [Test]
        [Order(16)]
        public void Test16_DangChuanBi_ChiTiet_CapNhatTrangThai()
        {
            NavigateToDonHang("DANGCHUANBI");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don DANGCHUANBI de test cap nhat."); return; }
            VaoChiTietDonDauTien();
            CapNhatTrangThai("HOANTHANH");

            var sel = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            Assert.That(sel.SelectedOption.GetDomProperty("value"), Is.EqualTo("HOANTHANH"));

            Console.WriteLine("[PASS] Cap nhat trang thai -> Hoan thanh (tu Dang chuan bi).");
        }

        [Test]
        [Order(17)]
        public void Test17_DangChuanBi_ChiTiet_QuayLai()
        {
            NavigateToDonHang("DANGCHUANBI");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don DANGCHUANBI de test quay lai."); return; }
            VaoChiTietDonDauTien();
            TestQuayLai("Dang chuan bi");
        }

        [Test]
        [Order(18)]
        public void Test18_DangChuanBi_PhanTrang()
        {
            NavigateToDonHang("DANGCHUANBI");
            KiemTraPhanTrang("Dang chuan bi");
        }

        // ==================== NHOM 4: HOAN THANH ====================

        [Test]
        [Order(19)]
        public void Test19_HoanThanh_TabActive()
        {
            NavigateToDonHang("tatca");
            ClickTab("HOANTHANH");

            Assert.That(_driver.Url, Does.Contain("trangThai=HOANTHANH"));
            Assert.That(TabDangActive("HOANTHANH"), Is.True);

            Console.WriteLine($"[PASS] Tab Hoan thanh active. So don: {LaySoDonTrenTab("HOANTHANH")}");
        }

        [Test]
        [Order(20)]
        public void Test20_HoanThanh_SapXep()
        {
            NavigateToDonHang("HOANTHANH");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don HOANTHANH de test sap xep."); return; }

            ChonSapXep("tongtien-cao");
            Assert.That(_driver.Url, Does.Contain("sortBy=tongtien-cao"));
            Assert.That(_driver.Url, Does.Contain("trangThai=HOANTHANH"));

            ChonSapXep("ban");
            Assert.That(_driver.Url, Does.Contain("sortBy=ban"));

            Console.WriteLine($"[PASS] Sap xep Tab Hoan thanh. URL: {_driver.Url}");
        }

        [Test]
        [Order(21)]
        public void Test21_HoanThanh_ChiTiet()
        {
            NavigateToDonHang("HOANTHANH");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don HOANTHANH de test chi tiet."); return; }
            VaoChiTietDonDauTien();
            TestChiTiet("Hoan thanh");
        }

        [Test]
        [Order(22)]
        public void Test22_HoanThanh_ChiTiet_CapNhatTrangThai()
        {
            NavigateToDonHang("HOANTHANH");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don HOANTHANH de test cap nhat."); return; }
            VaoChiTietDonDauTien();
            CapNhatTrangThai("HOANTHANH");

            var sel = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            Assert.That(sel.SelectedOption.GetDomProperty("value"), Is.EqualTo("HOANTHANH"));

            Console.WriteLine("[PASS] Cap nhat trang thai -> Hoan thanh.");
        }

        [Test]
        [Order(23)]
        public void Test23_HoanThanh_ChiTiet_QuayLai()
        {
            NavigateToDonHang("HOANTHANH");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don HOANTHANH de test quay lai."); return; }
            VaoChiTietDonDauTien();
            TestQuayLai("Hoan thanh");
        }

        [Test]
        [Order(24)]
        public void Test24_HoanThanh_PhanTrang()
        {
            NavigateToDonHang("HOANTHANH");
            KiemTraPhanTrang("Hoan thanh");
        }

        // ==================== NHOM 5: DA HUY ====================

        [Test]
        [Order(25)]
        public void Test25_DaHuy_TabActive()
        {
            NavigateToDonHang("tatca");
            ClickTab("HUY");

            Assert.That(_driver.Url, Does.Contain("trangThai=HUY"));
            Assert.That(TabDangActive("HUY"), Is.True);

            Console.WriteLine($"[PASS] Tab Da huy active. So don: {LaySoDonTrenTab("HUY")}");
        }

        [Test]
        [Order(26)]
        public void Test26_DaHuy_SapXep()
        {
            NavigateToDonHang("HUY");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don HUY de test sap xep."); return; }

            ChonSapXep("tongtien-cao");
            Assert.That(_driver.Url, Does.Contain("sortBy=tongtien-cao"));
            Assert.That(_driver.Url, Does.Contain("trangThai=HUY"));

            ChonSapXep("ban");
            Assert.That(_driver.Url, Does.Contain("sortBy=ban"));

            Console.WriteLine($"[PASS] Sap xep Tab Da huy. URL: {_driver.Url}");
        }

        [Test]
        [Order(27)]
        public void Test27_DaHuy_ChiTiet()
        {
            NavigateToDonHang("HUY");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don HUY de test chi tiet."); return; }
            VaoChiTietDonDauTien();
            TestChiTiet("Da huy");
        }

        [Test]
        [Order(28)]
        public void Test28_DaHuy_ChiTiet_CapNhatTrangThai()
        {
            NavigateToDonHang("HUY");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don HUY de test cap nhat."); return; }
            VaoChiTietDonDauTien();
            CapNhatTrangThai("HUY");

            var sel = new SelectElement(_wait.Until(
                ExpectedConditions.ElementIsVisible(By.CssSelector("select[name='trangThai']"))));
            Assert.That(sel.SelectedOption.GetDomProperty("value"), Is.EqualTo("HUY"));

            Console.WriteLine("[PASS] Cap nhat trang thai -> Huy.");
        }

        [Test]
        [Order(29)]
        public void Test29_DaHuy_ChiTiet_QuayLai()
        {
            NavigateToDonHang("HUY");
            if (!CoBangDonHang()) { Assert.Pass("Khong co don HUY de test quay lai."); return; }
            VaoChiTietDonDauTien();
            TestQuayLai("Da huy");
        }

        [Test]
        [Order(30)]
        public void Test30_DaHuy_PhanTrang()
        {
            NavigateToDonHang("HUY");
            KiemTraPhanTrang("Da huy");
        }
    }
}
