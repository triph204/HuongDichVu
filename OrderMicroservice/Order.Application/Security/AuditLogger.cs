using Microsoft.Extensions.Logging;

namespace Order.Application.Security
{
    /// <summary>
    /// Audit Logger - Ghi log các ho?t ??ng quan tr?ng
    /// Ph?c v? m?c ?ích security audit và compliance
    /// </summary>
    public interface IAuditLogger
    {
        void LogOrderCreated(int orderId, string orderNumber, string? userId, string ipAddress);
        void LogOrderUpdated(int orderId, string? userId, string ipAddress, string changes);
        void LogOrderStatusChanged(int orderId, string oldStatus, string newStatus, string? userId, string ipAddress);
        void LogOrderDeleted(int orderId, string? userId, string ipAddress);
        void LogUnauthorizedAccess(string resource, string? userId, string ipAddress, string reason);
        void LogSuspiciousActivity(string activity, string? userId, string ipAddress, string details);
    }

    public class AuditLogger : IAuditLogger
    {
        private readonly ILogger<AuditLogger> _logger;

        public AuditLogger(ILogger<AuditLogger> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void LogOrderCreated(int orderId, string orderNumber, string? userId, string ipAddress)
        {
            _logger.LogInformation(
                "[AUDIT] ORDER_CREATED | OrderId: {OrderId} | OrderNumber: {OrderNumber} | UserId: {UserId} | IP: {IpAddress} | Time: {Timestamp}",
                orderId, orderNumber, userId ?? "anonymous", ipAddress, DateTime.UtcNow);
        }

        public void LogOrderUpdated(int orderId, string? userId, string ipAddress, string changes)
        {
            _logger.LogInformation(
                "[AUDIT] ORDER_UPDATED | OrderId: {OrderId} | UserId: {UserId} | IP: {IpAddress} | Changes: {Changes} | Time: {Timestamp}",
                orderId, userId ?? "anonymous", ipAddress, changes, DateTime.UtcNow);
        }

        public void LogOrderStatusChanged(int orderId, string oldStatus, string newStatus, string? userId, string ipAddress)
        {
            _logger.LogInformation(
                "[AUDIT] ORDER_STATUS_CHANGED | OrderId: {OrderId} | OldStatus: {OldStatus} | NewStatus: {NewStatus} | UserId: {UserId} | IP: {IpAddress} | Time: {Timestamp}",
                orderId, oldStatus, newStatus, userId ?? "anonymous", ipAddress, DateTime.UtcNow);
        }

        public void LogOrderDeleted(int orderId, string? userId, string ipAddress)
        {
            _logger.LogWarning(
                "[AUDIT] ORDER_DELETED | OrderId: {OrderId} | UserId: {UserId} | IP: {IpAddress} | Time: {Timestamp}",
                orderId, userId ?? "anonymous", ipAddress, DateTime.UtcNow);
        }

        public void LogUnauthorizedAccess(string resource, string? userId, string ipAddress, string reason)
        {
            _logger.LogWarning(
                "[SECURITY] UNAUTHORIZED_ACCESS | Resource: {Resource} | UserId: {UserId} | IP: {IpAddress} | Reason: {Reason} | Time: {Timestamp}",
                resource, userId ?? "anonymous", ipAddress, reason, DateTime.UtcNow);
        }

        public void LogSuspiciousActivity(string activity, string? userId, string ipAddress, string details)
        {
            _logger.LogWarning(
                "[SECURITY] SUSPICIOUS_ACTIVITY | Activity: {Activity} | UserId: {UserId} | IP: {IpAddress} | Details: {Details} | Time: {Timestamp}",
                activity, userId ?? "anonymous", ipAddress, details, DateTime.UtcNow);
        }
    }
}
