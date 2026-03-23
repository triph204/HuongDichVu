using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;
using System.Threading;

namespace testClient
{
    [TestFixture]
    public class OrderPageTests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;
        private IJavaScriptExecutor _js;

        private static readonly string BaseUrl =
            Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:5002";

        private const int Short = 500;
        private const int Medium = 1000;
        private const int Long = 2000;

        // =====================================================================
        // SETUP / TEARDOWN
        // =====================================================================

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            var headless = Environment.GetEnvironmentVariable("TEST_HEADLESS");
            if (!string.IsNullOrEmpty(headless) &&
                headless.Equals("true", StringComparison.OrdinalIgnoreCase))
                options.AddArgument("--headless=new");

            options.AddArgument("--window-size=1280,900");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--remote-allow-origins=*");

            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            _js = (IJavaScriptExecutor)_driver;
        }

        [TearDown]
        public void TearDown()
        {
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }
        }

        // =====================================================================
        // HELPERS
        // =====================================================================

        private void Sleep(int ms) => Thread.Sleep(ms);

        private void Pass(string message) =>
            Console.WriteLine($"[PASS] {message}");

        private void Info(string message) =>
            Console.WriteLine($"[INFO] {message}");

        private void ScrollAndClick(IWebElement el)
        {
            _js.ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
            Sleep(Short);
            _js.ExecuteScript("arguments[0].click();", el);
        }

        private void SetTableLocalStorage()
        {
            _js.ExecuteScript(
                "localStorage.setItem('tableNumber',  '1');" +
                "localStorage.setItem('tableDisplay', 'B\u00e0n 1');" +
                "localStorage.setItem('tableId',      '13');"
            );
        }

        /// <summary>
        /// Mở trang Order?table=1, set localStorage, chờ SignalR load dữ liệu.
        /// Navigate 2 lần giống pattern MenuTests để bypass redirect.
        /// </summary>
        private void OpenOrderPage()
        {
            Info($"Đang mở trang Order: {BaseUrl}/Order?table=1");

            _driver.Navigate().GoToUrl(BaseUrl + "/Order?table=1");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("body")));
            SetTableLocalStorage();
            Sleep(Short);

            _driver.Navigate().GoToUrl(BaseUrl + "/Order?table=1");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("body")));
            SetTableLocalStorage();
            Sleep(Long); // Chờ SignalR kết nối và render đơn hàng

            Info($"Order page đã load. URL: {_driver.Url}");
        }

        private void ClickTab(string status)
        {
            var tab = _wait.Until(d => d.FindElement(By.CssSelector($".tab-btn[data-status='{status}']")));
            ScrollAndClick(tab);
            Sleep(Medium);
        }

        private IWebElement? FindChiTietButton()
        {
            var candidates = _driver.FindElements(By.XPath("//button[contains(.,'Chi ti\u1ebft')]"));
            return candidates.Count > 0 ? candidates[0] : null;
        }

        private void OpenModalChiTiet()
        {
            var btn = FindChiTietButton();
            Assert.That(btn, Is.Not.Null, "Không tìm thấy nút 'Chi tiết' – cần có đơn hàng trong DB.");
            ScrollAndClick(btn!);
            _wait.Until(d => d.FindElement(By.Id("orderDetailModal"))
                               .GetAttribute("class").Contains("show"));
        }

        // =====================================================================
        // GROUP 1 – HEADER
        // =====================================================================

        [Test, Order(1)]
        [Description("Header hiển thị tiêu đề 'Đơn Hàng'")]
        public void O01_Header_TitleDisplayed()
        {
            OpenOrderPage();

            var title = _wait.Until(d => d.FindElement(By.CssSelector("h4.header-title")));
            Assert.That(title.Displayed, Is.True);
            Assert.That(title.Text, Does.Contain("Đơn Hàng"));
            Pass($"Header tiêu đề: '{title.Text}'.");
        }

        [Test, Order(2)]
        [Description("Header hiển thị số bàn (id=headerTableNumber)")]
        public void O02_Header_TableNumberDisplayed()
        {
            OpenOrderPage();

            var tableNum = _wait.Until(d => d.FindElement(By.Id("headerTableNumber")));
            Assert.That(tableNum.Displayed, Is.True);
            Pass($"Số bàn hiển thị: '{tableNum.Text}'.");
        }

        [Test, Order(3)]
        [Description("Nút quay lại (←) hiển thị và có href hợp lệ")]
        public void O03_Header_BackButtonDisplayed()
        {
            OpenOrderPage();

            var backBtn = _wait.Until(d => d.FindElement(By.CssSelector("a.btn-outline-danger")));
            Assert.That(backBtn.Displayed, Is.True);
            Assert.That(backBtn.GetAttribute("href"), Is.Not.Null.And.Not.Empty);
            Pass($"Nút ← href='{backBtn.GetAttribute("href")}'.");
        }

        // =====================================================================
        // GROUP 2 – TAB LỌC TRẠNG THÁI
        // =====================================================================

        [Test, Order(4)]
        [Description("Hiển thị đủ 5 tab với đúng nhãn")]
        public void O04_Tabs_AllFiveDisplayed()
        {
            OpenOrderPage();

            var tabs = _driver.FindElements(By.CssSelector(".tab-btn"));
            Assert.That(tabs.Count, Is.EqualTo(5), $"Số tab: {tabs.Count}");

            var labels = new[] { "Tất cả", "Chờ xác nhận", "Đang chuẩn bị", "Hoàn thành", "Đã Hủy" };
            for (int i = 0; i < labels.Length; i++)
                Assert.That(tabs[i].Text, Does.Contain(labels[i]));

            Pass("5 tab hiển thị đúng nhãn.");
        }

        [Test, Order(5)]
        [Description("Tab 'Tất cả' mặc định active khi vào trang")]
        public void O05_Tab_AllActiveByDefault()
        {
            OpenOrderPage();

            var allTab = _wait.Until(d => d.FindElement(By.CssSelector(".tab-btn[data-status='all']")));
            StringAssert.Contains("active", allTab.GetAttribute("class"));
            Pass("Tab 'Tất cả' active mặc định.");
        }

        [Test, Order(6)]
        [Description("Click 'Chờ xác nhận' → tab active, chỉ hiển thị đơn đúng trạng thái")]
        public void O06_Tab_Pending_ActiveAndFilter()
        {
            OpenOrderPage();
            ClickTab("pending");

            var activeTab = _wait.Until(d => d.FindElement(By.CssSelector(".tab-btn.active")));
            Assert.That(activeTab.GetAttribute("data-status"), Is.EqualTo("pending"));

            var cards = _driver.FindElements(By.CssSelector("#ordersContainer .card"));
            if (cards.Count > 0)
            {
                bool allMatch = cards.All(c => c.Text.Contains("Chờ xác nhận"));
                Assert.That(allMatch, Is.True);
            }
            Pass($"Tab 'Chờ xác nhận': {cards.Count} đơn.");
        }

        [Test, Order(7)]
        [Description("Click 'Đang chuẩn bị' → tab active, chỉ hiển thị đơn đúng trạng thái")]
        public void O07_Tab_Cooking_ActiveAndFilter()
        {
            OpenOrderPage();
            ClickTab("cooking");

            var activeTab = _wait.Until(d => d.FindElement(By.CssSelector(".tab-btn.active")));
            Assert.That(activeTab.GetAttribute("data-status"), Is.EqualTo("cooking"));

            var cards = _driver.FindElements(By.CssSelector("#ordersContainer .card"));
            if (cards.Count > 0)
            {
                bool allMatch = cards.All(c => c.Text.Contains("Đang chuẩn bị"));
                Assert.That(allMatch, Is.True);
            }
            Pass($"Tab 'Đang chuẩn bị': {cards.Count} đơn.");
        }

        [Test, Order(8)]
        [Description("Click 'Hoàn thành' → tab active, chỉ hiển thị đơn đúng trạng thái")]
        public void O08_Tab_Completed_ActiveAndFilter()
        {
            OpenOrderPage();
            ClickTab("completed");

            var activeTab = _wait.Until(d => d.FindElement(By.CssSelector(".tab-btn.active")));
            Assert.That(activeTab.GetAttribute("data-status"), Is.EqualTo("completed"));

            var cards = _driver.FindElements(By.CssSelector("#ordersContainer .card"));
            if (cards.Count > 0)
            {
                bool allMatch = cards.All(c => c.Text.Contains("Hoàn thành"));
                Assert.That(allMatch, Is.True);
            }
            Pass($"Tab 'Hoàn thành': {cards.Count} đơn.");
        }

        [Test, Order(9)]
        [Description("Click 'Đã Hủy' → tab active, chỉ hiển thị đơn đúng trạng thái")]
        public void O09_Tab_Cancel_ActiveAndFilter()
        {
            OpenOrderPage();
            ClickTab("cancel");

            var activeTab = _wait.Until(d => d.FindElement(By.CssSelector(".tab-btn.active")));
            Assert.That(activeTab.GetAttribute("data-status"), Is.EqualTo("cancel"));

            var cards = _driver.FindElements(By.CssSelector("#ordersContainer .card"));

            // --- DEBUG: thêm vào đây ---
            Info($"Số card tìm được: {cards.Count}");
            foreach (var c in cards)
                Info($"Card text: '{c.Text}'");
            var container = _driver.FindElement(By.Id("ordersContainer"));
            Info($"Container text: '{container.Text}'");
            // --- hết DEBUG ---

            if (cards.Count > 0)
            {
                bool allMatch = cards.All(c =>
                    c.Text.Contains("Đã hủy") ||
                    c.Text.Contains("Hủy") ||
                    c.Text.Contains("hủy", StringComparison.OrdinalIgnoreCase));
                Assert.That(allMatch, Is.True);
            }
            Pass($"Tab 'Đã Hủy': {cards.Count} đơn.");
        }

        [Test, Order(10)]
        [Description("Lọc theo tab rồi quay về 'Tất cả' → tab all active trở lại")]
        public void O10_Tab_BackToAll_AfterFilter()
        {
            OpenOrderPage();
            ClickTab("pending");
            ClickTab("all");

            var activeTab = _wait.Until(d => d.FindElement(By.CssSelector(".tab-btn.active")));
            Assert.That(activeTab.GetAttribute("data-status"), Is.EqualTo("all"));
            Pass("Quay về tab 'Tất cả' thành công.");
        }

        // =====================================================================
        // GROUP 3 – DANH SÁCH ĐƠN HÀNG
        // =====================================================================

        [Test, Order(11)]
        [Description("ordersContainer hiển thị sau khi trang load")]
        public void O11_OrdersContainer_Displayed()
        {
            OpenOrderPage();

            var container = _wait.Until(d => d.FindElement(By.Id("ordersContainer")));
            Assert.That(container.Displayed, Is.True);
            Pass("ordersContainer hiển thị.");
        }

        [Test, Order(12)]
        [Description("Danh sách có ít nhất 1 card hoặc thông báo trống")]
        public void O12_OrderList_HasContentOrEmpty()
        {
            OpenOrderPage();
            Sleep(Long);

            var container = _driver.FindElement(By.Id("ordersContainer"));
            bool hasCards = _driver.FindElements(By.CssSelector("#ordersContainer .card")).Count > 0;
            bool hasEmptyMsg = container.Text.Contains("Không có") || container.Text.Contains("trống");

            Assert.That(hasCards || hasEmptyMsg, Is.True, "Container không có nội dung gì.");
            Pass($"hasCards={hasCards}, hasEmptyMsg={hasEmptyMsg}.");
        }

        [Test, Order(13)]
        [Description("Card đơn hàng hiển thị mã đơn (ORD-...)")]
        public void O13_OrderCard_ShowsOrderId()
        {
            OpenOrderPage();
            Sleep(Long);

            var cards = _driver.FindElements(By.CssSelector("#ordersContainer .card"));
            if (cards.Count == 0) { Info("Không có đơn hàng – bỏ qua."); Assert.Pass(); return; }

            Assert.That(cards[0].Text, Does.Contain("ORD").Or.Contain("#"));
            Pass("Card hiển thị mã đơn hàng.");
        }

        [Test, Order(14)]
        [Description("Card đơn hàng hiển thị tổng tiền")]
        public void O14_OrderCard_ShowsTotal()
        {
            OpenOrderPage();
            Sleep(Long);

            var cards = _driver.FindElements(By.CssSelector("#ordersContainer .card"));
            if (cards.Count == 0) { Info("Không có đơn hàng – bỏ qua."); Assert.Pass(); return; }

            Assert.That(cards[0].Text, Does.Contain("Tổng").Or.Contain("đ"));
            Pass("Card hiển thị tổng tiền.");
        }

        [Test, Order(15)]
        [Description("Card đơn hàng hiển thị badge trạng thái")]
        public void O15_OrderCard_ShowsStatusBadge()
        {
            OpenOrderPage();
            Sleep(Long);

            var cards = _driver.FindElements(By.CssSelector("#ordersContainer .card"));
            if (cards.Count == 0) { Info("Không có đơn hàng – bỏ qua."); Assert.Pass(); return; }

            var statusTexts = new[] { "Chờ xác nhận", "Đang chuẩn bị", "Hoàn thành", "Đã Hủy" };
            bool hasStatus = statusTexts.Any(s => cards[0].Text.Contains(s));
            Assert.That(hasStatus, Is.True, "Card không có badge trạng thái.");
            Pass("Card có badge trạng thái.");
        }

        // =====================================================================
        // GROUP 4 – MODAL CHI TIẾT ĐƠN HÀNG
        // =====================================================================

        [Test, Order(16)]
        [Description("Nút 'Chi tiết' tồn tại trong danh sách")]
        public void O16_DetailButton_Exists()
        {
            OpenOrderPage();
            Sleep(Long);

            var btns = _driver.FindElements(By.XPath("//button[contains(.,'Chi ti\u1ebft')]"));
            if (btns.Count == 0) { Info("Không có đơn hàng → không có nút Chi tiết."); Assert.Pass(); return; }

            Assert.That(btns.Count, Is.GreaterThan(0));
            Pass($"Tìm được {btns.Count} nút 'Chi tiết'.");
        }

        [Test, Order(17)]
        [Description("Click 'Chi tiết' → modal mở (class 'show')")]
        public void O17_DetailButton_OpensModal()
        {
            OpenOrderPage();
            Sleep(Long);

            var btn = FindChiTietButton();
            if (btn == null) { Info("Không có nút Chi tiết – bỏ qua."); Assert.Pass(); return; }

            ScrollAndClick(btn);
            _wait.Until(d => d.FindElement(By.Id("orderDetailModal"))
                               .GetAttribute("class").Contains("show"));

            StringAssert.Contains("show", _driver.FindElement(By.Id("orderDetailModal")).GetAttribute("class"));
            Pass("Modal chi tiết mở thành công.");
        }

        [Test, Order(18)]
        [Description("Modal có tiêu đề 'Chi tiết đơn hàng'")]
        public void O18_Modal_TitleDisplayed()
        {
            OpenOrderPage();
            Sleep(Long);

            var btn = FindChiTietButton();
            if (btn == null) { Info("Không có nút Chi tiết – bỏ qua."); Assert.Pass(); return; }

            ScrollAndClick(btn);
            _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("#orderDetailModal .modal-title")));

            var title = _driver.FindElement(By.CssSelector("#orderDetailModal .modal-title"));
            Assert.That(title.Text, Does.Contain("Chi tiết"));
            Pass($"Tiêu đề modal: '{title.Text}'.");
        }

        [Test, Order(19)]
        [Description("Modal hiển thị nội dung đơn hàng (không rỗng)")]
        public void O19_Modal_ContentNotEmpty()
        {
            OpenOrderPage();
            Sleep(Long);

            var btn = FindChiTietButton();
            if (btn == null) { Info("Không có nút Chi tiết – bỏ qua."); Assert.Pass(); return; }

            ScrollAndClick(btn);
            _wait.Until(d => !string.IsNullOrWhiteSpace(
                d.FindElement(By.Id("orderDetailContent")).Text));

            Assert.That(_driver.FindElement(By.Id("orderDetailContent")).Text, Is.Not.Empty);
            Pass("Modal có nội dung chi tiết.");
        }

        [Test, Order(20)]
        [Description("Click nút X → modal đóng")]
        public void O20_Modal_CloseButton_ClosesModal()
        {
            OpenOrderPage();
            Sleep(Long);

            var btn = FindChiTietButton();
            if (btn == null) { Info("Không có nút Chi tiết – bỏ qua."); Assert.Pass(); return; }

            ScrollAndClick(btn);
            _wait.Until(d => d.FindElement(By.Id("orderDetailModal"))
                               .GetAttribute("class").Contains("show"));

            ScrollAndClick(_driver.FindElement(By.CssSelector("#orderDetailModal .btn-close")));
            Sleep(Medium);

            Assert.That(_driver.FindElement(By.Id("orderDetailModal")).GetAttribute("class"),
                Does.Not.Contain("show"));
            Pass("Modal đóng sau khi click X.");
        }

        // =====================================================================
        // GROUP 5 – ĐIỀU HƯỚNG
        // =====================================================================

        [Test, Order(21)]
        [Description("Click nút ← → điều hướng ra khỏi trang Order")]
        public void O21_Navigation_BackButton()
        {
            OpenOrderPage();

            var backBtn = _wait.Until(d => d.FindElement(By.CssSelector("a.btn-outline-danger")));
            backBtn.Click();
            Sleep(Long);

            Assert.That(_driver.Url, Does.Not.Contain("/Order"));
            Pass($"Nút ← đến: {_driver.Url}");
        }

        [Test, Order(22)]
        [Description("Click 'Thực đơn' ở bottom nav → điều hướng về trang menu")]
        public void O22_Navigation_MenuButton()
        {
            OpenOrderPage();

            var menuBtn = _wait.Until(d => d.FindElement(By.CssSelector(".mobile-nav .nav-item:first-child")));
            Assert.That(menuBtn.Text, Does.Contain("Thực đơn"));

            menuBtn.Click();
            Sleep(Long);

            Assert.That(_driver.Url, Does.Not.Contain("/Order"));
            Pass($"Nút 'Thực đơn' đến: {_driver.Url}");
        }

        [Test, Order(23)]
        [Description("Nút 'Đơn hàng' ở bottom nav có class active khi đang ở trang Order")]
        public void O23_Navigation_OrderNavButtonIsActive()
        {
            OpenOrderPage();

            var orderBtn = _wait.Until(d => d.FindElement(By.CssSelector(".mobile-nav .nav-item:last-child")));
            Assert.That(orderBtn.Text, Does.Contain("Đơn hàng"));
            StringAssert.Contains("active", orderBtn.GetAttribute("class"));
            Pass("Nút 'Đơn hàng' active khi đang ở trang Order.");
        }
    }
}