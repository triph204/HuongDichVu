using Microsoft.AspNetCore.Mvc;
using WebAdmin.Services;
using WebAdmin.Models;

namespace WebAdmin.Controllers
{
    [Route("[controller]")]
    public class DanhMucController : Controller
    {
        private readonly IRestaurantApiClient _apiClient;

        public DanhMucController(IRestaurantApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var list = await _apiClient.GetAllDanhMuc();
                return View(list);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(new List<DanhMucViewModel>());
            }
        }

        [Route("[action]")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMucViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _apiClient.CreateDanhMuc(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Route("[action]/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _apiClient.GetDanhMuc(id);
            return View(model);
        }

        [HttpPost]
        [Route("[action]/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhMucViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _apiClient.UpdateDanhMuc(id, model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Route("[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _apiClient.DeleteDanhMuc(id);
            return RedirectToAction(nameof(Index));
        }
    }
}