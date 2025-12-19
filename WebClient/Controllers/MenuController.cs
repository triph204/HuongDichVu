using Microsoft.AspNetCore.Mvc;

namespace WebClient.Controllers
{
    public class MenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}