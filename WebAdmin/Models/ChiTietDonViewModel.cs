using System.Text.Json.Serialization;

public class ChiTietDonViewModel
{
    // ✅ API trả về "id"
    [JsonPropertyName("id")]
    public int ChiTietId { get; set; }
    
    // ✅ API trả về "monId"
    [JsonPropertyName("monId")]
    public int MonId { get; set; }
    
    // ✅ API trả về "tenMon"
    [JsonPropertyName("tenMon")]
    public string? TenMon { get; set; }
    
    // ✅ API trả về "soLuong"
    [JsonPropertyName("soLuong")]
    public int SoLuong { get; set; }
    
    // ✅ API trả về "donGia"
    [JsonPropertyName("donGia")]
    public decimal DonGia { get; set; }
    
    // ✅ API trả về "thanhTien"
    [JsonPropertyName("thanhTien")]
    public decimal ThanhTien { get; set; }
}