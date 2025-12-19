using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAdmin.Filters
{
    /// <summary>
    /// Attribute để cho phép truy cập mà không cần token
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowAnonymousAttribute : Attribute
    {
    }

    /// <summary>
    /// Custom Authorization Filter - Kiểm tra token trước khi vào controller
    /// </summary>
    public class AuthorizeTokenAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // ✅ Kiểm tra xem controller hoặc action có [AllowAnonymous] không
            var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));

            if (hasAllowAnonymous)
            {
                return; // Cho phép truy cập
            }

            // Lấy token từ cookie
            var token = context.HttpContext.Request.Cookies["token"];

            // Nếu không có token -> redirect về trang login
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Dangnhap", null);
                return;
            }

            // Optional: Kiểm tra session
            var username = context.HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new RedirectToActionResult("Login", "Dangnhap", null);
                return;
            }

            // Token hợp lệ -> cho phép truy cập
        }
    }
}