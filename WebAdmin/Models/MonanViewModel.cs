using System.Text.Json.Serialization;

namespace WebAdmin.Models
{
    // DTO để GỬI lên API (không có Id)
    public class CreateMonAnDto
    {
        [JsonPropertyName("tenMon")]
        public string TenMon { get; set; }
        
        [JsonPropertyName("gia")]
        public decimal Gia { get; set; }
        
        [JsonPropertyName("anhUrl")]
        public string? AnhUrl { get; set; }
        
        [JsonPropertyName("moTa")]
        public string? MoTa { get; set; }
        
        [JsonPropertyName("coSan")]
        public bool CoSan { get; set; }
        
        [JsonPropertyName("danhMucId")]
        public int DanhMucId { get; set; }
    }

    public class MonAnViewModel
    {
        [JsonPropertyName("id")]
        public int MonId { get; set; }
        
        [JsonPropertyName("tenMon")]
        public string? TenMon { get; set; }
        
        [JsonPropertyName("gia")]
        public decimal Gia { get; set; }
        
        [JsonPropertyName("anhUrl")]
        public string? AnhUrl { get; set; }
        
        [JsonPropertyName("moTa")]
        public string? MoTa { get; set; }
        
        [JsonPropertyName("coSan")]
        public bool CoSan { get; set; }
        
        [JsonPropertyName("danhMucId")]
        public int DanhMucId { get; set; }
        
        [JsonPropertyName("tenDanhMuc")]
        public string? TenDanhMuc { get; set; }
        
        [JsonIgnore]
        public List<DanhMucViewModel>? DanhMucList { get; set; }
        
        [JsonIgnore]
        public DateTime NgayTao { get; set; }
    }

    // ✅ THÊM CLASS MỚI - Response từ API Upload
    public class UploadImageResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
        
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;
    }
}