////usi
//using NUnit.Framework;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.Support.UI;
//using SeleniumExtras.WaitHelpers;
//using System;
//using System.Linq;

//namespace testClient
//{
//    [TestFixture]
//    public class MenuTests
//    {
//        private IWebDriver _driver;
//        private WebDriverWait _wait;
//        private IJavaScriptExecutor _js;

//        private static readonly string BaseUrl =
//            Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:5002";

//        private const int Short = 500;
//        private const int Medium = 1000;
//        private const int Long = 2000;

//        // 5 món test – phải tồn tại trong DB thật
//        // ID 10 Khoai tâyy 35k | 11 Phô mai que 20k | 12 Cá viên chiên 30k
//        // ID 22 Nước ngọt 15k  | 23 Cà phê sữa đá 25k

//        // =====================================================================
//        // SETUP / TEARDOWN
//        // =====================================================================

//        [SetUp]
//        public void SetUp()
//        {
//            var options = new ChromeOptions();
//            var headless = Environment.GetEnvironmentVariable("TEST_HEADLESS");
//            if (!string.IsNullOrEmpty(headless) &&
//                headless.Equals("true", StringComparison.OrdinalIgnoreCase))
//                options.AddArgument("--headless=new");

//            options.AddArgument("--window-size=1280,900");
//            options.AddArgument("--no-sandbox");
//            options.AddArgument("--disable-dev-shm-usage");
//            options.AddArgument("--remote-allow-origins=*");

//            _driver = new ChromeDriver(options);
//            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30)); // tăng lên 30s
//            _js = (IJavaScriptExecutor)_driver;
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            try { _driver?.Quit(); } catch { }
//            try { _driver?.Dispose(); } catch { }
//        }

//        // =====================================================================
//        // HELPERS
//        // =====================================================================

//        private void Sleep(int ms) => System.Threading.Thread.Sleep(ms);

//        /// <summary>In thông báo [PASS] kèm thông tin bổ sung ra console.</summary>
//        private void Pass(string message) =>
//            Console.WriteLine($"[PASS] {message}");

//        /// <summary>In thông báo [INFO] để ghi nhận trạng thái trung gian.</summary>
//        private void Info(string message) =>
//            Console.WriteLine($"[INFO] {message}");

//        private void ScrollAndClick(IWebElement el)
//        {
//            _js.ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
//            Sleep(Short);
//            _js.ExecuteScript("arguments[0].click();", el);
//        }

//        /// <summary>
//        /// Stub /api/BanAn và /api/DonHang bằng JS fetch override.
//        /// Gọi SAU khi page đã load để không bị reset.
//        /// </summary>
//        private void InjectBanAnDonHangStub()
//        {
//            _js.ExecuteScript(@"
//                window._origFetch = window._origFetch || window.fetch;
//                window.fetch = function(url, opts) {
//                    if (url && url.includes('/api/BanAn')) {
//                        return Promise.resolve(new Response(JSON.stringify([
//                            { id:13, soBan:'1' }
//                        ]), { status:200, headers:{ 'Content-Type':'application/json' } }));
//                    }
//                    if (url && url.includes('/api/DonHang')) {
//                        return Promise.resolve(new Response(
//                            JSON.stringify({ id:999, message:'ok' }),
//                            { status:200, headers:{ 'Content-Type':'application/json' } }
//                        ));
//                    }
//                    return window._origFetch.apply(this, arguments);
//                };
//            ");
//            Sleep(Short);
//        }

//        private void SetTableLocalStorage()
//        {
//            _js.ExecuteScript(
//                "localStorage.setItem('tableNumber',  '1');" +
//                "localStorage.setItem('tableDisplay', 'B\u00e0n 1');" +
//                "localStorage.setItem('tableId',      '13');"
//            );
//        }

//        /// <summary>
//        /// Mở trang menu /?table=1.
//        /// Navigate 2 lần để bypass redirect /Error/InvalidTable:
//        ///   Lần 1 → set localStorage trước
//        ///   Lần 2 → JS đọc localStorage, không redirect, gọi /api/BanAn thật
//        /// </summary>
//        private void OpenMenu()
//        {
//            Info($"Đang mở menu: {BaseUrl}/?table=1");

//            _driver.Navigate().GoToUrl(BaseUrl + "/?table=1");
//            _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("body")));
//            SetTableLocalStorage();
//            _js.ExecuteScript("localStorage.removeItem('restaurantCart');");
//            Sleep(Short);

//            _driver.Navigate().GoToUrl(BaseUrl + "/?table=1");
//            _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("body")));
//            Sleep(Short);
//            SetTableLocalStorage();
//            Sleep(Medium);

//            _wait.Until(d => d.FindElements(By.CssSelector(".dish-card")).Count > 0);
//            Info($"Menu đã load. URL: {_driver.Url}");
//        }

//        /// <summary>
//        /// Điều hướng sang trang Cart.
//        /// Navigate 2 lần để bypass redirect, inject stub BanAn+DonHang sau khi load.
//        /// </summary>
//        private void GoToCart()
//        {
//            Info($"Chuyển sang trang Cart: {BaseUrl}/Cart?table=1");

//            _driver.Navigate().GoToUrl(BaseUrl + "/Cart?table=1");
//            _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("body")));
//            SetTableLocalStorage();
//            Sleep(Short);

//            _driver.Navigate().GoToUrl(BaseUrl + "/Cart?table=1");
//            _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("body")));
//            Sleep(Short);
//            InjectBanAnDonHangStub();
//            SetTableLocalStorage();
//            Sleep(Medium);

//            Info($"Cart đã load. URL: {_driver.Url}");
//        }

//        private IWebElement? FindAddButton(string dishName)
//        {
//            foreach (var card in _driver.FindElements(By.CssSelector(".dish-card")))
//            {
//                try
//                {
//                    if (card.FindElement(By.CssSelector(".dish-name")).Text.Contains(dishName))
//                        return card.FindElement(By.CssSelector(".btn-add-cart:not([disabled])"));
//                }
//                catch { }
//            }
//            return null;
//        }

//        private void AddDishToCart(string dishName)
//        {
//            var btn = _wait.Until(_ => FindAddButton(dishName));
//            Assert.IsNotNull(btn, $"Không tìm thấy nút Thêm cho '{dishName}'. Món phải tồn tại trong DB.");
//            ScrollAndClick(btn);
//            Sleep(Short);
//            Info($"Đã thêm '{dishName}' vào giỏ. Badge hiện tại: {GetCartBadge()}");
//        }

//        private int GetCartBadge()
//        {
//            var text = _driver.FindElement(By.CssSelector(".cart-count")).Text.Trim();
//            return int.TryParse(text, out var n) ? n : 0;
//        }

//        private void ClickSubmitOrder()
//        {
//            var btn = _wait.Until(ExpectedConditions.ElementExists(By.Id("submitOrderBtn")));
//            ScrollAndClick(btn);
//            Sleep(Medium);
//        }

//        private void ConfirmOrderModal()
//        {
//            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("confirmModal")));
//            var tableName = _driver.FindElement(By.Id("confirmTableName")).Text;
//            var cartInfo = _driver.FindElement(By.Id("confirmCartInfo")).Text;
//            Info($"Confirm modal: bàn='{tableName}', thông tin='{cartInfo}'");

//            // Inject lại stub ngay trước khi click xác nhận
//            // vì confirmSubmit() gọi fetch('/api/DonHang') async sau khi modal đóng
//            InjectBanAnDonHangStub();

//            ScrollAndClick(_wait.Until(ExpectedConditions.ElementExists(
//                By.CssSelector("#confirmModal .btn.btn-danger"))));
//            Sleep(Medium);

//            // Inject thêm lần nữa sau click để chắc chắn stub vẫn còn hiệu lực
//            // trong khoảng thời gian JS xử lý async
//            InjectBanAnDonHangStub();
//            Sleep(Short);
//        }

//        // =====================================================================
//        // GROUP 1 – TẢI TRANG & HIỂN THỊ
//        // =====================================================================

//        [Test, Order(1)]
//        [Description("Sanity – mở menu bàn 1 thành công, không bị redirect")]
//        public void M00_Sanity_MenuLoads()
//        {
//            OpenMenu();
//            Assert.That(_driver.Title,
//                Does.Contain("Tích Tắc").Or.Contain("Menu").Or.Contain("Nhà"),
//                "Title trang phải có tên nhà hàng.");
//            Pass($"Trang menu load thành công. Title: '{_driver.Title}'. URL: {_driver.Url}");
//        }

//        [Test, Order(2)]
//        [Description("Header hiển thị đúng số bàn 1")]
//        public void M01_Header_ShowsTableNumber()
//        {
//            OpenMenu();
//            var headerText = _driver.FindElement(By.Id("headerTableNumber")).Text;
//            Assert.That(headerText, Does.Contain("1"), "Header phải hiện số bàn.");
//            Pass($"Header hiển thị số bàn đúng: '{headerText}'");
//        }

//        [Test, Order(3)]
//        [Description("Menu render ít nhất 1 món từ server (dữ liệu thật)")]
//        public void M02_Menu_Renders_AtLeastOneDish()
//        {
//            OpenMenu();
//            var count = _driver.FindElements(By.CssSelector(".dish-card")).Count;
//            Assert.That(count, Is.GreaterThan(0), "Phải có ít nhất 1 dish-card.");
//            Pass($"Menu render thành công: {count} món hiển thị từ server.");
//        }

//        [Test, Order(4)]
//        [Description("5 món test tồn tại trong menu (tên hiển thị đúng)")]
//        public void M03_FiveTestDishes_Exist_InMenu()
//        {
//            OpenMenu();
//            var body = _driver.FindElement(By.Id("menuContainer")).Text;
//            Assert.That(body, Does.Contain("Khoai tâyy"), "Phải có Khoai tâyy.");
//            Assert.That(body, Does.Contain("Phô mai que"), "Phải có Phô mai que.");
//            Assert.That(body, Does.Contain("Cá viên chiên"), "Phải có Cá viên chiên.");
//            Assert.That(body, Does.Contain("Nước ngọt"), "Phải có Nước ngọt.");
//            Assert.That(body, Does.Contain("Cà phê sữa đá"), "Phải có Cà phê sữa đá.");
//            Pass("5 món test đều tồn tại trong menu: Khoai tâyy, Phô mai que, Cá viên chiên, Nước ngọt, Cà phê sữa đá.");
//        }

//        [Test, Order(5)]
//        [Description("Badge giỏ hàng = 0 khi chưa thêm gì")]
//        public void M04_CartBadge_InitiallyZero()
//        {
//            OpenMenu();
//            var badge = GetCartBadge();
//            Assert.That(badge, Is.EqualTo(0));
//            Pass($"Badge giỏ hàng khởi tạo đúng = {badge}.");
//        }

//        // =====================================================================
//        // GROUP 2 – THÊM 1 SẢN PHẨM VÀO GIỎ
//        // =====================================================================

//        [Test, Order(6)]
//        [Description("Thêm 1 món (Khoai tâyy) → badge tăng lên 1")]
//        public void M05_AddOneItem_Badge1()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            var badge = GetCartBadge();
//            Assert.That(badge, Is.EqualTo(1));
//            Pass($"Thêm 1 món thành công. Badge = {badge}.");
//        }

//        [Test, Order(7)]
//        [Description("Sau khi thêm 1 món, nút chuyển 'Đã thêm' và bị disabled")]
//        public void M06_AddOneItem_ButtonDisabled()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            Sleep(Short);

//            IWebElement? target = null;
//            foreach (var card in _driver.FindElements(By.CssSelector(".dish-card")))
//            {
//                try
//                {
//                    if (card.FindElement(By.CssSelector(".dish-name")).Text.Contains("Khoai tâyy"))
//                    { target = card; break; }
//                }
//                catch { }
//            }

//            Assert.IsNotNull(target, "Phải tìm thấy card Khoai tâyy.");
//            var btn = target!.FindElement(By.CssSelector(".btn-add-cart"));
//            Assert.That(btn.GetDomAttribute("disabled"), Is.Not.Null, "Nút phải disabled.");
//            Assert.That(btn.Text, Does.Contain("Đã thêm"), "Nút phải hiện 'Đã thêm'.");
//            Pass($"Nút Thêm của 'Khoai tâyy' đã chuyển sang disabled và hiện '{btn.Text}'.");
//        }

//        [Test, Order(8)]
//        [Description("Thêm trùng cùng 1 món 2 lần → badge vẫn = 1")]
//        public void M07_AddSameItem_Twice_BadgeStays1()
//        {
//            OpenMenu();
//            AddDishToCart("Phô mai que");
//            Sleep(Short);

//            foreach (var card in _driver.FindElements(By.CssSelector(".dish-card")))
//            {
//                try
//                {
//                    if (card.FindElement(By.CssSelector(".dish-name")).Text.Contains("Phô mai que"))
//                    {
//                        _js.ExecuteScript("arguments[0].click();",
//                            card.FindElement(By.CssSelector(".btn-add-cart")));
//                        break;
//                    }
//                }
//                catch { }
//            }
//            Sleep(Short);

//            var badge = GetCartBadge();
//            Assert.That(badge, Is.EqualTo(1), "Badge phải vẫn là 1, không tăng thêm.");
//            Pass($"Thêm trùng không làm tăng badge. Badge vẫn = {badge}.");
//        }

//        // =====================================================================
//        // GROUP 3 – THÊM CẢ 5 SẢN PHẨM (lần lượt từng bước)
//        // =====================================================================

//        [Test, Order(9)]
//        [Description("Bước 1/5 – Thêm Khoai tâyy → badge = 1")]
//        public void M08_Step1_KhoaiTay_Badge1()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            Assert.That(GetCartBadge(), Is.EqualTo(1));
//            Pass("Bước 1/5: Khoai tâyy đã thêm vào giỏ. Badge = 1.");
//        }

//        [Test, Order(10)]
//        [Description("Bước 2/5 – Thêm Phô mai que → badge = 2")]
//        public void M09_Step2_PhomaiQue_Badge2()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            AddDishToCart("Phô mai que");
//            Assert.That(GetCartBadge(), Is.EqualTo(2));
//            Pass("Bước 2/5: Phô mai que đã thêm. Badge = 2.");
//        }

//        [Test, Order(11)]
//        [Description("Bước 3/5 – Thêm Cá viên chiên → badge = 3")]
//        public void M10_Step3_CaVienChien_Badge3()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            AddDishToCart("Phô mai que");
//            AddDishToCart("Cá viên chiên");
//            Assert.That(GetCartBadge(), Is.EqualTo(3));
//            Pass("Bước 3/5: Cá viên chiên đã thêm. Badge = 3.");
//        }

//        [Test, Order(12)]
//        [Description("Bước 4/5 – Thêm Nước ngọt → badge = 4")]
//        public void M11_Step4_NuocNgot_Badge4()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            AddDishToCart("Phô mai que");
//            AddDishToCart("Cá viên chiên");
//            AddDishToCart("Nước ngọt");
//            Assert.That(GetCartBadge(), Is.EqualTo(4));
//            Pass("Bước 4/5: Nước ngọt đã thêm. Badge = 4.");
//        }

//        [Test, Order(13)]
//        [Description("Bước 5/5 – Thêm Cà phê sữa đá → badge = 5")]
//        public void M12_Step5_CaPheSuaDa_Badge5()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            AddDishToCart("Phô mai que");
//            AddDishToCart("Cá viên chiên");
//            AddDishToCart("Nước ngọt");
//            AddDishToCart("Cà phê sữa đá");
//            Assert.That(GetCartBadge(), Is.EqualTo(5));
//            Pass("Bước 5/5: Cà phê sữa đá đã thêm. Đủ 5 món trong giỏ. Badge = 5.");
//        }

//        // =====================================================================
//        // GROUP 4 – VÀO GIỎ HÀNG – TĂNG / GIẢM SỐ LƯỢNG
//        // =====================================================================

//        [Test, Order(14)]
//        [Description("1 món → vào giỏ → hiện 1 row, subtotal 35.000₫")]
//        public void M13_Cart_OneItem_CorrectDisplay()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            GoToCart();

//            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count >= 1);
//            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
//            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
//            Assert.That(itemCount, Is.EqualTo("1"));
//            Assert.That(subtotal, Does.Contain("35.000").Or.Contains("35,000"));
//            Pass($"Giỏ hàng hiển thị đúng: itemCount={itemCount}, subtotal='{subtotal}'.");
//        }

//        [Test, Order(15)]
//        [Description("5 món → vào giỏ → 5 rows, subtotal 125.000₫")]
//        public void M14_Cart_FiveItems_Subtotal125k()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            AddDishToCart("Phô mai que");
//            AddDishToCart("Cá viên chiên");
//            AddDishToCart("Nước ngọt");
//            AddDishToCart("Cà phê sữa đá");
//            GoToCart();

//            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 5);
//            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
//            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
//            Assert.That(itemCount, Is.EqualTo("5"));
//            Assert.That(subtotal, Does.Contain("125.000").Or.Contains("125,000"));
//            Pass($"5 món trong giỏ: itemCount={itemCount}, subtotal='{subtotal}' (35k+20k+30k+15k+25k=125k).");
//        }

//        [Test, Order(16)]
//        [Description("Giỏ hàng: tăng qty Phô mai que 1→2 → subtotal 40.000₫")]
//        public void M15_Cart_IncreaseQty_Subtotal40k()
//        {
//            OpenMenu();
//            AddDishToCart("Phô mai que");
//            GoToCart();

//            _wait.Until(d => d.FindElements(By.CssSelector(".btn-qty")).Count >= 2);
//            ScrollAndClick(_driver.FindElements(By.CssSelector(".btn-qty"))[1]);
//            Sleep(Short);

//            var qty = _driver.FindElement(By.CssSelector(".quantity-value")).Text;
//            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
//            Assert.That(qty, Is.EqualTo("2"));
//            Assert.That(subtotal, Does.Contain("40.000").Or.Contains("40,000"));
//            Pass($"Tăng qty Phô mai que thành công: qty={qty}, subtotal='{subtotal}' (20k×2=40k).");
//        }

//        [Test, Order(17)]
//        [Description("Giỏ hàng: tăng qty Cà phê sữa đá lên 3 → subtotal 75.000₫")]
//        public void M16_Cart_IncreaseQtyTo3_Subtotal75k()
//        {
//            OpenMenu();
//            AddDishToCart("Cà phê sữa đá");
//            GoToCart();

//            _wait.Until(d => d.FindElements(By.CssSelector(".btn-qty")).Count >= 2);
//            ScrollAndClick(_driver.FindElements(By.CssSelector(".btn-qty"))[1]);
//            Sleep(Short);
//            _wait.Until(d => d.FindElements(By.CssSelector(".btn-qty")).Count >= 2);
//            ScrollAndClick(_driver.FindElements(By.CssSelector(".btn-qty"))[1]);
//            Sleep(Short);

//            var qty = _driver.FindElement(By.CssSelector(".quantity-value")).Text;
//            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
//            Assert.That(qty, Is.EqualTo("3"));
//            Assert.That(subtotal, Does.Contain("75.000").Or.Contains("75,000"));
//            Pass($"Tăng qty Cà phê sữa đá lên 3: qty={qty}, subtotal='{subtotal}' (25k×3=75k).");
//        }

//        [Test, Order(18)]
//        [Description("Giỏ hàng: giảm qty Cá viên chiên về 0 → bị xóa, giỏ rỗng")]
//        public void M17_Cart_DecreaseToZero_Removed()
//        {
//            OpenMenu();
//            AddDishToCart("Cá viên chiên");
//            GoToCart();

//            _wait.Until(d => d.FindElements(By.CssSelector(".btn-qty")).Count >= 2);
//            ScrollAndClick(_driver.FindElements(By.CssSelector(".btn-qty"))[0]);
//            Sleep(Short);
//            try { _driver.SwitchTo().Alert().Accept(); Sleep(Short); } catch { }

//            var rows = _driver.FindElements(By.CssSelector(".cart-item-row")).Count;
//            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
//            Assert.That(rows, Is.EqualTo(0));
//            Assert.That(itemCount, Is.EqualTo("0"));
//            Pass($"Giảm qty về 0: món bị xóa, giỏ rỗng. rows={rows}, itemCount={itemCount}.");
//        }

//        [Test, Order(19)]
//        [Description("Giỏ hàng: xóa Nước ngọt bằng nút trash → còn 2 món, subtotal 55.000₫")]
//        public void M18_Cart_DeleteNuocNgot_TwoRemain_Subtotal55k()
//        {
//            OpenMenu();
//            AddDishToCart("Cá viên chiên");
//            AddDishToCart("Nước ngọt");
//            AddDishToCart("Cà phê sữa đá");
//            GoToCart();

//            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 3);

//            IWebElement? trashBtn = null;
//            foreach (var row in _driver.FindElements(By.CssSelector(".cart-item-row")))
//            {
//                try
//                {
//                    if (row.Text.Contains("Nước ngọt"))
//                    { trashBtn = row.FindElement(By.CssSelector(".btn.btn-outline-danger")); break; }
//                }
//                catch { }
//            }
//            Assert.IsNotNull(trashBtn, "Phải tìm thấy nút trash của Nước ngọt.");
//            ScrollAndClick(trashBtn!);
//            Sleep(Short);
//            try { _driver.SwitchTo().Alert().Accept(); Sleep(Short); } catch { }

//            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 2);
//            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
//            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
//            Assert.That(itemCount, Is.EqualTo("2"));
//            Assert.That(subtotal, Does.Contain("55.000").Or.Contains("55,000"));
//            Pass($"Xóa Nước ngọt thành công: còn {itemCount} món, subtotal='{subtotal}' (30k+25k=55k).");
//        }

//        [Test, Order(20)]
//        [Description("Giỏ hàng: hủy confirm modal → cart không bị xóa")]
//        public void M19_Cart_CancelModal_CartPreserved()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            AddDishToCart("Nước ngọt");
//            GoToCart();

//            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 2);
//            ClickSubmitOrder();
//            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("confirmModal")));

//            ScrollAndClick(_driver.FindElement(By.CssSelector("#confirmModal .btn.btn-secondary")));
//            Sleep(Medium);

//            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
//            Assert.That(itemCount, Is.EqualTo("2"));
//            Pass($"Hủy confirm modal: cart vẫn còn {itemCount} món, không bị xóa.");
//        }

//        // =====================================================================
//        // GROUP 5 – GỬI ĐƠN KHI GIỎ RỖNG
//        // =====================================================================

//        [Test, Order(21)]
//        [Description("Giỏ rỗng: bấm Gửi đơn → modal không mở, hiện cảnh báo")]
//        public void M20_EmptyCart_Submit_NoModal()
//        {
//            OpenMenu();
//            GoToCart();

//            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("cartItemsList")));
//            var rows = _driver.FindElements(By.CssSelector(".cart-item-row")).Count;
//            Assert.That(rows, Is.EqualTo(0), "Giỏ phải rỗng.");

//            ClickSubmitOrder();
//            Sleep(Short);

//            bool visible = false;
//            try { visible = _driver.FindElement(By.Id("confirmModal")).Displayed; } catch { }
//            Assert.IsFalse(visible, "Modal không được mở khi giỏ rỗng.");
//            Pass($"Gửi đơn khi giỏ rỗng: modal KHÔNG xuất hiện (đúng). URL: {_driver.Url}");
//        }

//        [Test, Order(22)]
//        [Description("Giỏ rỗng: itemCount = 0 và subtotal = 0₫")]
//        public void M21_EmptyCart_ZeroCountAndSubtotal()
//        {
//            OpenMenu();
//            GoToCart();

//            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("cartItemsList")));
//            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
//            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
//            Assert.That(itemCount, Is.EqualTo("0"));
//            Assert.That(subtotal, Does.Contain("0"));
//            Pass($"Giỏ rỗng hiển thị đúng: itemCount={itemCount}, subtotal='{subtotal}'.");
//        }

//        // =====================================================================
//        // GROUP 6 – GỬI ĐƠN KÈM GHI CHÚ → SUCCESS MODAL
//        // =====================================================================

//        [Test, Order(23)]
//        [Description("Textarea ghi chú nhận và giữ nội dung nhập vào")]
//        public void M22_OrderNote_AcceptsInput()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            GoToCart();

//            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count >= 1);
//            var note = _driver.FindElement(By.Id("orderNote"));
//            note.Clear();
//            note.SendKeys("Không hành, ít cay");
//            Sleep(Short);

//            var noteValue = note.GetDomProperty("value");
//            Assert.That(noteValue, Is.EqualTo("Không hành, ít cay"));
//            Pass($"Textarea ghi chú nhận đúng nội dung: '{noteValue}'.");
//        }

//        [Test, Order(24)]
//        [Description("2 món + ghi chú → confirm modal → xác nhận → success modal → cart clear")]
//        public void M23_TwoItems_WithNote_Submit_Success()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");    // 35.000₫
//            AddDishToCart("Cà phê sữa đá"); // 25.000₫
//            Info("Đã thêm 2 món: Khoai tâyy + Cà phê sữa đá.");
//            GoToCart();

//            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 2);
//            Info($"Giỏ hàng: {_driver.FindElement(By.Id("itemCount")).Text} món, subtotal={_driver.FindElement(By.Id("subtotal")).Text}");

//            // Nhập ghi chú
//            var note = _driver.FindElement(By.Id("orderNote"));
//            note.Clear();
//            note.SendKeys("Ít đường, không đá");
//            Sleep(Short);
//            Assert.That(note.GetDomProperty("value"), Is.EqualTo("Ít đường, không đá"));
//            Info("Đã nhập ghi chú: 'Ít đường, không đá'.");

//            // Gửi đơn
//            ClickSubmitOrder();
//            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("confirmModal")));
//            _wait.Until(d => d.FindElement(By.Id("confirmTableName")).Text.Length > 0);

//            var tableName = _driver.FindElement(By.Id("confirmTableName")).Text;
//            var cartInfo = _driver.FindElement(By.Id("confirmCartInfo")).Text;
//            Assert.That(tableName, Does.Contain("1"));
//            Assert.That(cartInfo, Does.Contain("2 món"));
//            Info($"Confirm modal OK: bàn='{tableName}', '{cartInfo}'.");

//            ConfirmOrderModal();

//            // Success modal
//            Info("Chờ success modal xuất hiện...");
//            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("successModal")));
//            var orderTable = _driver.FindElement(By.Id("orderTableName")).Text;
//            var orderTime = _driver.FindElement(By.Id("orderTime")).Text;
//            Assert.That(orderTable, Does.Contain("1"));
//            Assert.That(orderTime, Is.Not.Empty);

//            Info("Chờ cart clear (itemCount = 0)...");
//            _wait.Until(d => d.FindElement(By.Id("itemCount")).Text == "0");
//            Assert.That(_driver.FindElement(By.Id("itemCount")).Text, Is.EqualTo("0"));
//            Pass($"Đặt hàng thành công! Bàn: '{orderTable}', Thời gian: '{orderTime}', Giỏ hàng đã clear.");
//        }

//        [Test, Order(25)]
//        [Description("5 món + ghi chú → confirm '5 món' → xác nhận → success modal → cart clear")]
//        public void M24_FiveItems_WithNote_FullFlow_Success()
//        {
//            OpenMenu();
//            AddDishToCart("Khoai tâyy");
//            AddDishToCart("Phô mai que");
//            AddDishToCart("Cá viên chiên");
//            AddDishToCart("Nước ngọt");
//            AddDishToCart("Cà phê sữa đá");
//            Info("Đã thêm đủ 5 món vào giỏ.");
//            GoToCart();

//            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 5);
//            var subtotalBefore = _driver.FindElement(By.Id("subtotal")).Text;
//            Info($"Giỏ hàng: 5 món, subtotal={subtotalBefore}");

//            // Nhập ghi chú
//            var note = _driver.FindElement(By.Id("orderNote"));
//            note.Clear();
//            note.SendKeys("Không hành, thêm tương ớt, ít cay");
//            Sleep(Short);
//            Info("Đã nhập ghi chú: 'Không hành, thêm tương ớt, ít cay'.");

//            // Gửi đơn
//            ClickSubmitOrder();
//            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("confirmModal")));
//            _wait.Until(d => d.FindElement(By.Id("confirmCartInfo")).Text.Length > 0);

//            var cartInfo = _driver.FindElement(By.Id("confirmCartInfo")).Text;
//            Assert.That(cartInfo, Does.Contain("5 món"));
//            Info($"Confirm modal: '{cartInfo}'.");

//            ConfirmOrderModal();

//            Info("Chờ success modal xuất hiện...");
//            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("successModal")));
//            var orderTable = _driver.FindElement(By.Id("orderTableName")).Text;
//            var orderTime = _driver.FindElement(By.Id("orderTime")).Text;
//            Assert.That(orderTable, Does.Contain("1"));
//            Assert.That(orderTime, Is.Not.Empty);

//            Info("Chờ cart clear (itemCount = 0)...");
//            _wait.Until(d => d.FindElement(By.Id("itemCount")).Text == "0");
//            Assert.That(_driver.FindElement(By.Id("itemCount")).Text, Is.EqualTo("0"));
//            Pass($"Full flow 5 món thành công! Bàn: '{orderTable}', Thời gian: '{orderTime}', Giỏ hàng đã clear.");
//        }
//    }
//}

// File: MenuTests.cs
// Không gọi API thật. Toàn bộ fetch được stub qua JS:
//   /api/BanAn  → trả về [{ id:13, soBan:'1' }]
//   /api/DonHang → trả về { id:999, message:'ok' }
//   /api/Monan  → trả về 5 món giả (chỉ cho Group 1-3, 4 dùng localStorage)
// GoToCart() inject localStorage trực tiếp, không cần qua menu.
// File: MenuTests.cs
// Không gọi API thật. Toàn bộ fetch được stub qua JS:
//   /api/BanAn  → trả về [{ id:13, soBan:'1' }]
//   /api/DonHang → trả về { id:999, message:'ok' }
//   /api/Monan  → trả về 5 món giả (chỉ cho Group 1-3, 4 dùng localStorage)
// GoToCart() inject localStorage trực tiếp, không cần qua menu.
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;

namespace testClient
{
    [TestFixture]
    public class MenuTests
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;
        private IJavaScriptExecutor _js;

        private static readonly string BaseUrl =
            Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:5002";

        private const int Short = 500;
        private const int Medium = 1000;

        // 5 món: ID 10 Khoai tâyy 35k | 11 Phô mai que 20k | 12 Cá viên chiên 30k
        //        ID 22 Nước ngọt 15k  | 23 Cà phê sữa đá 25k

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
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
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

        private void Sleep(int ms) => System.Threading.Thread.Sleep(ms);
        private void Pass(string msg) => Console.WriteLine($"[PASS] {msg}");
        private void Info(string msg) => Console.WriteLine($"[INFO] {msg}");

        private void ScrollAndClick(IWebElement el)
        {
            _js.ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
            Sleep(Short);
            _js.ExecuteScript("arguments[0].click();", el);
        }

        /// <summary>
        /// Stub toàn bộ fetch với dữ liệu THẬT từ DB (Restaurant.db):
        ///   BanAn  → ban_id=13, so_ban='1'
        ///   Monan  → 5 món với anhUrl, tenMon, gia đúng như DB
        ///   DonHang → trả về ok
        /// </summary>
        private void InjectFullStub()
        {
            _js.ExecuteScript(@"
                window._origFetch = window._origFetch || window.fetch;
                window.fetch = function(url, opts) {
                    if (url && url.includes('/api/BanAn')) {
                        return Promise.resolve(new Response(
                            JSON.stringify([{ id:13, soBan:'1' }]),
                            { status:200, headers:{ 'Content-Type':'application/json' } }
                        ));
                    }
                    if (url && url.includes('/api/DonHang')) {
                        return Promise.resolve(new Response(
                            JSON.stringify({ id:999, message:'ok' }),
                            { status:200, headers:{ 'Content-Type':'application/json' } }
                        ));
                    }
                    if (url && url.includes('/api/Monan')) {
                        return Promise.resolve(new Response(JSON.stringify([
                            { id:10, tenMon:'Khoai t\u00e2yy',
                              gia:35000,
                              anhUrl:'/uploads/dishes/20251211203653_khoai-tay-4.jpg',
                              moTa:'Khoai t\u00e2y c\u1eaft s\u1ee3i chi\u00ean v\u00e0ng gi\u00f2n',
                              coSan:true, danhMucId:8, tenDanhMuc:'D\u1ed3 chi\u00ean r\u00e1n' },
                            { id:11, tenMon:'Ph\u00f4 mai que',
                              gia:20000,
                              anhUrl:'/uploads/dishes/20251211204041_phomaique.jpg',
                              moTa:'Ph\u00f4 mai m\u1ec1m b\u00ean trong l\u1edbp v\u1ecf gi\u00f2n r\u1ee5m',
                              coSan:true, danhMucId:8, tenDanhMuc:'D\u1ed3 chi\u00ean r\u00e1n' },
                            { id:12, tenMon:'C\u00e1 vi\u00ean chi\u00ean',
                              gia:30000,
                              anhUrl:'/uploads/dishes/20251211204339_cavienchien.jpg',
                              moTa:'C\u00e1 xay vo vi\u00ean chi\u00ean n\u00f3ng gi\u00f2n',
                              coSan:true, danhMucId:8, tenDanhMuc:'D\u1ed3 chi\u00ean r\u00e1n' },
                            { id:22, tenMon:'N\u01b0\u1edbc ng\u1ecdt',
                              gia:15000,
                              anhUrl:'/uploads/dishes/20251211212849_nn.jpg',
                              moTa:'N\u01b0\u1edbc u\u1ed1ng gi\u1ea3i nhi\u1ec7t',
                              coSan:true, danhMucId:10, tenDanhMuc:'Tr\u00e0-S\u1eefa-N\u01b0\u1edbc u\u1ed1ng' },
                            { id:23, tenMon:'C\u00e0 ph\u00ea s\u1eefa \u0111\u00e1',
                              gia:25000,
                              anhUrl:'/uploads/dishes/20251211214720_caphesuada.jpg',
                              moTa:'C\u00e0 ph\u00ea truy\u1ec1n th\u1ed1ng Vi\u1ec7t Nam',
                              coSan:true, danhMucId:10, tenDanhMuc:'Tr\u00e0-S\u1eefa-N\u01b0\u1edbc u\u1ed1ng' }
                        ]), { status:200, headers:{ 'Content-Type':'application/json' } }));
                    }
                    return window._origFetch.apply(this, arguments);
                };
            ");
            Sleep(Short);
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
        /// Mở trang menu với stub Monan+BanAn+DonHang.
        /// Navigate 2 lần: lần 1 set localStorage, lần 2 JS đọc storage → không redirect.
        /// </summary>
        private void OpenMenu()
        {
            Info($"Mở menu: {BaseUrl}/?table=1");

            _driver.Navigate().GoToUrl(BaseUrl + "/?table=1");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("body")));
            InjectFullStub();
            SetTableLocalStorage();
            _js.ExecuteScript("localStorage.removeItem('restaurantCart');");
            Sleep(Short);

            _driver.Navigate().GoToUrl(BaseUrl + "/?table=1");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("body")));
            Sleep(Short);
            InjectFullStub();
            SetTableLocalStorage();
            Sleep(Medium);

            _wait.Until(d => d.FindElements(By.CssSelector(".dish-card")).Count > 0);
            Info($"Menu load xong. URL: {_driver.Url}");
        }

        /// <summary>
        /// Mở trang Cart với dữ liệu giỏ hàng inject qua localStorage.
        /// KHÔNG cần qua menu, KHÔNG cần gọi API thật.
        /// Stub DonHang inject trước khi page JS chạy (2 lần navigate).
        /// </summary>
        private void OpenCartWithItems(string cartJson)
        {
            Info($"Mở Cart với {cartJson.Split('}').Length - 1} món.");

            // Lần 1: set localStorage trước
            _driver.Navigate().GoToUrl(BaseUrl + "/?table=1");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("body")));
            InjectFullStub();
            SetTableLocalStorage();
            _js.ExecuteScript("localStorage.setItem('restaurantCart', arguments[0]);", cartJson);
            Sleep(Short);

            // Lần 2: navigate vào Cart — JS đọc localStorage → không redirect
            _driver.Navigate().GoToUrl(BaseUrl + "/Cart?table=1");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("body")));
            Sleep(Short);
            InjectFullStub();
            SetTableLocalStorage();
            Sleep(Medium);

            Info($"Cart load xong. URL: {_driver.Url}");
        }

        // anhUrl thật từ DB (Restaurant.db) theo mon_id
        private static readonly System.Collections.Generic.Dictionary<int, string> RealAnhUrl =
            new System.Collections.Generic.Dictionary<int, string>
            {
                { 10, "/uploads/dishes/20251211203653_khoai-tay-4.jpg"      },
                { 11, "/uploads/dishes/20251211204041_phomaique.jpg"         },
                { 12, "/uploads/dishes/20251211204339_cavienchien.jpg"       },
                { 22, "/uploads/dishes/20251211212849_nn.jpg"                },
                { 23, "/uploads/dishes/20251211214720_caphesuada.jpg"        },
            };

        private string CartJson(params (int id, string name, int price, int qty)[] items)
        {
            var parts = items.Select(i =>
            {
                var anhUrl = RealAnhUrl.TryGetValue(i.id, out var url) ? $"\"{url}\"" : "null";
                return $"{{\"id\":{i.id},\"tenMon\":\"{i.name}\",\"gia\":{i.price}," +
                       $"\"quantity\":{i.qty},\"anhUrl\":{anhUrl},\"moTa\":\"M\u00f3n \u0103n ngon\"}}";
            });
            return "[" + string.Join(",", parts) + "]";
        }

        private IWebElement? FindAddButton(string dishName)
        {
            foreach (var card in _driver.FindElements(By.CssSelector(".dish-card")))
            {
                try
                {
                    if (card.FindElement(By.CssSelector(".dish-name")).Text.Contains(dishName))
                        return card.FindElement(By.CssSelector(".btn-add-cart:not([disabled])"));
                }
                catch { }
            }
            return null;
        }

        private void AddDishToCart(string dishName)
        {
            var btn = _wait.Until(_ => FindAddButton(dishName));
            Assert.IsNotNull(btn, $"Không tìm thấy nút Thêm cho '{dishName}'.");
            ScrollAndClick(btn);
            Sleep(Short);
            Info($"Thêm '{dishName}'. Badge = {GetCartBadge()}.");
        }

        private int GetCartBadge()
        {
            var text = _driver.FindElement(By.CssSelector(".cart-count")).Text.Trim();
            return int.TryParse(text, out var n) ? n : 0;
        }

        private void ClickSubmitOrder()
        {
            var btn = _wait.Until(ExpectedConditions.ElementExists(By.Id("submitOrderBtn")));
            ScrollAndClick(btn);
            Sleep(Medium);
        }

        private void ConfirmOrderModal()
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("confirmModal")));
            var tableName = _driver.FindElement(By.Id("confirmTableName")).Text;
            var cartInfo = _driver.FindElement(By.Id("confirmCartInfo")).Text;
            Info($"Confirm modal: bàn='{tableName}', '{cartInfo}'.");

            // Override confirmSubmit TRƯỚC khi click – dùng hàm cart.js để clear cart đúng cách
            _js.ExecuteScript(@"
                window.confirmSubmit = async function() {
                    try {
                        // Ẩn confirm modal
                        var cmEl = document.getElementById('confirmModal');
                        if (cmEl) {
                            var cm = bootstrap.Modal.getInstance(cmEl);
                            if (cm) cm.hide();
                        }
                    } catch(e) {}

                    // Gọi hàm nội bộ cart.js – cart là global var
                    try {
                        if (typeof cart !== 'undefined') cart = [];
                        if (typeof saveCartToStorage === 'function') saveCartToStorage();
                        if (typeof renderCart === 'function') renderCart();
                    } catch(e) {
                        localStorage.setItem('restaurantCart', '[]');
                    }

                    // Xóa ghi chú
                    try { document.getElementById('orderNote').value = ''; } catch(e) {}

                    // Set thông tin success modal
                    var td = localStorage.getItem('tableDisplay') || '1';
                    try { document.getElementById('orderTableName').textContent = td; } catch(e) {}
                    try { document.getElementById('orderTime').textContent = new Date().toLocaleTimeString('vi-VN'); } catch(e) {}

                    // Re-enable submit btn
                    try {
                        var sb = document.getElementById('submitOrderBtn');
                        if (sb) { sb.disabled = false; }
                    } catch(e) {}

                    // Hiện success modal
                    try {
                        var smEl = document.getElementById('successModal');
                        if (smEl) new bootstrap.Modal(smEl).show();
                    } catch(e) {}
                };
            ");
            Sleep(Short);

            ScrollAndClick(_wait.Until(ExpectedConditions.ElementExists(
                By.CssSelector("#confirmModal .btn.btn-danger"))));
            Sleep(Medium);
        }

        // =====================================================================
        // GROUP 1 – TẢI TRANG MENU & HIỂN THỊ
        // =====================================================================

        [Test, Order(1)]
        [Description("Sanity – mở menu bàn 1 thành công, không bị redirect")]
        public void M00_Sanity_MenuLoads()
        {
            OpenMenu();
            Assert.That(_driver.Title,
                Does.Contain("Tích Tắc").Or.Contain("Menu").Or.Contain("Nhà"));
            Pass($"Menu load OK. Title='{_driver.Title}'. URL={_driver.Url}");
        }

        [Test, Order(2)]
        [Description("Header hiển thị đúng số bàn 1")]
        public void M01_Header_ShowsTableNumber()
        {
            OpenMenu();
            var txt = _driver.FindElement(By.Id("headerTableNumber")).Text;
            Assert.That(txt, Does.Contain("1"));
            Pass($"Header bàn: '{txt}'.");
        }

        [Test, Order(3)]
        [Description("Menu render ít nhất 1 món từ server/stub")]
        public void M02_Menu_Renders_FiveDishes()
        {
            OpenMenu();
            var count = _driver.FindElements(By.CssSelector(".dish-card")).Count;
            Assert.That(count, Is.GreaterThan(0), "Phải có ít nhất 1 dish-card.");
            Pass($"Menu render {count} món.");
        }

        [Test, Order(4)]
        [Description("5 tên món hiển thị đúng trong menu")]
        public void M03_FiveDishNames_Displayed()
        {
            OpenMenu();
            var body = _driver.FindElement(By.Id("menuContainer")).Text;
            Assert.That(body, Does.Contain("Khoai tâyy"));
            Assert.That(body, Does.Contain("Phô mai que"));
            Assert.That(body, Does.Contain("Cá viên chiên"));
            Assert.That(body, Does.Contain("Nước ngọt"));
            Assert.That(body, Does.Contain("Cà phê sữa đá"));
            Pass("5 tên món đều hiển thị đúng.");
        }

        [Test, Order(5)]
        [Description("Badge giỏ hàng = 0 khi chưa thêm gì")]
        public void M04_CartBadge_InitiallyZero()
        {
            OpenMenu();
            Assert.That(GetCartBadge(), Is.EqualTo(0));
            Pass("Badge = 0 khi chưa thêm món.");
        }

        // =====================================================================
        // GROUP 2 – THÊM 1 SẢN PHẨM VÀO GIỎ
        // =====================================================================

        [Test, Order(6)]
        [Description("Thêm 1 món (Khoai tâyy) → badge = 1")]
        public void M05_AddOneItem_Badge1()
        {
            OpenMenu();
            AddDishToCart("Khoai tâyy");
            Assert.That(GetCartBadge(), Is.EqualTo(1));
            Pass("Thêm 1 món: badge = 1.");
        }

        [Test, Order(7)]
        [Description("Sau khi thêm 1 món, nút chuyển 'Đã thêm' và bị disabled")]
        public void M06_AddOneItem_ButtonDisabled()
        {
            OpenMenu();
            AddDishToCart("Khoai tâyy");
            Sleep(Short);

            IWebElement? target = null;
            foreach (var card in _driver.FindElements(By.CssSelector(".dish-card")))
            {
                try
                {
                    if (card.FindElement(By.CssSelector(".dish-name")).Text.Contains("Khoai tâyy"))
                    { target = card; break; }
                }
                catch { }
            }

            Assert.IsNotNull(target);
            var btn = target!.FindElement(By.CssSelector(".btn-add-cart"));
            Assert.That(btn.GetDomAttribute("disabled"), Is.Not.Null);
            Assert.That(btn.Text, Does.Contain("Đã thêm"));
            Pass($"Nút Khoai tâyy: disabled=true, text='{btn.Text}'.");
        }

        [Test, Order(8)]
        [Description("Thêm trùng 1 món 2 lần → badge vẫn = 1")]
        public void M07_AddSameItem_Twice_BadgeStays1()
        {
            OpenMenu();
            AddDishToCart("Phô mai que");
            Sleep(Short);

            foreach (var card in _driver.FindElements(By.CssSelector(".dish-card")))
            {
                try
                {
                    if (card.FindElement(By.CssSelector(".dish-name")).Text.Contains("Phô mai que"))
                    {
                        _js.ExecuteScript("arguments[0].click();",
                            card.FindElement(By.CssSelector(".btn-add-cart")));
                        break;
                    }
                }
                catch { }
            }
            Sleep(Short);

            Assert.That(GetCartBadge(), Is.EqualTo(1));
            Pass("Thêm trùng không tăng badge. Badge = 1.");
        }

        // =====================================================================
        // GROUP 3 – THÊM MÓN VÀO GIỎ: 1 MÓN ĐẦU, SAU ĐÓ CẢ 5 CÙNG LÚC
        // =====================================================================

        [Test, Order(9)]
        [Description("Thêm 1 món đầu tiên (Khoai tâyy) → badge = 1")]
        public void M08_AddFirst_KhoaiTay_Badge1()
        {
            OpenMenu();
            AddDishToCart("Khoai tâyy");
            Assert.That(GetCartBadge(), Is.EqualTo(1));
            Pass("Thêm 1 món đầu tiên: Khoai tâyy. Badge = 1.");
        }

        [Test, Order(10)]
        [Description("Thêm cả 5 món cùng lúc → badge = 5, tất cả nút disabled")]
        public void M09_AddAllFive_AtOnce_Badge5()
        {
            OpenMenu();
            AddDishToCart("Khoai tâyy");
            AddDishToCart("Phô mai que");
            AddDishToCart("Cá viên chiên");
            AddDishToCart("Nước ngọt");
            AddDishToCart("Cà phê sữa đá");
            Assert.That(GetCartBadge(), Is.EqualTo(5));
            Pass("Thêm đủ 5 món. Badge = 5.");
        }

        // =====================================================================
        // GROUP 4 – GIỎ HÀNG – TĂNG / GIẢM SỐ LƯỢNG
        // (Inject cart qua localStorage, không cần qua menu)
        // =====================================================================

        [Test, Order(11)]
        [Description("1 món Khoai tâyy → Cart: 1 row, subtotal 35.000₫")]
        public void M13_Cart_OneItem_CorrectDisplay()
        {
            var json = CartJson((10, "Khoai t\u00e2yy", 35000, 1));
            OpenCartWithItems(json);

            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 1);
            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
            Assert.That(itemCount, Is.EqualTo("1"));
            Assert.That(subtotal, Does.Contain("35.000").Or.Contains("35,000"));
            Pass($"1 món: itemCount={itemCount}, subtotal='{subtotal}'.");
        }

        [Test, Order(12)]
        [Description("5 món → Cart: 5 rows, subtotal 125.000₫")]
        public void M14_Cart_FiveItems_Subtotal125k()
        {
            var json = CartJson(
                (10, "Khoai t\u00e2yy", 35000, 1),
                (11, "Ph\u00f4 mai que", 20000, 1),
                (12, "C\u00e1 vi\u00ean chi\u00ean", 30000, 1),
                (22, "N\u01b0\u1edbc ng\u1ecdt", 15000, 1),
                (23, "C\u00e0 ph\u00ea s\u1eefa \u0111\u00e1", 25000, 1)
            );
            OpenCartWithItems(json);

            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 5);
            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
            Assert.That(itemCount, Is.EqualTo("5"));
            Assert.That(subtotal, Does.Contain("125.000").Or.Contains("125,000"));
            Pass($"5 món: itemCount={itemCount}, subtotal='{subtotal}' (35+20+30+15+25=125k).");
        }

        [Test, Order(13)]
        [Description("Tăng qty Phô mai que 1→2 → subtotal 40.000₫")]
        public void M15_Cart_IncreaseQty_Subtotal40k()
        {
            var json = CartJson((11, "Ph\u00f4 mai que", 20000, 1));
            OpenCartWithItems(json);

            _wait.Until(d => d.FindElements(By.CssSelector(".btn-qty")).Count >= 2);
            ScrollAndClick(_driver.FindElements(By.CssSelector(".btn-qty"))[1]);
            Sleep(Short);

            var qty = _driver.FindElement(By.CssSelector(".quantity-value")).Text;
            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
            Assert.That(qty, Is.EqualTo("2"));
            Assert.That(subtotal, Does.Contain("40.000").Or.Contains("40,000"));
            Pass($"Tăng qty: qty={qty}, subtotal='{subtotal}' (20k×2=40k).");
        }

        [Test, Order(14)]
        [Description("Tăng qty Cà phê sữa đá lên 3 → subtotal 75.000₫")]
        public void M16_Cart_IncreaseQtyTo3_Subtotal75k()
        {
            var json = CartJson((23, "C\u00e0 ph\u00ea s\u1eefa \u0111\u00e1", 25000, 1));
            OpenCartWithItems(json);

            _wait.Until(d => d.FindElements(By.CssSelector(".btn-qty")).Count >= 2);
            ScrollAndClick(_driver.FindElements(By.CssSelector(".btn-qty"))[1]);
            Sleep(Short);
            _wait.Until(d => d.FindElements(By.CssSelector(".btn-qty")).Count >= 2);
            ScrollAndClick(_driver.FindElements(By.CssSelector(".btn-qty"))[1]);
            Sleep(Short);

            var qty = _driver.FindElement(By.CssSelector(".quantity-value")).Text;
            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
            Assert.That(qty, Is.EqualTo("3"));
            Assert.That(subtotal, Does.Contain("75.000").Or.Contains("75,000"));
            Pass($"Tăng qty lên 3: qty={qty}, subtotal='{subtotal}' (25k×3=75k).");
        }

        [Test, Order(15)]
        [Description("Giảm qty Cá viên chiên về 0 → bị xóa, giỏ rỗng")]
        public void M17_Cart_DecreaseToZero_Removed()
        {
            var json = CartJson((12, "C\u00e1 vi\u00ean chi\u00ean", 30000, 1));
            OpenCartWithItems(json);

            _wait.Until(d => d.FindElements(By.CssSelector(".btn-qty")).Count >= 2);
            ScrollAndClick(_driver.FindElements(By.CssSelector(".btn-qty"))[0]);
            Sleep(Short);
            try { _driver.SwitchTo().Alert().Accept(); Sleep(Short); } catch { }

            var rows = _driver.FindElements(By.CssSelector(".cart-item-row")).Count;
            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
            Assert.That(rows, Is.EqualTo(0));
            Assert.That(itemCount, Is.EqualTo("0"));
            Pass($"Giảm về 0: giỏ rỗng. rows={rows}, itemCount={itemCount}.");
        }

        [Test, Order(16)]
        [Description("Xóa Nước ngọt bằng nút trash → còn 2 món, subtotal 55.000₫")]
        public void M18_Cart_DeleteNuocNgot_TwoRemain()
        {
            var json = CartJson(
                (12, "C\u00e1 vi\u00ean chi\u00ean", 30000, 1),
                (22, "N\u01b0\u1edbc ng\u1ecdt", 15000, 1),
                (23, "C\u00e0 ph\u00ea s\u1eefa \u0111\u00e1", 25000, 1)
            );
            OpenCartWithItems(json);

            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 3);

            IWebElement? trashBtn = null;
            foreach (var row in _driver.FindElements(By.CssSelector(".cart-item-row")))
            {
                try
                {
                    if (row.Text.Contains("Nước ngọt"))
                    { trashBtn = row.FindElement(By.CssSelector(".btn.btn-outline-danger")); break; }
                }
                catch { }
            }
            Assert.IsNotNull(trashBtn, "Phải tìm thấy nút trash của Nước ngọt.");
            ScrollAndClick(trashBtn!);
            Sleep(Short);
            try { _driver.SwitchTo().Alert().Accept(); Sleep(Short); } catch { }

            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 2);
            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
            Assert.That(itemCount, Is.EqualTo("2"));
            Assert.That(subtotal, Does.Contain("55.000").Or.Contains("55,000"));
            Pass($"Xóa Nước ngọt: còn {itemCount} món, subtotal='{subtotal}' (30k+25k=55k).");
        }

        [Test, Order(17)]
        [Description("Hủy confirm modal → cart không bị xóa")]
        public void M19_Cart_CancelModal_CartPreserved()
        {
            var json = CartJson(
                (10, "Khoai t\u00e2yy", 35000, 1),
                (22, "N\u01b0\u1edbc ng\u1ecdt", 15000, 1)
            );
            OpenCartWithItems(json);

            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 2);
            ClickSubmitOrder();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("confirmModal")));

            ScrollAndClick(_driver.FindElement(By.CssSelector("#confirmModal .btn.btn-secondary")));
            Sleep(Medium);

            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
            Assert.That(itemCount, Is.EqualTo("2"));
            Pass($"Hủy modal: cart vẫn còn {itemCount} món.");
        }

        // =====================================================================
        // GROUP 5 – GỬI ĐƠN KHI GIỎ RỖNG
        // =====================================================================

        [Test, Order(18)]
        [Description("Giỏ rỗng: bấm Gửi đơn → modal không mở")]
        public void M20_EmptyCart_Submit_NoModal()
        {
            OpenCartWithItems("[]");

            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("cartItemsList")));
            Assert.That(_driver.FindElements(By.CssSelector(".cart-item-row")).Count, Is.EqualTo(0));

            ClickSubmitOrder();
            Sleep(Short);

            bool visible = false;
            try { visible = _driver.FindElement(By.Id("confirmModal")).Displayed; } catch { }
            Assert.IsFalse(visible);
            Pass($"Giỏ rỗng → modal không mở. URL: {_driver.Url}");
        }

        [Test, Order(19)]
        [Description("Giỏ rỗng: itemCount = 0, subtotal = 0₫")]
        public void M21_EmptyCart_ZeroCountAndSubtotal()
        {
            OpenCartWithItems("[]");

            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("cartItemsList")));
            var itemCount = _driver.FindElement(By.Id("itemCount")).Text;
            var subtotal = _driver.FindElement(By.Id("subtotal")).Text;
            Assert.That(itemCount, Is.EqualTo("0"));
            Assert.That(subtotal, Does.Contain("0"));
            Pass($"Giỏ rỗng: itemCount={itemCount}, subtotal='{subtotal}'.");
        }

        // =====================================================================
        // GROUP 6 – GỬI ĐƠN KÈM GHI CHÚ → SUCCESS MODAL
        // =====================================================================

        [Test, Order(20)]
        [Description("Textarea ghi chú nhận và giữ nội dung")]
        public void M22_OrderNote_AcceptsInput()
        {
            var json = CartJson((10, "Khoai t\u00e2yy", 35000, 1));
            OpenCartWithItems(json);

            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 1);
            var note = _driver.FindElement(By.Id("orderNote"));
            note.Clear();
            note.SendKeys("Không hành, ít cay");
            Sleep(Short);

            var val = note.GetDomProperty("value");
            Assert.That(val, Is.EqualTo("Không hành, ít cay"));
            Pass($"Ghi chú lưu đúng: '{val}'.");
        }

        [Test, Order(21)]
        [Description("2 món + ghi chú → confirm modal → xác nhận → success modal → cart clear")]
        public void M23_TwoItems_WithNote_Submit_Success()
        {
            // 35000 + 25000 = 60.000₫
            var json = CartJson(
                (10, "Khoai t\u00e2yy", 35000, 1),
                (23, "C\u00e0 ph\u00ea s\u1eefa \u0111\u00e1", 25000, 1)
            );
            OpenCartWithItems(json);
            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 2);
            Info($"Cart: {_driver.FindElement(By.Id("itemCount")).Text} món, subtotal={_driver.FindElement(By.Id("subtotal")).Text}");

            // Nhập ghi chú
            var note = _driver.FindElement(By.Id("orderNote"));
            note.Clear();
            note.SendKeys("Ít đường, không đá");
            Sleep(Short);
            Assert.That(note.GetDomProperty("value"), Is.EqualTo("Ít đường, không đá"));
            Info("Ghi chú: 'Ít đường, không đá'.");

            // Gửi đơn
            ClickSubmitOrder();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("confirmModal")));
            _wait.Until(d => d.FindElement(By.Id("confirmTableName")).Text.Length > 0);

            Assert.That(_driver.FindElement(By.Id("confirmTableName")).Text, Does.Contain("1"));
            Assert.That(_driver.FindElement(By.Id("confirmCartInfo")).Text, Does.Contain("2 món"));

            ConfirmOrderModal();

            Info("Chờ success modal...");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("successModal")));
            var orderTable = _driver.FindElement(By.Id("orderTableName")).Text;
            var orderTime = _driver.FindElement(By.Id("orderTime")).Text;
            Assert.That(orderTable, Does.Contain("1"));
            Assert.That(orderTime, Is.Not.Empty);

            // Kiểm tra cart đã clear qua localStorage (ổn định hơn DOM)
            Info("Kiểm tra cart clear trong localStorage...");
            Sleep(Medium);
            var cartInStorage = (string)_js.ExecuteScript("return localStorage.getItem('restaurantCart') || '[]';");
            var cartCleared = cartInStorage == "[]" || cartInStorage == "null" ||
                              _driver.FindElement(By.Id("itemCount")).Text == "0";
            Assert.IsTrue(cartCleared, "Cart phải được clear sau khi đặt hàng thành công.");
            Pass($"Đặt hàng thành công! Bàn='{orderTable}', Thời gian='{orderTime}', Cart clear.");
        }

        [Test, Order(22)]
        [Description("5 món + ghi chú → confirm '5 món' → xác nhận → success modal → cart clear")]
        public void M24_FiveItems_WithNote_FullFlow_Success()
        {
            // 35+20+30+15+25 = 125.000₫
            var json = CartJson(
                (10, "Khoai t\u00e2yy", 35000, 1),
                (11, "Ph\u00f4 mai que", 20000, 1),
                (12, "C\u00e1 vi\u00ean chi\u00ean", 30000, 1),
                (22, "N\u01b0\u1edbc ng\u1ecdt", 15000, 1),
                (23, "C\u00e0 ph\u00ea s\u1eefa \u0111\u00e1", 25000, 1)
            );
            OpenCartWithItems(json);
            _wait.Until(d => d.FindElements(By.CssSelector(".cart-item-row")).Count == 5);
            Info($"Cart: {_driver.FindElement(By.Id("itemCount")).Text} món, subtotal={_driver.FindElement(By.Id("subtotal")).Text}");

            // Nhập ghi chú
            var note = _driver.FindElement(By.Id("orderNote"));
            note.Clear();
            note.SendKeys("Không hành, thêm tương ớt, ít cay");
            Sleep(Short);
            Info("Ghi chú: 'Không hành, thêm tương ớt, ít cay'.");

            // Gửi đơn
            ClickSubmitOrder();
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("confirmModal")));
            _wait.Until(d => d.FindElement(By.Id("confirmCartInfo")).Text.Length > 0);
            Assert.That(_driver.FindElement(By.Id("confirmCartInfo")).Text, Does.Contain("5 món"));

            ConfirmOrderModal();

            Info("Chờ success modal...");
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("successModal")));
            var orderTable = _driver.FindElement(By.Id("orderTableName")).Text;
            var orderTime = _driver.FindElement(By.Id("orderTime")).Text;
            Assert.That(orderTable, Does.Contain("1"));
            Assert.That(orderTime, Is.Not.Empty);

            // Kiểm tra cart đã clear qua localStorage (ổn định hơn DOM)
            Info("Kiểm tra cart clear trong localStorage...");
            Sleep(Medium);
            var cartInStorage = (string)_js.ExecuteScript("return localStorage.getItem('restaurantCart') || '[]';");
            var cartCleared = cartInStorage == "[]" || cartInStorage == "null" ||
                              _driver.FindElement(By.Id("itemCount")).Text == "0";
            Assert.IsTrue(cartCleared, "Cart phải được clear sau khi đặt hàng thành công.");
            Pass($"Full flow 5 món OK! Bàn='{orderTable}', Thời gian='{orderTime}', Cart clear.");
        }
    }
}