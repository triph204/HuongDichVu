using System.Text.Json.Serialization;

namespace WebAdmin.Models
{
    public class BanAnViewModel
    {
        [JsonPropertyName("id")]
        public int BanId { get; set; }
        
        [JsonPropertyName("soBan")]
        public string? SoBan { get; set; }
        
        [JsonPropertyName("trangThai")]
        public string? TrangThai { get; set; }
        
        [JsonPropertyName("ngayTao")]
        public DateTime NgayTao { get; set; }
    }
}