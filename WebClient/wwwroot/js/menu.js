// Cấu hình API URL - Server chạy ở port 5137
const API_BASE_URL = "http://localhost:5137";
const API_URL = `${API_BASE_URL}/api/Monan`;
const TABLE_API_URL = `${API_BASE_URL}/api/Banan`;

let cart = [];
let menuData = [];
let currentTableInfo = null; // Lưu thông tin bàn hiện tại

// ✅ Validate và lấy số bàn từ URL
async function validateAndLoadTable() {
  const urlParams = new URLSearchParams(window.location.search);
  const tableNumber = urlParams.get("table");

  // Nếu không có số bàn trong URL, kiểm tra localStorage
  if (!tableNumber) {
    const savedTableNumber = localStorage.getItem("tableNumber");
    
    // Nếu cũng không có trong localStorage -> redirect về trang lỗi
    if (!savedTableNumber) {
      redirectToErrorPage("EMPTY_TABLE", null);
      return false;
    }
    
    // Validate bàn đã lưu trong localStorage
    const isValid = await validateTable(savedTableNumber);
    if (!isValid) {
      // Xóa localStorage và redirect
      localStorage.removeItem("tableNumber");
      localStorage.removeItem("tableId");
      redirectToErrorPage("TABLE_NOT_FOUND", savedTableNumber);
      return false;
    }
    
    // Bàn hợp lệ, hiển thị
    displayTableInfo(savedTableNumber);
    return true;
  }

  // Có số bàn trong URL -> validate với server
  const isValid = await validateTable(tableNumber);
  
  if (!isValid) {
    redirectToErrorPage("TABLE_NOT_FOUND", tableNumber);
    return false;
  }

  // Bàn hợp lệ, lưu và hiển thị
  localStorage.setItem("tableNumber", tableNumber);
  if (currentTableInfo) {
    localStorage.setItem("tableId", currentTableInfo.tableId);
  }
  displayTableInfo(tableNumber);
  return true;
}

// ✅ Validate bàn với API
async function validateTable(tableNumber) {
  try {
    console.log("🔄 Đang validate bàn:", tableNumber);
    
    const response = await fetch(`${TABLE_API_URL}/validate/${encodeURIComponent(tableNumber)}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
      mode: "cors",
    });

    const data = await response.json();
    console.log("📡 Kết quả validate:", data);

    if (response.ok && data.valid) {
      currentTableInfo = data;
      console.log("✅ Bàn hợp lệ:", data);
      return true;
    } else {
      console.warn("❌ Bàn không hợp lệ:", data.message);
      return false;
    }
  } catch (error) {
    console.error("❌ Lỗi khi validate bàn:", error);
    // Nếu không kết nối được API, cho phép tiếp tục (offline mode)
    // Hoặc có thể redirect về trang lỗi tùy theo yêu cầu
    return true; // Cho phép offline mode
  }
}

// ✅ Redirect về trang lỗi
function redirectToErrorPage(errorCode, tableName) {
  const params = new URLSearchParams();
  params.set("code", errorCode);
  if (tableName) {
    params.set("table", tableName);
  }
  
  window.location.href = `/Error/TableError?${params.toString()}`;
}

// ✅ Hiển thị thông tin bàn trên header
function displayTableInfo(tableNumber) {
  const headerTableNumber = document.getElementById("headerTableNumber");
  if (headerTableNumber) {
    headerTableNumber.textContent = `Bàn ${tableNumber}`;
  }
}

// Khởi tạo
document.addEventListener("DOMContentLoaded", async function () {
  // ✅ Validate bàn trước khi load menu
  const isValidTable = await validateAndLoadTable();
  
  if (!isValidTable) {
    // Đã redirect về trang lỗi, không cần tiếp tục
    return;
  }

  loadCartFromStorage();
  loadMenu();
  updateCartCount();

  // Tìm kiếm
  document
    .getElementById("searchInput")
    .addEventListener("input", function (e) {
      filterMenu(e.target.value);
    });
});

// Load giỏ hàng từ localStorage
function loadCartFromStorage() {
  const savedCart = localStorage.getItem("restaurantCart");
  if (savedCart) {
    try {
      cart = JSON.parse(savedCart);
      console.log("✅ Đã load giỏ hàng:", cart.length, "món");
    } catch (error) {
      console.error("❌ Lỗi load giỏ hàng:", error);
      cart = [];
    }
  }
}

// Lưu giỏ hàng vào localStorage
function saveCartToStorage() {
  localStorage.setItem("restaurantCart", JSON.stringify(cart));
}

// Load menu từ API
async function loadMenu() {
  try {
    console.log("🔄 Đang kết nối API:", API_URL);

    const response = await fetch(API_URL, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
      mode: "cors",
    });

    console.log("📡 Response status:", response.status);

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    menuData = await response.json();
    console.log("✅ Đã tải dữ liệu:", menuData.length, "món");

    // Map dữ liệu từ API
    menuData = menuData.map((item) => ({
      id: item.id,
      tenMon: item.tenMon,
      gia: item.gia,
      anhUrl: item.anhUrl,
      moTa: item.moTa,
      coSan: item.coSan,
      danhMucId: item.danhMucId,
      tenDanhMuc: item.tenDanhMuc,
    }));

    // ✅ Render tabs danh mục
    renderCategoryTabs();

    // ✅ Hiển thị tất cả món ban đầu
    renderMenu(menuData);
  } catch (error) {
    console.error("Lỗi tải menu:", error);
    document.getElementById("menuContainer").innerHTML = `
            <div class="alert alert-danger" role="alert">
                <h4 class="alert-heading">Không thể tải menu!</h4>
                <p>Lỗi: ${error.message}</p>
                <hr>
                <p class="mb-0">Vui lòng kiểm tra:</p>
                <ul>
                    <li>API server đang chạy tại: <strong>${API_BASE_URL}</strong></li>
                    <li>CORS đã được cấu hình đúng</li>
                    <li>Endpoint API: <strong>${API_URL}</strong></li>
                </ul>
                <button class="btn btn-danger mt-3" onclick="loadMenu()">
                    <i class="fas fa-sync-alt"></i> Thử lại
                </button>
            </div>
        `;
  }
}

// Render menu - chỉ hiển thị món ăn, KHÔNG hiển thị tiêu đề danh mục
function renderMenu(items) {
  if (!items || items.length === 0) {
    document.getElementById("menuContainer").innerHTML = `
            <div class="alert alert-info text-center" role="alert">
                <i class="fas fa-info-circle fa-3x mb-3"></i>
                <h4>Không có món ăn nào</h4>
            </div>
        `;
    return;
  }

  // ✅ Chỉ render grid món ăn, không có tiêu đề danh mục
  let html = `
        <div class="row g-3">
            ${items.map((dish) => renderDishCard(dish)).join("")}
        </div>
    `;

  document.getElementById("menuContainer").innerHTML = html;
}

// Render một card món ăn
function renderDishCard(dish) {
  // ✅ FIX: Khai báo imageUrl TRƯỚC khi dùng
  const imageUrl = dish.anhUrl
    ? `${API_BASE_URL}${dish.anhUrl}`
    : "https://via.placeholder.com/300x200?text=Món+Ăn";

  const description = dish.moTa || "Món ăn ngon, chất lượng";

  // Kiểm tra món đã có trong giỏ chưa
  const isInCart = cart.some((item) => item.id === dish.id);

  return `
        <div class="col-md-6 col-lg-4 col-xl-3">
            <div class="dish-card">
                <img src="${imageUrl}" 
                     alt="${dish.tenMon}" 
                     onerror="this.src='https://via.placeholder.com/300x200?text=Món+Ăn'">
                ${
                  !dish.coSan ? '<div class="sold-out-badge">Hết món</div>' : ""
                }
                ${
                  isInCart
                    ? '<div class="in-cart-badge"><i class="fas fa-check"></i> Đã có trong giỏ</div>'
                    : ""
                }
                <div class="dish-info">
                    <h5 class="dish-name">${dish.tenMon}</h5>
                    <p class="dish-desc">${description}</p>
                    <div class="d-flex justify-content-between align-items-center">
                        <span class="dish-price">${formatPrice(dish.gia)}</span>
                        ${
                          dish.coSan
                            ? `
                            <button class="btn btn-add-cart ${
                              isInCart ? "btn-secondary" : "btn-danger"
                            }" 
                                    onclick="addToCart(${dish.id})" 
                                    ${isInCart ? "disabled" : ""}>
                                <i class="fas fa-${
                                  isInCart ? "check" : "cart-plus"
                                }"></i>
                                ${isInCart ? "Đã thêm" : "Thêm"}
                            </button>
                        `
                            : '<span class="text-muted small">Tạm hết</span>'
                        }
                    </div>
                </div>
            </div>
        </div>
    `;
}

// Thêm món vào giỏ hàng
function addToCart(dishId) {
  const dish = menuData.find((d) => d.id === dishId);
  if (!dish) return;

  // Kiểm tra món đã có trong giỏ chưa
  const existingItem = cart.find((item) => item.id === dishId);

  if (existingItem) {
    // Hiển thị thông báo
    showNotification(
      "Món ăn đã có trong đơn hàng! Vui lòng vào giỏ hàng để chọn số lượng hoặc xóa món.",
      "warning"
    );
    return;
  }

  // Thêm món mới với số lượng = 1
  cart.push({
    ...dish,
    quantity: 1,
  });

  saveCartToStorage();
  updateCartCount();

  // Render lại menu để cập nhật trạng thái nút
  renderMenu(menuData);

  showNotification(`Đã thêm "${dish.tenMon}" vào giỏ hàng!`, "success");
}

// Cập nhật số lượng trong giỏ hàng (header)
function updateCartCount() {
  const cartCount = cart.length; // Đếm số món ăn, không phải tổng số lượng
  const cartCountElement = document.querySelector(".cart-count");
  if (cartCountElement) {
    cartCountElement.textContent = cartCount;
  }
}

// Tìm kiếm món ăn
function filterMenu(keyword) {
  if (!keyword.trim()) {
    renderMenu(menuData);
    return;
  }

  const filtered = menuData.filter(
    (item) =>
      item.tenMon.toLowerCase().includes(keyword.toLowerCase()) ||
      (item.moTa && item.moTa.toLowerCase().includes(keyword.toLowerCase())) ||
      (item.tenDanhMuc &&
        item.tenDanhMuc.toLowerCase().includes(keyword.toLowerCase()))
  );

  if (filtered.length === 0) {
    document.getElementById("menuContainer").innerHTML = `
//     <div class="alert alert-warning text-center" role="alert">
//         <i class="fas fa-search fa-3x mb-3"></i>
//         <h4>Không tìm thấy món ăn</h4>
//         <p>Không có món ăn nào phù hợp với từ khóa "<strong>${keyword}</strong>"</p>
//     </div>
// `;
`<div class="alert alert-warning text-center" role="alert">
    <i class="fas fa-search fa-3x mb-3"></i>
    <h4>Không tìm thấy món ăn</h4>
    <p>Không có món ăn nào phù hợp với từ khóa "<strong>${keyword}</strong>"</p>
</div>
`;
  } else {
    renderMenu(filtered);
  }
}

// Hiển thị thông báo
function showNotification(message, type = "info") {
  // Xóa thông báo cũ nếu có
  const oldNotif = document.querySelector(".notification-toast");
  if (oldNotif) {
    oldNotif.remove();
  }

  const notification = document.createElement("div");
  notification.className = `notification-toast alert alert-${type} alert-dismissible fade show`;
  notification.style.cssText = `
        position: fixed;
        top: 100px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        max-width: 400px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    `;

  const icon =
    type === "success"
      ? "check-circle"
      : type === "warning"
      ? "exclamation-triangle"
      : "info-circle";

  notification.innerHTML = `
        <i class="fas fa-${icon} me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

  document.body.appendChild(notification);

  setTimeout(() => {
    notification.remove();
  }, 4000);
}

// Format giá tiền
function formatPrice(price) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(price);
}

// ✅ HÀM MỚI: Render tabs danh mục
function renderCategoryTabs() {
  const categories = {};

  // Lấy danh sách danh mục unique
  menuData.forEach((item) => {
    const categoryName = item.tenDanhMuc || "Khác";
    if (!categories[categoryName]) {
      categories[categoryName] = true;
    }
  });

  const categoryNames = Object.keys(categories);

  // Render tabs
  let tabsHtml = `
        <button class="category-tab active" onclick="filterByCategory('all')">
            <i class="fas fa-th-large me-2"></i>Tất cả
        </button>
    `;

  categoryNames.forEach((category) => {
    tabsHtml += `
            <button class="category-tab" onclick="filterByCategory('${category}')">
                ${category}
            </button>
        `;
  });

  document.getElementById("categoryTabs").innerHTML = tabsHtml;
}

// ✅ HÀM MỚI: Lọc món theo danh mục
function filterByCategory(category) {
  // Cập nhật active tab
  document.querySelectorAll(".category-tab").forEach((tab) => {
    tab.classList.remove("active");
  });
  event.target.classList.add("active");

  // Lọc món ăn
  if (category === "all") {
    renderMenu(menuData);
  } else {
    const filtered = menuData.filter((item) => item.tenDanhMuc === category);
    renderMenu(filtered);
  }

  // Scroll về đầu menu
  document.getElementById("menuContainer").scrollIntoView({
    behavior: "smooth",
    block: "start",
  });
}
