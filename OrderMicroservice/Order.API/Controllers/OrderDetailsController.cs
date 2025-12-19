using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs;
using Order.Application.Services;

namespace Order.API.Controllers
{
    [Route("api/orders/{orderId}/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly IOrderDetailService _orderDetailService;

        public OrderDetailsController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

        /// <summary>
        /// L?y chi ti?t các món trong ??n hàng
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetByOrderId(int orderId)
        {
            var details = await _orderDetailService.GetDetailsByOrderIdAsync(orderId);
            return Ok(details);
        }

        /// <summary>
        /// Thêm món vào ??n hàng
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderDetailDto>> AddDish(int orderId, AddOrderDetailDto dto)
        {
            var detail = await _orderDetailService.AddDishToOrderAsync(orderId, dto);
            if (detail == null)
                return BadRequest(new { message = "Không th? thêm món vào ??n hàng" });

            return CreatedAtAction(nameof(GetByOrderId), new { orderId }, detail);
        }

        /// <summary>
        /// C?p nh?t s? l??ng món
        /// </summary>
        [HttpPut("{detailId}")]
        public async Task<IActionResult> UpdateQuantity(int orderId, int detailId, [FromBody] UpdateOrderDetailDto dto)
        {
            var result = await _orderDetailService.UpdateQuantityAsync(detailId, dto.Quantity);
            if (!result)
                return NotFound(new { message = $"Không tìm th?y chi ti?t v?i ID {detailId}" });

            return Ok(new { message = "C?p nh?t s? l??ng thành công" });
        }

        /// <summary>
        /// Xóa món kh?i ??n hàng
        /// </summary>
        [HttpDelete("{detailId}")]
        public async Task<IActionResult> RemoveDish(int orderId, int detailId)
        {
            var result = await _orderDetailService.RemoveDishFromOrderAsync(detailId);
            if (!result)
                return NotFound(new { message = $"Không tìm th?y chi ti?t v?i ID {detailId}" });

            return Ok(new { message = "Xóa món thành công" });
        }
    }
}
