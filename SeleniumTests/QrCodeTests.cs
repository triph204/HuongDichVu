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
    public class QrCodeTests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        private const string BaseUrl       = "http://localhost:5192";
        private const string QrCodeUrl     = BaseUrl + "/QrCode";
        private const string BanAnUrl      = BaseUrl + "/BanAn";
        private const string LoginUrl      = BaseUrl + "/Dangnhap/Login";
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123";
        private const string SoBanTest     = "9801";
        private const int    Delay         = 1000;

        // ==================== SETUP / TEARDOWN ====================

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            _driver = new ChromeDriver(options);
            _wait   = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
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

        private void NavigateToQrCode()
        {
            _driver.Navigate().GoToUrl(QrCodeUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".page-header")));
            Pause();
        }

        private void VaoChiTietQrDauTien()
        {
            NavigateToQrCode();
            var chiTietBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector(".qr-actions a.btn-info.btn-sm")));
            chiTietBtn.Click();
            _wait.Until(ExpectedConditions.UrlContains("/QrCode/View/"));
            Pause();
        }

        private IWebElement? TimQrCardTheoSoBan(string soBan)
        {
            var cards = _driver.FindElements(By.CssSelector(".qr-card"));
            return cards.FirstOrDefault(c =>
                c.FindElements(By.CssSelector(".qr-card-header h3"))
                 .Any(h => h.Text.Trim().Contains(soBan)));
        }

        private void XoaBanNeuTonTai(string soBan)
        {
            _driver.Navigate().GoToUrl(BanAnUrl);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".card")));

            var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
            var row = rows.FirstOrDefault(r =>
            {
                var cells = r.FindElements(By.TagName("td"));
                if (cells.Count == 0) return false;
                string text = cells[0].Text.Trim();
                return text == $"Bàn {soBan}" || text == $"Ban {soBan}" || text == soBan;
            });

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

        // ==================== NHOM 1: DANH SACH QR CODE ====================

        [Test]
        [Order(1)]
        public void Test01_XemDanhSachMaQrCode()
        {
            // Arrange
            NavigateToQrCode();

            // Assert - URL ?úng
            Assert.That(_driver.Url, Does.Contain("/QrCode"),
                "Ph?i ?i?u h??ng ??n trang /QrCode.");

            // Assert - Có tiêu ?? trang
            var header = _driver.FindElement(By.CssSelector(".page-header h2"));
            Assert.That(header.Displayed, Is.True, "Ph?i có tiêu ?? trang.");
            Assert.That(header.Text.Length, Is.GreaterThan(0), "Tiêu ?? không ???c r?ng.");

            // Assert - Có n?i dung: QR card ho?c empty state
            bool coNoiDung =
                _driver.FindElements(By.CssSelector(".qr-card")).Count > 0 ||
                _driver.FindElements(By.CssSelector(".empty-state")).Count > 0;
            Assert.That(coNoiDung, Is.True,
                "Trang ph?i hi?n th? QR card ho?c thông báo r?ng.");

            Pause();
            int soCard = _driver.FindElements(By.CssSelector(".qr-card")).Count;
            Console.WriteLine($"[PASS] Xem danh sách mã QR Code. S? bàn hi?n th?: {soCard}. URL: {_driver.Url}");
        }

        [Test]
        [Order(2)]
        public void Test02_QrCard_CoDuThanhPhan()
        {
            // Arrange
            NavigateToQrCode();

            var cards = _driver.FindElements(By.CssSelector(".qr-card"));
            if (cards.Count == 0) { Assert.Pass("Chua co ban an de kiem tra QR card."); return; }

            var card = cards[0];

            // Assert - Có header v?i tên bàn "?? Bàn X"
            var tenBan = card.FindElement(By.CssSelector(".qr-card-header h3"));
            Assert.That(tenBan.Displayed, Is.True, "Ph?i có tiêu ?? tên bàn trong QR card.");
            Assert.That(tenBan.Text, Does.Contain("Bàn"),
                "Tiêu ?? QR card ph?i ch?a 'Bàn'.");

            // Assert - Có badge tr?ng thái
            var badge = card.FindElement(By.CssSelector(".qr-card-header .badge"));
            Assert.That(badge.Displayed, Is.True, "Ph?i có badge tr?ng thái.");
            Assert.That(badge.Text.Length, Is.GreaterThan(0), "Badge tr?ng thái không ???c r?ng.");

            // Assert - Có ?nh QR t? qrserver.com kích th??c 200x200
            var qrImg = card.FindElement(By.CssSelector(".qr-image img"));
            Assert.That(qrImg.Displayed, Is.True, "Ph?i có ?nh QR trong card.");
            string imgSrc = qrImg.GetDomProperty("src") ?? "";
            Assert.That(imgSrc, Does.Contain("qrserver.com/v1/create-qr-code"),
                "?nh QR ph?i dùng API qrserver.com.");
            Assert.That(imgSrc, Does.Contain("size=200x200"),
                "QR card danh sách ph?i dùng kích th??c 200x200.");
            Assert.That(imgSrc, Does.Contain("data="),
                "URL ?nh QR ph?i ch?a tham s? data.");

            // Assert - Có nút Xem Chi Ti?t
            var xemChiTiet = card.FindElement(By.CssSelector(".qr-actions a.btn-info.btn-sm"));
            Assert.That(xemChiTiet.Displayed, Is.True, "Ph?i có nút 'Xem Chi Ti?t'.");
            string chiTietHref = xemChiTiet.GetDomProperty("href") ?? "";
            Assert.That(chiTietHref, Does.Contain("/QrCode/View/"),
                "Nút 'Xem Chi Ti?t' ph?i tr? ??n /QrCode/View/{id}.");

            // Assert - Có nút T?i Xu?ng
            var taiXuong = card.FindElement(By.CssSelector(".qr-actions a.btn-success.btn-sm"));
            Assert.That(taiXuong.Displayed, Is.True, "Ph?i có nút 'T?i Xu?ng'.");

            Pause();
            Console.WriteLine($"[PASS] QR card ??y ?? thành ph?n. Bàn: '{tenBan.Text}', Tr?ng thái: '{badge.Text}'");
        }

        [Test]
        [Order(3)]
        public void Test03_NutTaiXuong_TrenQrCard()
        {
            // Arrange
            NavigateToQrCode();

            var cards = _driver.FindElements(By.CssSelector(".qr-card"));
            if (cards.Count == 0) { Assert.Pass("Chua co ban an de kiem tra nut Tai Xuong."); return; }

            var card = cards[0];
            string tenBan = card.FindElement(By.CssSelector(".qr-card-header h3")).Text.Trim();
            var taiXuongBtn = card.FindElement(By.CssSelector(".qr-actions a.btn-success.btn-sm"));

            // Assert 1 - Nút hi?n th?
            Assert.That(taiXuongBtn.Displayed, Is.True, "Nút T?i Xu?ng ph?i hi?n th?.");

            // Assert 2 - Có thu?c tính download (cho phép t?i file)
            string downloadAttr = taiXuongBtn.GetDomProperty("download") ?? "";
            Assert.That(downloadAttr.Length, Is.GreaterThan(0),
                "Nút T?i Xu?ng ph?i có thu?c tính 'download'.");

            // Assert 3 - Tên file có ??nh d?ng "QR_Ban_X.png"
            Assert.That(downloadAttr, Does.Contain("QR"),
                $"Tên file ph?i ch?a 'QR', th?c t?: '{downloadAttr}'.");
            Assert.That(downloadAttr.EndsWith(".png"), Is.True,
                $"File t?i xu?ng ph?i có ?uôi .png, th?c t?: '{downloadAttr}'.");

            // Assert 4 - href tr? ?úng API qrserver v?i size 500x500
            string href = taiXuongBtn.GetDomProperty("href") ?? "";
            Assert.That(href, Does.Contain("qrserver.com/v1/create-qr-code"),
                "Link t?i xu?ng ph?i tr? ??n API qrserver.com.");
            Assert.That(href, Does.Contain("size=500x500"),
                "Nút T?i Xu?ng trên card ph?i t?i file kích th??c 500x500.");

            Pause();
            Console.WriteLine($"[PASS] Nút T?i Xu?ng h?p l?. Bàn: '{tenBan}', File: '{downloadAttr}', Size: 500x500.");
        }

        // ==================== NHOM 2: CHI TIET QR CODE ====================

        [Test]
        [Order(4)]
        public void Test04_ChiTiet_KiemTraThongTinUrl()
        {
            // Arrange
            NavigateToQrCode();

            var cards = _driver.FindElements(By.CssSelector(".qr-card"));
            if (cards.Count == 0) { Assert.Pass("Chua co ban an de kiem tra chi tiet QR."); return; }

            // L?y s? bàn t? card tr??c khi click
            string tenBanTrenCard = cards[0]
                .FindElement(By.CssSelector(".qr-card-header h3")).Text.Trim();
            string soBanTrenCard = new string(tenBanTrenCard.Where(char.IsDigit).ToArray());

            // Act - Vào trang chi ti?t
            VaoChiTietQrDauTien();

            // Assert - URL trang ?úng
            Assert.That(_driver.Url, Does.Contain("/QrCode/View/"),
                "Ph?i chuy?n ??n trang /QrCode/View/{id}.");

            // Assert - Tiêu ?? trang ch?a s? bàn
            var header = _driver.FindElement(By.CssSelector(".qr-detail-header h2"));
            Assert.That(header.Text, Does.Contain("Bàn"),
                "Tiêu ?? trang chi ti?t ph?i ch?a 'Bàn'.");
            Assert.That(header.Text, Does.Contain(soBanTrenCard),
                $"Tiêu ?? ph?i ch?a s? bàn '{soBanTrenCard}'.");

            // Assert - Kh?i thông tin .qr-info hi?n th?
            var qrInfo = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector(".qr-info")));
            Assert.That(qrInfo.Displayed, Is.True, "Ph?i có kh?i thông tin .qr-info.");

            // Assert - Có ?? 3 th? <p> (S? Bàn, Tr?ng Thái, URL)
            var paragraphs = qrInfo.FindElements(By.TagName("p"));
            Assert.That(paragraphs.Count, Is.GreaterThanOrEqualTo(3),
                "Kh?i .qr-info ph?i có ít nh?t 3 dòng thông tin.");

            // Assert - M?i dòng có th? <strong> làm nhãn
            var labels = qrInfo.FindElements(By.TagName("strong"));
            Assert.That(labels.Count, Is.GreaterThanOrEqualTo(3),
                "Ph?i có ít nh?t 3 nhãn <strong> trong kh?i thông tin.");

            // Assert - Có th? <code> ch?a URL ??t món v?i table=
            var codeTag = qrInfo.FindElement(By.CssSelector("code"));
            string qrUrl = codeTag.Text.Trim();
            Assert.That(qrUrl.Length, Is.GreaterThan(0),
                "URL QR trong th? <code> không ???c r?ng.");
            Assert.That(qrUrl, Does.Contain("table="),
                "URL QR ph?i ch?a tham s? 'table=' ?? nh?n di?n bàn.");
            Assert.That(qrUrl, Does.Contain(soBanTrenCard),
                $"URL QR ph?i ch?a s? bàn '{soBanTrenCard}', th?c t?: '{qrUrl}'.");

            Pause();
            Console.WriteLine($"[PASS] Thong tin URL QR dung. Ban: {soBanTrenCard}, URL: '{qrUrl}'");
        }

        [Test]
        [Order(5)]
        public void Test05_ChiTiet_TaiQr_800x800()
        {
            // Arrange
            NavigateToQrCode();

            if (_driver.FindElements(By.CssSelector(".qr-card")).Count == 0)
            { Assert.Pass("Chua co ban an de kiem tra tai QR 800x800."); return; }

            VaoChiTietQrDauTien();

            // Tìm nút t?i 800x800 trong .qr-actions-large
            var actionBtns = _driver.FindElements(
                By.CssSelector(".qr-actions-large a[download]"));
            Assert.That(actionBtns.Count, Is.GreaterThan(0),
                "Trang chi ti?t ph?i có ít nh?t m?t nút t?i xu?ng.");

            var btn800 = actionBtns.FirstOrDefault(b =>
                (b.GetDomProperty("href") ?? "").Contains("size=800x800"));

            // Assert 1 - Nút 800x800 t?n t?i
            Assert.That(btn800, Is.Not.Null,
                "Ph?i có nút t?i QR kích th??c 800x800.");

            // Assert 2 - Nút hi?n th?
            Assert.That(btn800!.Displayed, Is.True,
                "Nút t?i QR 800x800 ph?i hi?n th?.");

            // Assert 3 - href tr? ?úng API
            string href = btn800.GetDomProperty("href") ?? "";
            Assert.That(href, Does.Contain("qrserver.com/v1/create-qr-code"),
                "Nút t?i 800x800 ph?i dùng API qrserver.com.");
            Assert.That(href, Does.Contain("size=800x800"),
                "href ph?i ch?a size=800x800.");

            // Assert 4 - Có thu?c tính download v?i tên file .png
            string downloadName = btn800.GetDomProperty("download") ?? "";
            Assert.That(downloadName, Does.Contain("QR"),
                $"Tên file ph?i ch?a 'QR', th?c t?: '{downloadName}'.");
            Assert.That(downloadName.EndsWith(".png"), Is.True,
                $"File ph?i có ?uôi .png, th?c t?: '{downloadName}'.");
            Assert.That(downloadName, Does.Contain("Large"),
                "Tên file 800x800 ph?i ch?a 'Large' ?? phân bi?t.");

            Pause();
            Console.WriteLine($"[PASS] Nút t?i QR 800x800 h?p l?. File: '{downloadName}'");
        }

        [Test]
        [Order(6)]
        public void Test06_ChiTiet_TaiQr_1200x1200()
        {
            // Arrange
            NavigateToQrCode();

            if (_driver.FindElements(By.CssSelector(".qr-card")).Count == 0)
            { Assert.Pass("Chua co ban an de kiem tra tai QR 1200x1200."); return; }

            VaoChiTietQrDauTien();

            var actionBtns = _driver.FindElements(
                By.CssSelector(".qr-actions-large a[download]"));
            Assert.That(actionBtns.Count, Is.GreaterThan(0),
                "Trang chi ti?t ph?i có ít nh?t m?t nút t?i xu?ng.");

            var btn1200 = actionBtns.FirstOrDefault(b =>
                (b.GetDomProperty("href") ?? "").Contains("size=1200x1200"));

            // Assert 1 - Nút 1200x1200 t?n t?i
            Assert.That(btn1200, Is.Not.Null,
                "Ph?i có nút t?i QR kích th??c 1200x1200.");

            // Assert 2 - Nút hi?n th?
            Assert.That(btn1200!.Displayed, Is.True,
                "Nút t?i QR 1200x1200 ph?i hi?n th?.");

            // Assert 3 - href tr? ?úng API
            string href = btn1200.GetDomProperty("href") ?? "";
            Assert.That(href, Does.Contain("qrserver.com/v1/create-qr-code"),
                "Nút t?i 1200x1200 ph?i dùng API qrserver.com.");
            Assert.That(href, Does.Contain("size=1200x1200"),
                "href ph?i ch?a size=1200x1200.");

            // Assert 4 - Có thu?c tính download v?i tên file .png
            string downloadName = btn1200.GetDomProperty("download") ?? "";
            Assert.That(downloadName, Does.Contain("QR"),
                $"Tên file ph?i ch?a 'QR', th?c t?: '{downloadName}'.");
            Assert.That(downloadName.EndsWith(".png"), Is.True,
                $"File ph?i có ?uôi .png, th?c t?: '{downloadName}'.");
            Assert.That(downloadName, Does.Contain("HD"),
                "Tên file 1200x1200 ph?i ch?a 'HD' ?? phân bi?t.");

            Pause();
            Console.WriteLine($"[PASS] Nút t?i QR 1200x1200 h?p l?. File: '{downloadName}'");
        }

        [Test]
        [Order(7)]
        public void Test07_ChiTiet_InQrCode()
        {
            // Arrange
            NavigateToQrCode();

            if (_driver.FindElements(By.CssSelector(".qr-card")).Count == 0)
            { Assert.Pass("Chua co ban an de kiem tra nut In."); return; }

            VaoChiTietQrDauTien();

            // Act
            var inBtn = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector(".qr-actions-large button.btn-info")));

            // Assert 1 - Nút In hi?n th?
            Assert.That(inBtn.Displayed, Is.True, "Ph?i có nút 'In QR Code'.");

            // Assert 2 - Text nút không r?ng
            Assert.That(inBtn.Text.Trim().Length, Is.GreaterThan(0),
                "Nút In không ???c r?ng text.");

            // Assert 3 - Ki?m tra onclick b?ng GetAttribute (tránh l?i GetDomProperty tr? dict)
            string onclick = inBtn.GetAttribute("onclick") ?? "";
            Assert.That(onclick, Does.Contain("print"),
                "Nút In ph?i có onclick g?i window.print().");

            // Act - Inject JS ?? ch?n h?p tho?i print (tránh treo test)
            ((IJavaScriptExecutor)_driver).ExecuteScript("window.print = function(){};");
            inBtn.Click();
            Pause();

            // Assert 4 - Trang không b? ?i?u h??ng sau khi click In
            Assert.That(_driver.Url, Does.Contain("/QrCode/View/"),
                "Sau khi click In, v?n ph?i ? trang chi ti?t QR.");

            Pause();
            Console.WriteLine($"[PASS] Nut In QR Code hoat dong dung. onclick: '{onclick}'");
        }

        [Test]
        [Order(8)]
        public void Test08_ChiTiet_HuongDanSuDung()
        {
            // Arrange
            NavigateToQrCode();

            if (_driver.FindElements(By.CssSelector(".qr-card")).Count == 0)
            { Assert.Pass("Chua co ban an de kiem tra huong dan."); return; }

            VaoChiTietQrDauTien();

            // Assert 1 - Có section h??ng d?n
            var hdSection = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.CssSelector(".qr-instructions")));
            Assert.That(hdSection.Displayed, Is.True,
                "Ph?i có section h??ng d?n s? d?ng .qr-instructions.");

            // Assert 2 - Có tiêu ?? "H??ng D?n S? D?ng"
            var hdTitle = hdSection.FindElement(By.TagName("h3"));
            Assert.That(hdTitle.Displayed, Is.True, "Tiêu ?? h??ng d?n ph?i hi?n th?.");
            Assert.That(hdTitle.Text.Length, Is.GreaterThan(0),
                "Tiêu ?? h??ng d?n không ???c r?ng.");

            // Assert 3 - Có danh sách b??c (ol > li), t?i thi?u 4 b??c
            var steps = hdSection.FindElements(By.CssSelector("ol li"));
            Assert.That(steps.Count, Is.GreaterThanOrEqualTo(4),
                "H??ng d?n ph?i có ít nh?t 4 b??c s? d?ng.");

            // Assert 4 - M?i b??c không ???c r?ng
            foreach (var step in steps)
            {
                Assert.That(step.Text.Trim().Length, Is.GreaterThan(0),
                    "M?i b??c h??ng d?n không ???c r?ng n?i dung.");
            }

            // Assert 5 - Có ph?n l?u ý .qr-note
            var note = hdSection.FindElement(By.CssSelector(".qr-note"));
            Assert.That(note.Displayed, Is.True,
                "Ph?i có ph?n 'L?u ý' (.qr-note) trong h??ng d?n.");
            Assert.That(note.Text.Trim().Length, Is.GreaterThan(0),
                "N?i dung 'L?u ý' không ???c r?ng.");

            Pause();
            Console.WriteLine($"[PASS] Huong dan su dung day du. Tieu de: '{hdTitle.Text}', So buoc: {steps.Count}");
        }

        [Test]
        [Order(9)]
        public void Test09_ChiTiet_NutQuayLai()
        {
            // Arrange
            NavigateToQrCode();

            if (_driver.FindElements(By.CssSelector(".qr-card")).Count == 0)
            { Assert.Pass("Chua co ban an de test nut Quay Lai."); return; }

            // Ghi nh? s? card tr??c khi vào chi ti?t
            int soCardTruoc = _driver.FindElements(By.CssSelector(".qr-card")).Count;

            // Act
            VaoChiTietQrDauTien();
            Assert.That(_driver.Url, Does.Contain("/QrCode/View/"),
                "Ph?i ?ang ? trang chi ti?t tr??c khi test Quay L?i.");

            // Assert 1 - Nút Quay L?i t?n t?i và hi?n th?
            var quayLaiBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.CssSelector(".qr-detail-header a.btn-secondary")));
            Assert.That(quayLaiBtn.Displayed, Is.True, "Ph?i có nút '? Quay L?i'.");

            // Assert 2 - href tr? v? trang Index QrCode
            string href = quayLaiBtn.GetDomProperty("href") ?? "";
            Assert.That(href, Does.Contain("/QrCode"),
                "Nút Quay L?i ph?i tr? v? trang /QrCode.");

            // Act - Click Quay L?i
            quayLaiBtn.Click();
            _wait.Until(d => !d.Url.Contains("/View/"));
            Pause();

            // Assert 3 - URL v? ?úng trang danh sách
            Assert.That(_driver.Url, Does.Contain("/QrCode"),
                "Sau khi click Quay L?i ph?i v? trang danh sách QR.");
            Assert.That(_driver.Url, Does.Not.Contain("/View/"),
                "Không ???c ? l?i trang View sau khi click Quay L?i.");

            // Assert 4 - Danh sách QR card v?n hi?n th? ??y ??
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".page-header")));
            int soCardSau = _driver.FindElements(By.CssSelector(".qr-card")).Count;
            Assert.That(soCardSau, Is.EqualTo(soCardTruoc),
                "S? QR card sau khi Quay L?i ph?i b?ng v?i tr??c khi vào chi ti?t.");

            Pause();
            Console.WriteLine($"[PASS] Nut Quay Lai hoat dong dung. URL: {_driver.Url}, So card: {soCardSau}");
        }

        // ==================== NHOM 3: DON DEP ====================

        [Test]
        [Order(10)]
        public void Test10_DonDep_XoaDuLieuTest()
        {
            // Act
            XoaBanNeuTonTai(SoBanTest);

            NavigateToQrCode();

            // Assert
            var card = TimQrCardTheoSoBan(SoBanTest);
            Assert.That(card, Is.Null,
                $"QR card c?a bàn {SoBanTest} ph?i ???c d?n d?p.");

            Pause();
            Console.WriteLine("[PASS] Don dep du lieu test QR thanh cong.");
        }
    }
}
