// ===== SIGNALR ADMIN CLIENT =====
// File: wwwroot/js/signalr-admin.js

const SIGNALR_HUB_URL = "http://localhost:5137/orderHub"; // ⚠️ Thay port của Server API

let connection = null;
let reconnectAttempts = 0;
const MAX_RECONNECT_ATTEMPTS = 5;

// ===== KHỞI TẠO SIGNALR CONNECTION =====
function initSignalR() {
  console.log("🔌 Đang kết nối SignalR Hub...");

  connection = new signalR.HubConnectionBuilder()
    .withUrl(SIGNALR_HUB_URL, {
      skipNegotiation: false,
      transport:
        signalR.HttpTransportType.WebSockets |
        signalR.HttpTransportType.ServerSentEvents |
        signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Retry intervals
    .configureLogging(signalR.LogLevel.Information)
    .build();

  // Event: Khi kết nối thành công
  connection.onreconnecting((error) => {
    console.warn("⚠️ SignalR đang reconnect...", error);
    showConnectionStatus("reconnecting");
  });

  connection.onreconnected((connectionId) => {
    console.log("✅ SignalR reconnected:", connectionId);
    showConnectionStatus("connected");
    reconnectAttempts = 0;
  });

  connection.onclose((error) => {
    console.error("❌ SignalR connection closed:", error);
    showConnectionStatus("disconnected");

    // Thử reconnect thủ công
    if (reconnectAttempts < MAX_RECONNECT_ATTEMPTS) {
      setTimeout(() => {
        reconnectAttempts++;
        console.log(`🔄 Thử reconnect lần ${reconnectAttempts}...`);
        startConnection();
      }, 5000);
    }
  });

  // ===== LISTEN: ĐƠN HÀNG MỚI TỪ CLIENT =====
  connection.on("ReceiveNewOrder", (orderData) => {
    console.log("📦 Nhận đơn hàng mới:", orderData);
    handleNewOrder(orderData);
  });

  // ===== LISTEN: TRẠNG THÁI ĐƠN HÀNG THAY ĐỔI =====
  connection.on("OrderStatusChanged", (data) => {
    console.log("🔄 Trạng thái đơn hàng thay đổi:", data);
    handleOrderStatusChanged(data);
  });

  // Bắt đầu kết nối
  startConnection();
}

// ===== BẮT ĐẦU KẾT NỐI =====
async function startConnection() {
  try {
    await connection.start();
    console.log("✅ SignalR connected successfully!");
    showConnectionStatus("connected");
    reconnectAttempts = 0;
  } catch (err) {
    console.error("❌ SignalR connection failed:", err);
    showConnectionStatus("disconnected");

    // Retry sau 5s
    setTimeout(() => {
      reconnectAttempts++;
      if (reconnectAttempts < MAX_RECONNECT_ATTEMPTS) {
        console.log(
          `🔄 Retry connection (${reconnectAttempts}/${MAX_RECONNECT_ATTEMPTS})...`
        );
        startConnection();
      }
    }, 5000);
  }
}

// ===== XỬ LÝ ĐƠN HÀNG MỚI =====
function handleNewOrder(orderData) {
  // 1. Hiển thị toast notification
  showToast(
    "🔔 Đơn hàng mới!",
    `Bàn ${orderData.soBan} - ${formatPrice(orderData.tongTien)}`,
    "success"
  );

  // 2. Play sound notification
  playNotificationSound();

  // 3. Nếu đang ở trang Index, thêm đơn vào table
  if (
    window.location.pathname.includes("/DonHang") &&
    !window.location.pathname.includes("/Details")
  ) {
    const currentTab = new URLSearchParams(window.location.search).get(
      "trangThai"
    );

    // Nếu đang xem "Tất cả" hoặc "Chờ xác nhận" → Thêm đơn vào table
    if (!currentTab || currentTab === "tatca" || currentTab === "CHOXACNHAN") {
      addNewOrderToTable(orderData);
    }

    // Update count badges
    updateCountBadge("tatca", 1);
    updateCountBadge("CHOXACNHAN", 1);
  }
}

// ===== THÊM ĐƠN MỚI VÀO TABLE =====
function addNewOrderToTable(orderData) {
  const tbody = document.querySelector(".donhang-table tbody");
  if (!tbody) return;

  // Tạo row mới
  const newRow = document.createElement("tr");
  newRow.className = "table-row-highlight new-order-animation";

  const statusBadge =
    orderData.trangThai === "ChoXacNhan" ? "badge-warning" : "badge-secondary";
  const statusText =
    orderData.trangThai === "ChoXacNhan" ? "Chờ xác nhận" : orderData.trangThai;

  // ✅ Format ghi chú (cắt ngắn nếu quá dài)
  let ghiChuDisplay = "--";
  if (orderData.ghiChu) {
    const maxLength = 30;
    ghiChuDisplay =
      orderData.ghiChu.length > maxLength
        ? orderData.ghiChu.substring(0, maxLength) + "..."
        : orderData.ghiChu;
  }

  newRow.innerHTML = `
        <td><strong class="text-primary">#${orderData.soDon}</strong></td>
        <td><span class="badge bg-light text-dark border">Bàn ${
          orderData.soBan
        }</span></td>
        <td><strong class="text-success">${formatPrice(
          orderData.tongTien
        )}</strong></td>
        <td><small class="text-muted">${ghiChuDisplay}</small></td>
        <td>
            <span class="badge donhang-badge ${statusBadge}">
                ${statusText}
            </span>
        </td>
        <td>
            <span class="text-muted">${new Date().toLocaleDateString(
              "vi-VN"
            )}</span><br>
            <small class="text-secondary">${new Date().toLocaleTimeString(
              "vi-VN",
              { hour: "2-digit", minute: "2-digit" }
            )}</small>
        </td>
        <td class="donhang-actions text-center">
            <a href="/DonHang/Details/${
              orderData.orderId
            }" class="btn btn-primary btn-sm">
                👁️ Chi Tiết
            </a>
        </td>
    `;

  // Thêm vào đầu table
  tbody.insertBefore(newRow, tbody.firstChild);

  // Animation highlight
  setTimeout(() => {
    newRow.classList.remove("new-order-animation");
  }, 2000);

  // Update "Hiển thị X-Y trong tổng số Z đơn"
  updatePaginationInfo();
}

// ===== UPDATE COUNT BADGE =====
function updateCountBadge(status, increment = 1) {
  const tabs = document.querySelectorAll(".status-tab");
  tabs.forEach((tab) => {
    const href = tab.getAttribute("href");
    if (href && href.includes(`trangThai=${status}`)) {
      const countSpan = tab.querySelector(".tab-count");
      if (countSpan) {
        const currentCount = parseInt(countSpan.textContent) || 0;
        countSpan.textContent = currentCount + increment;
      }
    }
  });
}

// ===== UPDATE PAGINATION INFO =====
function updatePaginationInfo() {
  const toolbar = document.querySelector(".toolbar-left");
  if (toolbar) {
    const rows = document.querySelectorAll(".donhang-table tbody tr").length;
    const totalSpan = toolbar.querySelector("strong:last-child");
    if (totalSpan) {
      const currentTotal = parseInt(totalSpan.textContent) || 0;
      totalSpan.textContent = currentTotal + 1;
    }
  }
}

// ===== XỬ LÝ THAY ĐỔI TRẠNG THÁI =====
function handleOrderStatusChanged(data) {
  // Update UI nếu đang xem chi tiết đơn này
  const currentPath = window.location.pathname;
  const orderIdMatch = currentPath.match(/\/DonHang\/Details\/(\d+)/);

  if (orderIdMatch && parseInt(orderIdMatch[1]) === data.orderId) {
    // Đang xem đơn này -> Reload page
    showToast(
      "🔄 Cập nhật trạng thái",
      `Đơn #${data.soDon} đã được cập nhật`,
      "info"
    );
    setTimeout(() => window.location.reload(), 1000);
  }
}

// ===== UPDATE COUNT BADGES =====
async function updateOrderCounts() {
  // ✅ Đơn giản: Reload page để update counts
  // (counts đã được tính sẵn trong Controller)
  console.log("🔄 Updating order counts...");

  // Nếu đang ở trang DonHang Index, reload sau 1s
  if (window.location.pathname.includes("/DonHang")) {
    // Không reload ngay để user thấy toast trước
    // Có thể thêm logic reload thông minh hơn nếu cần
  }
}

// ===== HIỂN THỊ TOAST NOTIFICATION =====
function showToast(title, message, type = "info") {
  // Tạo toast container nếu chưa có
  let container = document.getElementById("toast-container");
  if (!container) {
    container = document.createElement("div");
    container.id = "toast-container";
    container.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
        `;
    document.body.appendChild(container);
  }

  // Tạo toast element
  const toast = document.createElement("div");
  toast.className = `toast-notification toast-${type}`;
  toast.innerHTML = `
        <div class="toast-header">
            <strong>${title}</strong>
            <button class="toast-close" onclick="this.parentElement.parentElement.remove()">×</button>
        </div>
        <div class="toast-body">${message}</div>
    `;

  toast.style.cssText = `
        background: ${
          type === "success"
            ? "#28a745"
            : type === "warning"
            ? "#ffc107"
            : "#17a2b8"
        };
        color: white;
        padding: 15px;
        border-radius: 8px;
        margin-bottom: 10px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        min-width: 300px;
        animation: slideInRight 0.3s ease;
    `;

  container.appendChild(toast);

  // Auto remove sau 5s
  setTimeout(() => {
    toast.style.animation = "slideOutRight 0.3s ease";
    setTimeout(() => toast.remove(), 300);
  }, 5000);
}

// ===== PLAY NOTIFICATION SOUND =====
function playNotificationSound() {
  try {
    // Sử dụng Web Audio API để tạo beep sound
    const audioContext = new (window.AudioContext ||
      window.webkitAudioContext)();
    const oscillator = audioContext.createOscillator();
    const gainNode = audioContext.createGain();

    oscillator.connect(gainNode);
    gainNode.connect(audioContext.destination);

    oscillator.frequency.value = 800; // Tần số Hz
    oscillator.type = "sine";

    gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
    gainNode.gain.exponentialRampToValueAtTime(
      0.01,
      audioContext.currentTime + 0.5
    );

    oscillator.start(audioContext.currentTime);
    oscillator.stop(audioContext.currentTime + 0.5);
  } catch (err) {
    console.warn("Cannot play sound:", err);
  }
}

// ===== HIỂN THỊ TRẠNG THÁI KẾT NỐI =====
function showConnectionStatus(status) {
  let indicator = document.getElementById("signalr-status");
  if (!indicator) {
    indicator = document.createElement("div");
    indicator.id = "signalr-status";
    indicator.style.cssText = `
            position: fixed;
            bottom: 20px;
            right: 20px;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: bold;
            z-index: 9998;
            display: flex;
            align-items: center;
            gap: 8px;
        `;
    document.body.appendChild(indicator);
  }

  const statusConfig = {
    connected: { color: "#28a745", text: "🟢 Realtime ON", display: "none" },
    reconnecting: {
      color: "#ffc107",
      text: "🟡 Đang kết nối lại...",
      display: "flex",
    },
    disconnected: { color: "#dc3545", text: "🔴 Mất kết nối", display: "flex" },
  };

  const config = statusConfig[status] || statusConfig.disconnected;
  indicator.style.background = config.color;
  indicator.style.color = "white";
  indicator.style.display = config.display;
  indicator.textContent = config.text;
}

// ===== HELPER: FORMAT PRICE =====
function formatPrice(price) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(price);
}

// ===== CSS ANIMATIONS =====
const style = document.createElement("style");
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }

    /* ✅ THÊM: Animation cho đơn hàng mới */
    @keyframes highlightNew {
        0%, 100% {
            background-color: transparent;
        }
        50% {
            background-color: #fff3cd;
        }
    }

    .new-order-animation {
        animation: highlightNew 2s ease-in-out;
    }

    .toast-close {
        background: none;
        border: none;
        color: white;
        font-size: 20px;
        cursor: pointer;
        margin-left: auto;
        padding: 0 5px;
    }

    .toast-header {
        display: flex;
        align-items: center;
        margin-bottom: 5px;
    }
`;
document.head.appendChild(style);

// ===== AUTO INIT KHI LOAD PAGE =====
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", initSignalR);
} else {
  initSignalR();
}

console.log("📡 SignalR Admin module loaded");
