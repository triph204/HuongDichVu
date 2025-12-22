using Microsoft.AspNetCore.Mvc;

namespace WebClient.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public IActionResult InvalidTable(string table)
        {
            ViewData["TableNumber"] = table ?? "không xác định";
            return View();
        }
    }
}
