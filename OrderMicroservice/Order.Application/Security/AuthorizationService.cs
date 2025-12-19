using Order.Infrastructure.Repositories;

namespace Order.Application.Security
{
    /// <summary>
    /// Authorization Service Interface - Ki?m tra quy?n truy c?p
    /// Phòng ch?ng IDOR (Insecure Direct Object Reference)
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Ki?m tra user có quy?n xem order không
        /// </summary>
        Task<bool> CanViewOrderAsync(int orderId, string userId, string userRole);

        /// <summary>
        /// Ki?m tra user có quy?n ch?nh s?a order không
        /// </summary>
        Task<bool> CanEditOrderAsync(int orderId, string userId, string userRole);

        /// <summary>
        /// Ki?m tra user có quy?n xóa order không
        /// </summary>
        Task<bool> CanDeleteOrderAsync(int orderId, string userId, string userRole);

        /// <summary>
        /// Ki?m tra user có quy?n thay ??i status order không
        /// </summary>
        Task<bool> CanUpdateOrderStatusAsync(int orderId, string userId, string userRole);
    }

    /// <summary>
    /// Authorization Service Implementation
    /// Phòng ch?ng IDOR - Ki?m tra quy?n tr??c khi truy c?p tài nguyên
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IOrderRepository _orderRepository;

        public AuthorizationService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task<bool> CanViewOrderAsync(int orderId, string userId, string userRole)
        {
            // Admin và Manager có th? xem t?t c? orders
            if (IsAdminOrManager(userRole))
                return true;

            // Staff ch? xem ???c orders ?ang active
            if (userRole == "Staff")
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                return order != null && order.Status != "Huy";
            }

            // Guest/Anonymous không có quy?n xem
            return false;
        }

        public async Task<bool> CanEditOrderAsync(int orderId, string userId, string userRole)
        {
            // Ch? Admin và Manager có quy?n edit
            if (!IsAdminOrManager(userRole))
                return false;

            // Ki?m tra order có t?n t?i không
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return false;

            // Không cho edit orders ?ã hoàn thành ho?c ?ã h?y
            if (order.Status == "HoanThanh" || order.Status == "Huy")
                return false;

            return true;
        }

        public async Task<bool> CanDeleteOrderAsync(int orderId, string userId, string userRole)
        {
            // Ch? Admin có quy?n xóa
            if (userRole != "Admin")
                return false;

            // Ki?m tra order có t?n t?i không
            var order = await _orderRepository.GetByIdAsync(orderId);
            return order != null;
        }

        public async Task<bool> CanUpdateOrderStatusAsync(int orderId, string userId, string userRole)
        {
            // Admin, Manager và Staff có th? update status
            if (userRole != "Admin" && userRole != "Manager" && userRole != "Staff")
                return false;

            // Ki?m tra order có t?n t?i không
            var order = await _orderRepository.GetByIdAsync(orderId);
            return order != null;
        }

        private static bool IsAdminOrManager(string userRole)
        {
            return userRole == "Admin" || userRole == "Manager";
        }
    }
}
