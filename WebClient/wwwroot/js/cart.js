// Cấu hình API
const API_BASE_URL = "http://localhost:5137";
const API_ORDER_URL = `${API_BASE_URL}/api/DonHang`;
const API_TABLE_URL = `${API_BASE_URL}/api/BanAn`;

let cart = [];

// Khởi tạo
document.addEventListener("DOMContentLoaded", function () {
  console.log("🔍 Đang khởi tạo giỏ hàng...");
  loadCartFromStorage();
  forceLoadTableFromURL(); // ⚠️ SỬA: Dùng hàm mới
  renderCart();
});

// ⚠️ HÀM MỚI: Luôn load số bàn từ URL, không dùng localStorage cũ
function forceLoadTableFromURL() {
  const urlParams = new URLSearchParams(window.location.search);
  const tableFromURL = urlParams.get("table");

  console.log("🔍 URL hiện tại:", window.location.href);
  console.log("🔍 Số bàn từ URL:", tableFromURL);

  if (tableFromURL) {
    // Clear localStorage cũ và set giá trị mới
    localStorage.removeItem("tableNumber");
    localStorage.removeItem("tableDisplay");
    localStorage.removeItem("tableId");

    // Lưu số bàn từ URL
    localStorage.setItem("tableNumber", tableFromURL);
    localStorage.setItem("tableDisplay", `Bàn ${tableFromURL}`);

    // Hiển thị ngay lập tức
    const tableElement = document.getElementById("tableNumber");
    if (tableElement) {
      tableElement.textContent = `Bàn ${tableFromURL}`;
    }

    console.log(`✅ Đã set bàn từ URL: Bàn ${tableFromURL}`);

    // Sau đó mới load thông tin đầy đủ từ server
    loadTableInfo();
  } else {
    // Nếu không có số bàn trong URL, dùng mặc định
    console.warn("⚠️ Không có số bàn trong URL, dùng mặc định");
    loadTableInfo();
  }
}

// Hàm cũ (giữ nguyên nhưng đã được gọi từ forceLoadTableFromURL)
async function loadTableInfo() {
  const tableNumber = localStorage.getItem("tableNumber") || "1";

  console.log("🔍 Đang load thông tin bàn:", tableNumber);

  try {
    const response = await fetch(API_TABLE_URL);
    if (response.ok) {
      const tables = await response.json();

      // Tìm bàn theo số bàn
      const foundTable = tables.find(
        (table) =>
          table.soBan === tableNumber ||
          table.soBan === `Bàn ${tableNumber}` ||
          table.id === parseInt(tableNumber)
      );

      if (foundTable) {
        console.log("✅ Tìm thấy bàn:", foundTable);

        // Cập nhật localStorage
        localStorage.setItem("tableNumber", tableNumber);
        localStorage.setItem("tableDisplay", foundTable.soBan);
        localStorage.setItem("tableId", foundTable.id);

        // Hiển thị
        const displayName = foundTable.soBan.includes("Bàn")
          ? foundTable.soBan
          : `Bàn ${foundTable.soBan}`;
        document.getElementById("tableNumber").textContent = displayName;
      } else {
        console.warn("⚠️ Không tìm thấy bàn trong DB");
        document.getElementById(
          "tableNumber"
        ).textContent = `Bàn ${tableNumber}`;
      }
    }
  } catch (error) {
    console.error("❌ Lỗi khi lấy thông tin bàn:", error);
    document.getElementById("tableNumber").textContent = `Bàn ${tableNumber}`;
  }
}

// Fallback khi không kết nối được server
function setDefaultTable(tableFromQR) {
  localStorage.setItem("tableNumber", tableFromQR);
  localStorage.setItem("tableDisplay", `Bàn ${tableFromQR}`);
  localStorage.setItem("tableId", parseInt(tableFromQR) || 1);
  document.getElementById("tableNumber").textContent = `Bàn ${tableFromQR}`;
}

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
  console.log("💾 Đã lưu giỏ hàng");
}

// Render giỏ hàng
function renderCart() {
  const cartItemsList = document.getElementById("cartItemsList");

  if (cart.length === 0) {
    cartItemsList.innerHTML = `
      <div class="text-center py-5">
        <i class="fas fa-shopping-cart fa-4x text-muted mb-3"></i>
        <h5 class="text-muted">Giỏ hàng trống</h5>
        <p class="text-muted">Hãy thêm món ăn vào giỏ hàng</p>
        <a href="/" class="btn btn-danger mt-3">
          <i class="fas fa-utensils me-2"></i>Xem thực đơn
        </a>
      </div>
    `;
    updateSummary();
    return;
  }

  let html = "";
  cart.forEach((item, index) => {
    const itemTotal = item.gia * item.quantity;
    const imageUrl = item.anhUrl
      ? `${API_BASE_URL}${item.anhUrl}`
      : "https://via.placeholder.com/100?text=Món+Ăn";

    html += `
      <div class="cart-item-row">
        <div class="d-flex align-items-center flex-grow-1">
          <img src="${imageUrl}" 
               alt="${item.tenMon}" 
               class="cart-item-img me-3"
               onerror="this.src='https://via.placeholder.com/100?text=Món+Ăn'">
          
          <div class="flex-grow-1">
            <h6 class="mb-1">${item.tenMon}</h6>
            <p class="text-muted small mb-1">${item.moTa || "Món ăn ngon"}</p>
            <p class="text-danger mb-0">${formatPrice(item.gia)}</p>
          </div>
        </div>
        
        <div class="d-flex align-items-center gap-3">
          <div class="quantity-controls">
            <button class="btn-qty" onclick="updateQuantity(${index}, -1)">
              <i class="fas fa-minus"></i>
            </button>
            <span class="quantity-value">${item.quantity}</span>
            <button class="btn-qty" onclick="updateQuantity(${index}, 1)">
              <i class="fas fa-plus"></i>
            </button>
          </div>
          
          <div class="text-end" style="min-width: 100px;">
            <strong class="text-danger">${formatPrice(itemTotal)}</strong>
          </div>
          
          <button class="btn btn-sm btn-outline-danger" onclick="removeItem(${index})" title="Xóa món">
            <i class="fas fa-trash"></i>
          </button>
        </div>
      </div>
    `;
  });

  cartItemsList.innerHTML = html;
  updateSummary();
}

// Cập nhật số lượng
function updateQuantity(index, change) {
  if (index < 0 || index >= cart.length) return;

  cart[index].quantity += change;

  if (cart[index].quantity <= 0) {
    removeItem(index);
    return;
  }

  if (cart[index].quantity > 99) {
    cart[index].quantity = 99;
    showAlert("Số lượng tối đa là 99", "warning");
  }

  saveCartToStorage();
  renderCart();
}

// Xóa món
function removeItem(index) {
  if (index >= 0 && index < cart.length) {
    const removedItem = cart[index];

    if (
      confirm(`Bạn có chắc muốn xóa "${removedItem.tenMon}" khỏi giỏ hàng?`)
    ) {
      cart.splice(index, 1);
      saveCartToStorage();
      renderCart();
      showAlert(`Đã xóa "${removedItem.tenMon}" khỏi giỏ hàng`, "success");
    }
  }
}

// Cập nhật tóm tắt đơn hàng
function updateSummary() {
  const subtotal = cart.reduce(
    (sum, item) => sum + item.gia * item.quantity,
    0
  );
  const total = subtotal;

  document.getElementById("subtotal").textContent = formatPrice(subtotal);
  document.getElementById("totalAmount").textContent = formatPrice(total);
  document.getElementById("itemCount").textContent = cart.length;
}

// Gửi đơn hàng - TRY CATCH
async function submitOrder() {
  try {
    if (cart.length === 0) {
      showAlert("Giỏ hàng trống! Vui lòng thêm món ăn", "warning");
      return;
    }

    const tableDisplay = localStorage.getItem("tableDisplay") || "Bàn 1";
    const itemCount = cart.length;
    const totalAmount = cart.reduce(
      (sum, item) => sum + item.gia * item.quantity,
      0
    );

    // Dùng try-catch cho từng phần
    try {
      const confirmTableElement = document.getElementById("confirmTableName");
      if (confirmTableElement) {
        confirmTableElement.textContent = tableDisplay;
      }
    } catch (e) {
      console.warn("Không thể set confirmTableName:", e.message);
    }

    try {
      const confirmCartInfoElement = document.getElementById("confirmCartInfo");
      if (confirmCartInfoElement) {
        confirmCartInfoElement.textContent = `${itemCount} món • ${formatPrice(
          totalAmount
        )}`;
      }
    } catch (e) {
      console.warn("Không thể set confirmCartInfo:", e.message);
    }

    // Hiển thị modal
    const confirmModalElement = document.getElementById("confirmModal");
    if (confirmModalElement) {
      const confirmModal = new bootstrap.Modal(confirmModalElement);
      confirmModal.show();
    }
  } catch (error) {
    console.error("Lỗi trong submitOrder:", error);
    showAlert("Có lỗi xảy ra: " + error.message, "danger");
  }
}

// Xác nhận gửi đơn - GỬI CẢ ID BÀN VÀ SỐ BÀN
async function confirmSubmit() {
  const confirmModal = bootstrap.Modal.getInstance(
    document.getElementById("confirmModal")
  );
  if (confirmModal) confirmModal.hide();

  const submitBtn = document.getElementById("submitOrderBtn");
  submitBtn.disabled = true;
  submitBtn.innerHTML =
    '<i class="fas fa-spinner fa-spin me-2"></i>Đang gửi...';

  try {
    // Lấy thông tin bàn
    const tableNumber = localStorage.getItem("tableNumber") || "1";
    const tableDisplay =
      localStorage.getItem("tableDisplay") || `Bàn ${tableNumber}`;
    const tableId = parseInt(localStorage.getItem("tableId")) || 1;
    const orderNote = document.getElementById("orderNote").value.trim();

    console.log("=== THÔNG TIN ĐƠN HÀNG ===");
    console.log("Số bàn:", tableNumber);
    console.log("Tên bàn:", tableDisplay);
    console.log("ID bàn:", tableId);
    console.log("Số món:", cart.length);

    // ✅ GỬI CẢ ID BÀN VÀ SỐ BÀN
    const orderData = {
      BanId: tableId, // ID bàn trong database
      SoBan: tableNumber, // Số bàn từ QR code
      TenBan: tableDisplay, // Tên bàn để hiển thị
      GhiChuKhach: orderNote,
      MonOrder: cart.map((item) => ({
        MonId: item.id,
        SoLuong: item.quantity,
      })),
    };

    console.log("📤 Đang gửi đơn hàng:", JSON.stringify(orderData, null, 2));

    const response = await fetch(API_ORDER_URL, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(orderData),
    });

    console.log("📥 Response status:", response.status);

    if (!response.ok) {
      const errorText = await response.text();
      console.error("❌ Server trả về lỗi:", errorText);

      // Hiển thị lỗi thân thiện
      let errorMessage = "Có lỗi xảy ra khi gửi đơn hàng";
      try {
        const errorJson = JSON.parse(errorText);
        if (errorJson.title) errorMessage = errorJson.title;
        if (errorJson.errors) {
          const errors = Object.values(errorJson.errors).flat();
          errorMessage = errors.join(", ");
        }
      } catch (e) {
        // Nếu không parse được JSON
      }

      showAlert(errorMessage, "danger");
      throw new Error(`HTTP ${response.status}: ${errorMessage}`);
    }

    const result = await response.json();
    console.log("✅ Đơn hàng đã được gửi:", result);

    // Xóa giỏ hàng
    cart = [];
    saveCartToStorage();
    document.getElementById("orderNote").value = "";

    // Hiển thị modal thành công
    document.getElementById("orderTableName").textContent = tableDisplay;
    document.getElementById("orderTime").textContent =
      new Date().toLocaleTimeString("vi-VN");

    const successModal = new bootstrap.Modal(
      document.getElementById("successModal")
    );
    successModal.show();
  } catch (error) {
    console.error("❌ Lỗi gửi đơn hàng:", error);

    if (error.message.includes("Failed to fetch")) {
      showAlert(
        "Không thể kết nối đến server. Vui lòng kiểm tra lại!",
        "danger"
      );
    } else {
      showAlert(error.message, "danger");
    }
  } finally {
    submitBtn.disabled = false;
    submitBtn.innerHTML = '<i class="fas fa-paper-plane me-2"></i>Gửi đơn hàng';
  }
}

// Hiển thị thông báo
function showAlert(message, type = "info") {
  const alertDiv = document.createElement("div");
  alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
  alertDiv.style.cssText = `
    position: fixed;
    top: 80px;
    right: 20px;
    z-index: 9999;
    min-width: 300px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  `;
  alertDiv.innerHTML = `
    ${message}
    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
  `;

  document.body.appendChild(alertDiv);

  setTimeout(() => {
    if (alertDiv.parentNode) {
      alertDiv.remove();
    }
  }, 3000);
}

// Format giá tiền
function formatPrice(price) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(price);
}

// Debug function
function debugTableInfo() {
  console.log("=== DEBUG TABLE INFO ===");
  console.log(
    "URL params:",
    new URLSearchParams(window.location.search).toString()
  );
  console.log(
    "tableNumber from localStorage:",
    localStorage.getItem("tableNumber")
  );
  console.log(
    "tableDisplay from localStorage:",
    localStorage.getItem("tableDisplay")
  );
  console.log("tableId from localStorage:", localStorage.getItem("tableId"));
}

// Gọi debug khi load
debugTableInfo();
