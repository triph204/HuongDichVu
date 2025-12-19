using Microsoft.AspNetCore.Mvc;

namespace WebClient.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}