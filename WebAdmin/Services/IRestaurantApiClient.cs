using WebAdmin.Models;

namespace WebAdmin.Services;

public interface IRestaurantApiClient
{
    // Danh Muc
    Task<List<DanhMucViewModel>> GetAllDanhMuc();
    Task<DanhMucViewModel> GetDanhMuc(int id);
    Task<int> CreateDanhMuc(DanhMucViewModel model);
    Task UpdateDanhMuc(int id, DanhMucViewModel model);
    Task DeleteDanhMuc(int id);

    // Mon An
    Task<List<MonAnViewModel>> GetAllMonAn();
    Task<MonAnViewModel> GetMonAn(int id);
    Task<int> CreateMonAn(MonAnViewModel model);
    Task UpdateMonAn(int id, MonAnViewModel model);
    Task DeleteMonAn(int id);

    // Ban An
    Task<List<BanAnViewModel>> GetAllBanAn();
    Task<BanAnViewModel> GetBanAn(int id);
    Task<int> CreateBanAn(BanAnViewModel model);
    Task UpdateBanAn(int id, BanAnViewModel model);
    Task DeleteBanAn(int id);

    // Don Hang
    Task<List<DonHangViewModel>> GetAllDonHang();
    Task<DonHangViewModel> GetDonHang(int id);
    Task UpdateDonHang(int id, DonHangViewModel model);

    // ✅ THÊM METHOD MỚI - Upload Image
    Task<string> UploadImageAsync(IFormFile file);
    
    // ✅ THÊM METHOD MỚI - Delete Image (dùng khi update món)
    Task<bool> DeleteImageAsync(string imageUrl);
}