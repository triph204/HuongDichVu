using System.Text.Json.Serialization;

namespace WebAdmin.Models
{
    public class DanhMucViewModel
    {
        [JsonPropertyName("id")]
        public int DanhMucId { get; set; }
        
        [JsonPropertyName("tenDanhMuc")]
        public string? TenDanhMuc { get; set; }
        
        [JsonPropertyName("moTa")]
        public string? MoTa { get; set; }
    }
}