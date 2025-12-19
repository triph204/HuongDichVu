using Microsoft.AspNetCore.Mvc;
using WebAdmin.Services;
using WebAdmin.Models;

namespace WebAdmin.Controllers
{
    [Route("[controller]")]
    public class MonAnController : Controller
    {
        private readonly IRestaurantApiClient _apiClient;
        private readonly ILogger<MonAnController> _logger;
        private readonly IConfiguration _configuration;

        public MonAnController(
            IRestaurantApiClient apiClient, 
            ILogger<MonAnController> logger,
            IConfiguration configuration)
        {
            _apiClient = apiClient;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: /MonAn?page=1&categoryId=0
[Route("")]
[Route("[action]")]
public async Task<IActionResult> Index(int page = 1, int categoryId = 0)
{
    try
    {
        const int pageSize = 12; // Số món ăn mỗi trang
        
        // ✅ 1. Lấy toàn bộ danh sách món ăn
        var allMonAn = await _apiClient.GetAllMonAn();
        
        // ✅ 2. Lọc theo danh mục (nếu có)
        var filteredMonAn = categoryId > 0 
            ? allMonAn.Where(m => m.DanhMucId == categoryId).ToList()
            : allMonAn;
        
        // ✅ 3. Tạo phân trang
        var paginatedList = PaginatedList<MonAnViewModel>.Create(
            filteredMonAn, 
            page, 
            pageSize
        );
        
        // ✅ 4. Lấy danh sách danh mục cho filter
        var danhMucList = await _apiClient.GetAllDanhMuc();
        
        // ✅ 5. Truyền dữ liệu vào View
        ViewData["ApiBaseUrl"] = _configuration["ApiSettings:BaseUrl"];
        ViewData["DanhMucList"] = danhMucList;
        ViewData["CurrentCategory"] = categoryId;
        
        return View(paginatedList);
    }
    catch (Exception ex)
    {
        ViewBag.Error = ex.Message;
        return View(new PaginatedList<MonAnViewModel>(new List<MonAnViewModel>(), 0, 1, 12));
    }
}

        // GET: /MonAn/Create
        [Route("[action]")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new MonAnViewModel
                {
                    CoSan = true, // Mặc định là có sẵn
                    DanhMucList = await _apiClient.GetAllDanhMuc()
                };
                
                // ✅ Thêm ApiBaseUrl
                ViewData["ApiBaseUrl"] = _configuration["ApiSettings:BaseUrl"];
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /MonAn/Create
        [HttpPost]
        [Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonAnViewModel model, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ 1. Xử lý upload ảnh (nếu có)
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        _logger.LogInformation("📤 Uploading image: {FileName}", ImageFile.FileName);
                        
                        var imageUrl = await _apiClient.UploadImageAsync(ImageFile);
                        model.AnhUrl = imageUrl;
                        
                        _logger.LogInformation("✅ Image uploaded: {Url}", imageUrl);
                    }

                    // ✅ 2. Tạo món ăn
                    await _apiClient.CreateMonAn(model);
                    
                    TempData["Success"] = "Thêm món ăn thành công";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError("❌ Error creating món ăn: {Message}", ex.Message);
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }
            
            // Load lại danh mục nếu có lỗi
            model.DanhMucList = await _apiClient.GetAllDanhMuc();
            // ✅ Thêm ApiBaseUrl khi return view với error
            ViewData["ApiBaseUrl"] = _configuration["ApiSettings:BaseUrl"];
            return View(model);
        }

        // GET: /MonAn/Edit/5
        [Route("[action]/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _apiClient.GetMonAn(id);
                if (model == null)
                {
                    TempData["Error"] = "Không tìm thấy món ăn";
                    return RedirectToAction(nameof(Index));
                }
                
                model.DanhMucList = await _apiClient.GetAllDanhMuc();
                
                // ✅ QUAN TRỌNG: Thêm ApiBaseUrl để hiển thị ảnh
                ViewData["ApiBaseUrl"] = _configuration["ApiSettings:BaseUrl"];
                
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /MonAn/Edit/5
        [HttpPost]
        [Route("[action]/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MonAnViewModel model, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ 1. Xử lý upload ảnh mới (nếu có)
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        _logger.LogInformation("📤 Uploading new image: {FileName}", ImageFile.FileName);
                        
                        // Lưu URL ảnh cũ để xóa sau
                        var oldImageUrl = model.AnhUrl;
                        
                        // Upload ảnh mới
                        var newImageUrl = await _apiClient.UploadImageAsync(ImageFile);
                        model.AnhUrl = newImageUrl;
                        
                        _logger.LogInformation("✅ New image uploaded: {Url}", newImageUrl);
                        
                        // ✅ 2. Xóa ảnh cũ (nếu có)
                        if (!string.IsNullOrEmpty(oldImageUrl))
                        {
                            _logger.LogInformation("🗑️ Deleting old image: {Url}", oldImageUrl);
                            await _apiClient.DeleteImageAsync(oldImageUrl);
                        }
                    }

                    // ✅ 3. Cập nhật món ăn
                    await _apiClient.UpdateMonAn(id, model);
                    
                    TempData["Success"] = "Cập nhật món ăn thành công";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError("❌ Error updating món ăn: {Message}", ex.Message);
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }
            
            // Load lại danh mục nếu có lỗi
            model.DanhMucList = await _apiClient.GetAllDanhMuc();
            // ✅ Thêm ApiBaseUrl khi return view với error
            ViewData["ApiBaseUrl"] = _configuration["ApiSettings:BaseUrl"];
            return View(model);
        }

        // GET: /MonAn/Delete/5
        [Route("[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // ✅ Lấy thông tin món ăn để xóa ảnh
                var monAn = await _apiClient.GetMonAn(id);
                
                // ✅ Xóa món ăn
                await _apiClient.DeleteMonAn(id);
                
                // ✅ Xóa ảnh (nếu có)
                if (monAn != null && !string.IsNullOrEmpty(monAn.AnhUrl))
                {
                    _logger.LogInformation("🗑️ Deleting image: {Url}", monAn.AnhUrl);
                    await _apiClient.DeleteImageAsync(monAn.AnhUrl);
                }
                
                TempData["Success"] = "Xóa món ăn thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Error deleting món ăn: {Message}", ex.Message);
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}