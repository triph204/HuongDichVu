using Microsoft.AspNetCore.Mvc;
using WebAdmin.Services;
using WebAdmin.Models;  // ✅ THÊM DÒNG NÀY

namespace WebAdmin.Controllers
{
    [Route("[controller]")]
    public class BanAnController : Controller
    {
        private readonly IRestaurantApiClient _apiClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BanAnController> _logger;

         public BanAnController(
            IRestaurantApiClient apiClient, 
            IConfiguration configuration,
            ILogger<BanAnController> logger)
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
                ViewBag.Error = ex.Message;
                return View(new List<BanAnViewModel>());
            }
        }

        [Route("[action]")]
        public IActionResult Create()
        {
            ViewBag.IsEdit = false;
            var model = new BanAnViewModel
            {
                NgayTao = DateTime.Now
            };
            return View(model);
        }


        [HttpPost]
        [Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BanAnViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.NgayTao == default)
                    model.NgayTao = DateTime.Now;

                await _apiClient.CreateBanAn(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Route("[action]/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _apiClient.GetBanAn(id);
            ViewBag.IsEdit = true;
            return View(model);
        }

        [HttpPost]
        [Route("[action]/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BanAnViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _apiClient.UpdateBanAn(id, model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Route("[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _apiClient.DeleteBanAn(id);
            return RedirectToAction(nameof(Index));
        }
    }
}