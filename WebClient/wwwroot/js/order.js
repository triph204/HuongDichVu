// ===== CẤU HÌNH API & SIGNALR =====
const API_BASE_URL = "http://localhost:5137";
const API_ORDER_URL = `${API_BASE_URL}/api/DonHang`;
const SIGNALR_HUB_URL = `${API_BASE_URL}/orderHub`;

let connection = null;
let currentTableNumber = null;
let currentOrders = [];
let currentFilter = "all";

// ===== KHỞI TẠO KHI LOAD PAGE =====
document.addEventListener("DOMContentLoaded", async function () {
  await forceLoadTableFromURL(); // Chờ load xong số bàn
  initSignalR();
  loadOrders();
  updateLinksWithTableParam(); // Cập nhật các link với table parameter
});

// ✅ Cập nhật tất cả các link và onclick với table parameter
function updateLinksWithTableParam() {
  const tableNumber = localStorage.getItem("tableNumber");
  if (!tableNumber) return;

  // Cập nhật các thẻ <a>
  document.querySelectorAll("a[href]").forEach((link) => {
    const href = link.getAttribute("href");
    if (href && href.startsWith("/") && !href.includes("?table=")) {
      link.setAttribute("href", `${href}?table=${tableNumber}`);
    }
  });

  // Cập nhật các button với onclick
  document.querySelectorAll("[onclick]").forEach((el) => {
    const onclick = el.getAttribute("onclick");
    if (onclick && onclick.includes("window.location.href")) {
      const newOnclick = onclick.replace(
        /window\.location\.href='([^']+)'/g,
        (match, url) => {
          if (url.startsWith("/") && !url.includes("?table=")) {
            return `window.location.href='${url}?table=${tableNumber}'`;
          }
          return match;
        }
      );
      el.setAttribute("onclick", newOnclick);
    }
  });
}

// ===== LẤY SỐ BÀN TỪ URL (bắt buộc có table parameter hoặc localStorage) =====
async function forceLoadTableFromURL() {
  const urlParams = new URLSearchParams(window.location.search);
  let tableFromURL = urlParams.get("table");

  console.log("🔍 Orders: Đang load số bàn từ URL:", tableFromURL);

  // Nếu không có table trong URL, kiểm tra localStorage
  if (!tableFromURL) {
    tableFromURL = localStorage.getItem("tableNumber");
    console.log("🔍 Orders: Số bàn từ localStorage:", tableFromURL);
  }

  // Bắt buộc phải có table parameter hoặc localStorage
  if (!tableFromURL) {
    console.error(
      "❌ Orders: Không có table parameter trong URL và localStorage"
    );
    window.location.href = `/Error/InvalidTable?table=không_xác_định`;
    return;
  }

  // Có table thì validate
  try {
    // Kiểm tra bàn có tồn tại trong database không
    const response = await fetch(`${API_BASE_URL}/api/BanAn`);
    if (response.ok) {
      const tables = await response.json();
      const tableExists = tables.some(
        (t) =>
          t.soBan === tableFromURL ||
          t.soBan === `Bàn ${tableFromURL}` ||
          t.id === parseInt(tableFromURL)
      );

      if (!tableExists) {
        // Bàn không tồn tại, redirect đến trang lỗi
        console.error("❌ Orders: Bàn không tồn tại:", tableFromURL);
        window.location.href = `/Error/InvalidTable?table=${encodeURIComponent(
          tableFromURL
        )}`;
        return;
      }
    }

    localStorage.removeItem("tableNumber");
    localStorage.removeItem("tableDisplay");
    localStorage.setItem("tableNumber", tableFromURL);
    localStorage.setItem("tableDisplay", `Bàn ${tableFromURL}`);

    currentTableNumber = tableFromURL;

    const headerTableNumber = document.getElementById("headerTableNumber");
    if (headerTableNumber) {
      headerTableNumber.textContent = `Bàn ${tableFromURL}`;
    }

    console.log(`✅ Orders: Đã set bàn: Bàn ${tableFromURL}`);
  } catch (error) {
    console.error("❌ Orders: Lỗi kiểm tra bàn:", error);
    // Nếu không kết nối được, vẫn cho qua
    localStorage.removeItem("tableNumber");
    localStorage.removeItem("tableDisplay");
    localStorage.setItem("tableNumber", tableFromURL);
    localStorage.setItem("tableDisplay", `Bàn ${tableFromURL}`);

    currentTableNumber = tableFromURL;

    const headerTableNumber = document.getElementById("headerTableNumber");
    if (headerTableNumber) {
      headerTableNumber.textContent = `Bàn ${tableFromURL}`;
    }
  }
}

// ===== KHỞI TẠO SIGNALR CONNECTION =====
function initSignalR() {
  console.log("🔌 WebClient: Đang kết nối SignalR Hub...");

  connection = new signalR.HubConnectionBuilder()
    .withUrl(SIGNALR_HUB_URL, {
      skipNegotiation: false,
      transport:
        signalR.HttpTransportType.WebSockets |
        signalR.HttpTransportType.ServerSentEvents |
        signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .configureLogging(signalR.LogLevel.Information)
    .build();

  // Event handlers
  connection.onreconnecting(() => {
    console.warn("⚠️ SignalR reconnecting...");
    showConnectionStatus("reconnecting");
  });

  connection.onreconnected((connectionId) => {
    console.log("✅ SignalR reconnected:", connectionId);
    showConnectionStatus("connected");
  });

  connection.onclose(() => {
    console.error("❌ SignalR connection closed");
    showConnectionStatus("disconnected");
    setTimeout(() => startConnection(), 5000);
  });

  // ===== LISTEN: TRẠNG THÁI ĐƠN HÀNG THAY ĐỔI =====
  connection.on("OrderStatusChanged", (data) => {
    console.log("🔄 Trạng thái đơn hàng thay đổi:", data);
    handleOrderStatusChanged(data);
  });

  // ===== LISTEN: ĐƠN HÀNG MỚI (nếu khách đặt thêm món) =====
  connection.on("ReceiveNewOrder", (orderData) => {
    console.log("📦 Đơn hàng mới:", orderData);
    // Kiểm tra xem có phải đơn của bàn mình không
    if (orderData.soBan === currentTableNumber) {
      loadOrders(); // Reload danh sách đơn
      showToast("🎉 Đơn hàng mới", "Đơn hàng của bạn đã được tạo!", "success");
    }
  });

  startConnection();
}

// ===== BẮT ĐẦU KẾT NỐI =====
async function startConnection() {
  try {
    await connection.start();
    console.log("✅ WebClient SignalR connected!");
    showConnectionStatus("connected");

    // Join vào group của bàn (optional)
    if (currentTableNumber) {
      await connection.invoke("JoinTableGroup", currentTableNumber);
      console.log(`📍 Joined Table_${currentTableNumber} group`);
    }
  } catch (err) {
    console.error("❌ SignalR connection failed:", err);
    showConnectionStatus("disconnected");
    setTimeout(() => startConnection(), 5000);
  }
}

// ===== XỬ LÝ KHI TRẠNG THÁI THAY ĐỔI =====
function handleOrderStatusChanged(data) {
  // Kiểm tra xem có phải đơn của bàn mình không
  if (data.soBan !== currentTableNumber) return;

  console.log(
    `🔄 Đơn #${data.soDon} thay đổi: ${data.oldStatus} → ${data.newStatus}`
  );

  // 1. Hiển thị toast
  const statusText = mapServerStatusToDisplay(data.newStatus);
  showToast(
    "🔔 Cập nhật đơn hàng",
    `Đơn #${data.soDon} đã chuyển sang: ${statusText}`,
    getStatusToastType(data.newStatus)
  );

  // 2. Update trong danh sách hiện tại
  const orderIndex = currentOrders.findIndex((o) => o.id === data.orderId);
  if (orderIndex !== -1) {
    currentOrders[orderIndex].trangThai = data.newStatus;
    currentOrders[orderIndex].ngayCapNhat = data.ngayCapNhat;

    // Re-render danh sách
    displayOrders(currentOrders, currentFilter);
  } else {
    // Đơn mới hoặc chưa có trong danh sách -> reload
    loadOrders();
  }
}

// ===== LOAD DANH SÁCH ĐƠN HÀNG =====
async function loadOrders() {
  const container = document.getElementById("ordersContainer");

  if (!currentTableNumber) {
    container.innerHTML = `
            <div class="text-center py-5">
                <i class="fas fa-exclamation-triangle fa-3x text-warning mb-3"></i>
                <h5>Không xác định được số bàn</h5>
                <p class="text-muted">Vui lòng quét lại mã QR</p>
                <a href="/" class="btn btn-danger mt-3">
                    <i class="fas fa-qrcode me-2"></i>Quét mã QR
                </a>
            </div>
        `;
    return;
  }

  // Show loading
  container.innerHTML = `
        <div class="text-center py-5">
            <div class="spinner-border text-danger" role="status">
                <span class="visually-hidden">Đang tải...</span>
            </div>
            <p class="mt-3 text-muted">Đang tải đơn hàng...</p>
        </div>
    `;

  try {
    // Gọi API lấy tất cả đơn hàng
    const response = await fetch(API_ORDER_URL);

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const allOrders = await response.json();

    // Lọc đơn hàng của bàn hiện tại
    currentOrders = allOrders.filter(
      (order) =>
        order.soBan === currentTableNumber ||
        order.soBan === `Bàn ${currentTableNumber}`
    );

    console.log(
      `📦 Tìm thấy ${currentOrders.length} đơn hàng của Bàn ${currentTableNumber}`
    );

    // Hiển thị
    displayOrders(currentOrders, currentFilter);
  } catch (error) {
    console.error("Lỗi tải đơn hàng:", error);
    container.innerHTML = `
            <div class="text-center py-5">
                <i class="fas fa-exclamation-circle fa-3x text-danger mb-3"></i>
                <h5>Không thể tải đơn hàng</h5>
                <p class="text-muted">${error.message}</p>
                <button class="btn btn-danger mt-3" onclick="loadOrders()">
                    <i class="fas fa-redo me-2"></i>Thử lại
                </button>
            </div>
        `;
  }
}

// ===== HIỂN THỊ DANH SÁCH ĐƠN HÀNG =====
function displayOrders(orders, filter = "all") {
  const container = document.getElementById("ordersContainer");

  // Lọc theo trạng thái
  let filteredOrders = orders;
  if (filter !== "all") {
    filteredOrders = orders.filter((order) => {
      const clientStatus = mapServerStatusToClient(order.trangThai);
      return clientStatus === filter;
    });
  }

  // Sắp xếp: Mới nhất lên đầu
  filteredOrders.sort(
    (a, b) =>
      new Date(b.ngayCapNhat || b.ngayTao) -
      new Date(a.ngayCapNhat || a.ngayTao)
  );

  if (filteredOrders.length === 0) {
    container.innerHTML = `
            <div class="text-center py-5">
                <i class="fas fa-inbox fa-3x text-muted mb-3"></i>
                <h5>Chưa có đơn hàng nào</h5>
                <p class="text-muted">
                    ${
                      filter === "all"
                        ? "Bạn chưa đặt món nào"
                        : "Không có đơn hàng với trạng thái này"
                    }
                </p>
                <a href="/?table=${currentTableNumber}" class="btn btn-danger mt-3">
                    <i class="fas fa-utensils me-2"></i>Đặt món ngay
                </a>
            </div>
        `;
    return;
  }

  // Render danh sách đơn hàng
  const ordersHTML = filteredOrders
    .map((order) => {
      const statusBadge = getStatusBadgeClass(order.trangThai);
      const statusText = mapServerStatusToDisplay(order.trangThai);

      return `
            <div class="card mb-3 order-card" data-order-id="${order.id}">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <div>
                            <h6 class="mb-1">
                                <i class="fas fa-receipt text-danger me-1"></i>
                                Đơn #${order.soDon}
                            </h6>
                            <small class="text-muted">
                                <i class="far fa-clock me-1"></i>
                                ${formatDateTime(
                                  order.ngayCapNhat || order.ngayTao
                                )}
                            </small>
                        </div>
                        <span class="badge ${statusBadge}">${statusText}</span>
                    </div>

                    ${
                      order.chiTiet && order.chiTiet.length > 0
                        ? `
                        <div class="order-items mt-3">
                            ${order.chiTiet
                              .slice(0, 3)
                              .map(
                                (item) => `
                                <div class="d-flex justify-content-between mb-1">
                                    <span class="text-muted">${item.tenMon} x${
                                  item.soLuong
                                }</span>
                                    <span class="text-dark">${formatPrice(
                                      item.thanhTien
                                    )}</span>
                                </div>
                            `
                              )
                              .join("")}
                            ${
                              order.chiTiet.length > 3
                                ? `
                                <small class="text-muted">...và ${
                                  order.chiTiet.length - 3
                                } món khác</small>
                            `
                                : ""
                            }
                        </div>
                    `
                        : ""
                    }

                    <hr class="my-2">
                    
                    <div class="d-flex justify-content-between align-items-center">
                        <strong class="text-danger">
                            Tổng: ${formatPrice(order.tongTien)}
                        </strong>
                        <button class="btn btn-sm btn-outline-danger" onclick="viewOrderDetail(${
                          order.id
                        })">
                            <i class="fas fa-eye me-1"></i>Chi tiết
                        </button>
                    </div>
                </div>
            </div>
        `;
    })
    .join("");

  container.innerHTML = ordersHTML;
}

// ===== XEM CHI TIẾT ĐƠN HÀNG =====
async function viewOrderDetail(orderId) {
  try {
    const response = await fetch(`${API_ORDER_URL}/${orderId}`);

    if (!response.ok) {
      throw new Error("Không thể tải chi tiết đơn hàng");
    }

    const order = await response.json();

    const statusBadge = getStatusBadgeClass(order.trangThai);
    const statusText = mapServerStatusToDisplay(order.trangThai);

    const modalContent = `
            <div class="order-detail">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <h6 class="mb-0">Đơn #${order.soDon}</h6>
                    <span class="badge ${statusBadge}">${statusText}</span>
                </div>

                <div class="mb-3">
                    <small class="text-muted d-block">
                        <i class="far fa-clock me-1"></i>
                        Ngày đặt: ${formatDateTime(order.ngayTao)}
                    </small>
                    <small class="text-muted d-block">
                        <i class="fas fa-sync-alt me-1"></i>
                        Cập nhật: ${formatDateTime(order.ngayCapNhat)}
                    </small>
                </div>

                <h6 class="mb-2">Chi tiết món ăn:</h6>
                <div class="table-responsive">
                    <table class="table table-sm">
                        <thead>
                            <tr>
                                <th>Món</th>
                                <th class="text-center">SL</th>
                                <th class="text-end">Đơn giá</th>
                                <th class="text-end">Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${order.chiTiet
                              .map(
                                (item) => `
                                <tr>
                                    <td>${item.tenMon}</td>
                                    <td class="text-center">${item.soLuong}</td>
                                    <td class="text-end">${formatPrice(
                                      item.donGia
                                    )}</td>
                                    <td class="text-end">${formatPrice(
                                      item.thanhTien
                                    )}</td>
                                </tr>
                            `
                              )
                              .join("")}
                        </tbody>
                        <tfoot>
                            <tr class="table-active">
                                <td colspan="3" class="text-end"><strong>Tổng cộng:</strong></td>
                                <td class="text-end"><strong class="text-danger">${formatPrice(
                                  order.tongTien
                                )}</strong></td>
                            </tr>
                        </tfoot>
                    </table>
                </div>

                ${
                  order.ghiChuKhach
                    ? `
                    <div class="alert alert-info mt-3">
                        <small><strong>Ghi chú:</strong> ${order.ghiChuKhach}</small>
                    </div>
                `
                    : ""
                }
            </div>
        `;

    document.getElementById("orderDetailContent").innerHTML = modalContent;
    const modal = new bootstrap.Modal(
      document.getElementById("orderDetailModal")
    );
    modal.show();
  } catch (error) {
    console.error("Lỗi tải chi tiết:", error);
    alert("Không thể tải chi tiết đơn hàng. Vui lòng thử lại!");
  }
}

// ===== FILTER ĐƠN HÀNG =====
function filterOrders(status) {
  currentFilter = status;

  // Update active tab
  document.querySelectorAll(".tab-btn").forEach((btn) => {
    btn.classList.remove("active");
    if (btn.dataset.status === status) {
      btn.classList.add("active");
    }
  });

  // Re-display với filter mới
  displayOrders(currentOrders, status);
}

// ===== HELPER FUNCTIONS =====

// Map server status sang client status
function mapServerStatusToClient(serverStatus) {
  const statusMap = {
    CHOXACNHAN: "pending",
    ChoXacNhan: "pending",
    DANGCHUANBI: "cooking",
    DangChuanBi: "cooking",
    HOANTHANH: "completed",
    HoanThanh: "completed",
    HUY: "cancel",
    Huy: "cancel",
  };
  return statusMap[serverStatus] || "pending";
}

// Map server status sang text hiển thị
function mapServerStatusToDisplay(serverStatus) {
  const displayMap = {
    CHOXACNHAN: "Chờ xác nhận",
    ChoXacNhan: "Chờ xác nhận",
    DANGCHUANBI: "Đang chuẩn bị",
    DangChuanBi: "Đang chuẩn bị",
    HOANTHANH: "Hoàn thành",
    HoanThanh: "Hoàn thành",
    HUY: "Đã hủy",
    Huy: "Đã hủy",
  };
  return displayMap[serverStatus] || serverStatus;
}

// Get badge class cho status
function getStatusBadgeClass(status) {
  const clientStatus = mapServerStatusToClient(status);
  const badgeMap = {
    pending: "bg-warning text-dark",
    cooking: "bg-info text-white",
    completed: "bg-success text-white",
    cancel: "bg-secondary text-white",
  };
  return badgeMap[clientStatus] || "bg-secondary";
}

// Get toast type cho status
function getStatusToastType(status) {
  const clientStatus = mapServerStatusToClient(status);
  const typeMap = {
    pending: "warning",
    cooking: "info",
    completed: "success",
    cancel: "secondary",
  };
  return typeMap[clientStatus] || "info";
}

// Format giá tiền
function formatPrice(price) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(price);
}

// Format ngày giờ
function formatDateTime(dateString) {
  if (!dateString) return "--";
  const date = new Date(dateString);
  return date.toLocaleString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

// ===== TOAST NOTIFICATION =====
function showToast(title, message, type = "info") {
  let container = document.getElementById("toast-container-client");
  if (!container) {
    container = document.createElement("div");
    container.id = "toast-container-client";
    container.style.cssText = `
            position: fixed;
            top: 80px;
            right: 20px;
            z-index: 9999;
        `;
    document.body.appendChild(container);
  }

  const bgColors = {
    success: "#28a745",
    warning: "#ffc107",
    info: "#17a2b8",
    secondary: "#6c757d",
  };

  const toast = document.createElement("div");
  toast.style.cssText = `
        background: ${bgColors[type] || bgColors.info};
        color: white;
        padding: 15px 20px;
        border-radius: 8px;
        margin-bottom: 10px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        min-width: 280px;
        animation: slideInRight 0.3s ease;
    `;

  toast.innerHTML = `
        <div style="display: flex; justify-content: space-between; align-items: start;">
            <div>
                <strong style="display: block; margin-bottom: 5px;">${title}</strong>
                <div style="font-size: 14px;">${message}</div>
            </div>
            <button onclick="this.parentElement.parentElement.remove()" 
                    style="background: none; border: none; color: white; font-size: 20px; cursor: pointer; margin-left: 10px;">×</button>
        </div>
    `;

  container.appendChild(toast);

  setTimeout(() => {
    toast.style.animation = "slideOutRight 0.3s ease";
    setTimeout(() => toast.remove(), 300);
  }, 5000);
}

// ===== CONNECTION STATUS =====
function showConnectionStatus(status) {
  let indicator = document.getElementById("signalr-status-client");
  if (!indicator) {
    indicator = document.createElement("div");
    indicator.id = "signalr-status-client";
    indicator.style.cssText = `
            position: fixed;
            bottom: 80px;
            right: 20px;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: bold;
            z-index: 9998;
        `;
    document.body.appendChild(indicator);
  }

  const statusConfig = {
    connected: { color: "#28a745", text: "🟢 Realtime", display: "none" },
    reconnecting: {
      color: "#ffc107",
      text: "🟡 Đang kết nối...",
      display: "block",
    },
    disconnected: {
      color: "#dc3545",
      text: "🔴 Mất kết nối",
      display: "block",
    },
  };

  const config = statusConfig[status] || statusConfig.disconnected;
  indicator.style.background = config.color;
  indicator.style.color = "white";
  indicator.style.display = config.display;
  indicator.textContent = config.text;
}

// ===== CSS ANIMATIONS =====
const style = document.createElement("style");
style.textContent = `
    @keyframes slideInRight {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    @keyframes slideOutRight {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(100%); opacity: 0; }
    }
    .order-card {
        transition: transform 0.2s, box-shadow 0.2s;
        border-left: 4px solid #dc3545;
    }
    .order-card:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }
`;
document.head.appendChild(style);

console.log("📡 WebClient Order module with SignalR loaded");
