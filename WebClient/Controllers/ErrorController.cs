using Microsoft.AspNetCore.Mvc;

namespace WebClient.Controllers
{
    public class ErrorController : Controller
    {
        /// <summary>
        /// Trang l?i khi bàn không h?p l?
        /// </summary>
        /// <param name="code">Mã l?i: EMPTY_TABLE, TABLE_NOT_FOUND, TABLE_MAINTENANCE, TABLE_INVALID</param>
        /// <param name="table">S? bàn ?ã nh?p</param>
        /// <param name="message">Thông báo l?i tùy ch?nh</param>
        public IActionResult TableError(string? code, string? table, string? message)
        {
            ViewBag.ErrorCode = code ?? "TABLE_INVALID";
            ViewBag.TableName = table;
            ViewBag.ErrorMessage = message;
            return View();
        }

        /// <summary>
        /// Trang l?i chung
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }
    }
}
