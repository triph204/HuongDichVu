using Microsoft.AspNetCore.Mvc;

namespace WebClient.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}