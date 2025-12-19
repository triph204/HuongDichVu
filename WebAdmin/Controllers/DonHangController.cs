using Microsoft.AspNetCore.Mvc;
using WebAdmin.Services;
using WebAdmin.Models;
using System.Globalization;
using System.Text;          // THÊM DÒNG NÀY
using System.Globalization; // THÊM DÒNG NÀY

namespace WebAdmin.Controllers
{
    [Route("[controller]")]
    public class DonHangController : Controller
    {
        private readonly IRestaurantApiClient _apiClient;
        private const int PageSize = 10;

        public DonHangController(IRestaurantApiClient apiClient)
        {
            _apiClient = apiClient;
        }

[Route("")]
[Route("[action]")]
public async Task<IActionResult> Index(
    string sortBy = "moinhat", 
    string trangThai = "tatca",
    int page = 1)
{
    try
    {
        // ✅ LẤY TOÀN BỘ ĐƠN HÀNG
        var allDonHangs = await _apiClient.GetAllDonHang();
        
        // ✅ ĐẾM SỐ LƯỢNG THEO TỪNG TRẠNG THÁI (TRƯỚC KHI LỌC)
        ViewBag.TotalAllOrders = allDonHangs.Count;
        ViewBag.CountChoXacNhan = allDonHangs.Count(d => d.TrangThai?.ToUpper() == "CHOXACNHAN");
        ViewBag.CountDangChuanBi = allDonHangs.Count(d => d.TrangThai?.ToUpper() == "DANGCHUANBI");
        ViewBag.CountHoanThanh = allDonHangs.Count(d => d.TrangThai?.ToUpper() == "HOANTHANH");
        ViewBag.CountHuy = allDonHangs.Count(d => d.TrangThai?.ToUpper() == "HUY");
        
        // Lọc theo trạng thái
        var filteredDonHangs = allDonHangs;
        if (!string.IsNullOrEmpty(trangThai) && trangThai != "tatca")
        {
            filteredDonHangs = allDonHangs
                .Where(d => d.TrangThai != null && 
                           d.TrangThai.ToUpper() == trangThai.ToUpper())
                .ToList();
        }
        
        // Sắp xếp
        filteredDonHangs = SortDonHang(filteredDonHangs, sortBy);
        
        // Phân trang
        var totalItems = filteredDonHangs.Count;
        var totalPages = (int)Math.Ceiling((double)totalItems / PageSize);
        var paginatedDonHangs = filteredDonHangs
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();
        
        // Truyền dữ liệu lên View
        ViewBag.SortBy = sortBy;
        ViewBag.TrangThai = trangThai;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems; // Số đơn hàng SAU KHI LỌC
        
        return View(paginatedDonHangs);
    }
    catch (Exception ex)
    {
        ViewBag.Error = $"Lỗi: {ex.Message}";
        ViewBag.TotalAllOrders = 0;
        ViewBag.CountChoXacNhan = 0;
        ViewBag.CountDangChuanBi = 0;
        ViewBag.CountHoanThanh = 0;
        ViewBag.CountHuy = 0;
        return View(new List<DonHangViewModel>());
    }
}

        [Route("[action]/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await _apiClient.GetDonHang(id);
                if (model == null)
                {
                    TempData["Error"] = $"Không tìm thấy đơn hàng #{id}";
                    return RedirectToAction(nameof(Index));
                }
                
                // ✅ THÊM: Truyền displayStatus qua ViewBag
                ViewBag.DisplayStatus = GetDisplayStatus(model.TrangThai);
                
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("[action]/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string trangThai)
        {
            try
            {
                var model = await _apiClient.GetDonHang(id);
                if (model == null)
                {
                    TempData["Error"] = $"Không tìm thấy đơn hàng #{id}";
                    return RedirectToAction(nameof(Index));
                }
                
                // Map từ UI status sang API status
                var apiStatus = MapToApiStatus(trangThai);
                
                // Cập nhật trạng thái
                model.TrangThai = apiStatus;
                model.NgayCapNhat = DateTime.Now;
                
                await _apiClient.UpdateDonHang(id, model);
                
                // Hiển thị text tiếng Việt
                var displayStatus = GetDisplayStatus(apiStatus);
                TempData["Success"] = $"Đã cập nhật đơn #{id} thành: {displayStatus}";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // Hàm map từ UI status sang API status
        private string MapToApiStatus(string uiStatus)
        {
            // Nếu đã là API status thì giữ nguyên
            var upperStatus = uiStatus.ToUpper();
            if (upperStatus == "HUY" || upperStatus == "CHOXACNHAN" || 
                upperStatus == "HOANTHANH" || upperStatus == "DANGCHUANBI")
                return upperStatus;
            
            // Map từ tiếng Việt
            var normalized = RemoveDiacritics(uiStatus).ToUpper();
            
            return normalized switch
            {
                "HUY" or "HỦY" => "HUY",
                "CHOXACNHAN" or "CHO XAC NHAN" => "CHOXACNHAN",
                "HOANTHANH" or "HOAN THANH" => "HOANTHANH",
                "DANGCHUANBI" or "DANG CHUAN BI" => "DANGCHUANBI",
                _ => uiStatus
            };
        }
        
        // Hàm lấy display text từ API status
        public static string GetDisplayStatus(string? apiStatus)
        {
            if (string.IsNullOrEmpty(apiStatus))
                return "Không xác định";
                
            return apiStatus.ToUpper() switch
            {
                "HUY" => "Hủy",
                "CHOXACNHAN" => "Chờ xác nhận",
                "HOANTHANH" => "Đã hoàn thành",
                "DANGCHUANBI" => "Đang chuẩn bị",
                _ => apiStatus
            };
        }
        
        // Hàm xóa dấu tiếng Việt
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        // Hàm sắp xếp thông minh
        private List<DonHangViewModel> SortDonHang(List<DonHangViewModel> donHangs, string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "cu nhat" => donHangs
                    .OrderBy(d => d.NgayCapNhat ?? DateTime.MinValue)
                    .ThenBy(d => d.DonId)
                    .ToList(),
                    
                "tongtien-cao" => donHangs
                    .OrderByDescending(d => d.TongTien)
                    .ThenByDescending(d => d.NgayCapNhat ?? DateTime.MinValue)
                    .ToList(),
                    
                "tongtien-thap" => donHangs
                    .OrderBy(d => d.TongTien)
                    .ThenByDescending(d => d.NgayCapNhat ?? DateTime.MinValue)
                    .ToList(),
                    
                "trangthai" => donHangs
                    .OrderBy(d => GetStatusOrder(d.TrangThai))
                    .ThenByDescending(d => d.NgayCapNhat ?? DateTime.MinValue)
                    .ToList(),
                    
                "ban" => donHangs
                    .OrderBy(d => d.SoBan)
                    .ThenByDescending(d => d.NgayCapNhat ?? DateTime.MinValue)
                    .ToList(),
                    
                _ => donHangs // Mặc định: mới nhất lên đầu
                    .OrderByDescending(d => d.NgayCapNhat ?? DateTime.MinValue)
                    .ThenByDescending(d => d.DonId)
                    .ToList()
            };
        }

        // Hàm xác định thứ tự ưu tiên trạng thái (cho sắp xếp)
        private int GetStatusOrder(string? trangThai)
        {
            if (string.IsNullOrEmpty(trangThai))
                return 999;
                
            return trangThai.ToUpper() switch
            {
                "CHOXACNHAN" => 1,    // Ưu tiên cao nhất
                "DANGCHUANBI" => 2,
                "HOANTHANH" => 3,
                "HUY" => 4,
                _ => 99
            };
        }
    }
}