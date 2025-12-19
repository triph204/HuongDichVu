using System.Text.Json.Serialization;

public class DonHangViewModel
{
    // ✅ API trả về "id" (số nguyên)
    [JsonPropertyName("id")]
    public int DonId { get; set; }  // Sẽ map đúng với "id": 3
    
    // ✅ API trả về "banId" 
    [JsonPropertyName("banId")]
    public int BanId { get; set; }
    
    // ✅ API trả về "soBan"
    [JsonPropertyName("soBan")]
    public string SoBan { get; set; }
    
    // ✅ API trả về "soDon"
    [JsonPropertyName("soDon")]
    public string SoDon { get; set; }
    
    // ✅ API trả về "tongTien"
    [JsonPropertyName("tongTien")]
    public decimal TongTien { get; set; }
    
    // ✅ API trả về "trangThai"
    [JsonPropertyName("trangThai")]
    public string? TrangThai { get; set; }

    [JsonPropertyName("ghiChuKhach")]
    public string GhiChuKhach { get; set; }
    
    // ✅ API trả về "ngayCapNhat"
    [JsonPropertyName("ngayCapNhat")]
    public DateTime? NgayCapNhat { get; set; }
    
    // ✅ API trả về "chiTiet"
    [JsonPropertyName("chiTiet")]
    public List<ChiTietDonViewModel>? ChiTiet { get; set; }
}