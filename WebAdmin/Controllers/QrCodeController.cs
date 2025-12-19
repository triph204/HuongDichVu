using Microsoft.AspNetCore.Mvc;
using WebAdmin.Services;
using WebAdmin.Models;

namespace WebAdmin.Controllers
{
    [Route("[controller]")]
    public class QrCodeController : Controller
    {
        private readonly IRestaurantApiClient _apiClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<QrCodeController> _logger;

        public QrCodeController(
            IRestaurantApiClient apiClient,
            IConfiguration configuration,
            ILogger<QrCodeController> logger)
        {
            _apiClient = apiClient;
            _configuration = configuration;
            _logger = logger;
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var list = await _apiClient.GetAllBanAn();
                return View(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách bàn ăn cho QR Code");
                TempData["Error"] = $"Không thể tải danh sách bàn ăn: {ex.Message}";
                return View(new List<BanAnViewModel>());
            }
        }

        [Route("[action]/{id}")]
        public async Task<IActionResult> View(int id)
        {
            try
            {
                var banAn = await _apiClient.GetBanAn(id);
                if (banAn == null)
                {
                    TempData["Error"] = $"Không tìm thấy bàn ăn ID {id}";
                    return RedirectToAction(nameof(Index));
                }

                // Tạo URL để khách hàng quét QR và order
                var webClientUrl = _configuration["AppSettings:WebClientUrl"];
                ViewBag.QrUrl = $"{webClientUrl}/?table={banAn.SoBan}";
                
                return View(banAn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết bàn ăn cho QR Code");
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}