namespace Order.API.Models
{
    /// <summary>
    /// Model test health check
    /// </summary>
    public class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Services { get; set; } = new();
    }

    /// <summary>
    /// Model response API chu?n hóa
    /// Bao g?m TraceId ?? h? tr? debug mà không leak sensitive info
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        
        /// <summary>
        /// TraceId ?? trace request trong logs mà không expose chi ti?t l?i
        /// </summary>
        public string? TraceId { get; set; }
        
        /// <summary>
        /// Timestamp c?a response
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Model pagination
    /// </summary>
    public class PaginatedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public List<T> Data { get; set; } = new();
    }
}
