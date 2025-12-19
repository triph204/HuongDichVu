using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs;
using Order.Application.Services;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// L?y danh sách t?t c? ??n hàng
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// L?y chi ti?t 1 ??n hàng theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { message = $"Không tìm th?y ??n hàng v?i ID {id}" });

            return Ok(order);
        }

        /// <summary>
        /// L?y các ??n hàng theo tr?ng thái
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetByStatus(string status)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(orders);
        }

        /// <summary>
        /// L?y các ??n hàng theo bàn ?n
        /// </summary>
        [HttpGet("table/{tableId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetByTableId(int tableId)
        {
            var orders = await _orderService.GetOrdersByTableIdAsync(tableId);
            return Ok(orders);
        }

        /// <summary>
        /// T?o ??n hàng m?i
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto)
        {
            var order = await _orderService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        /// <summary>
        /// C?p nh?t thông tin ??n hàng
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateOrderDto dto)
        {
            var result = await _orderService.UpdateOrderAsync(id, dto);
            if (!result)
                return NotFound(new { message = $"Không tìm th?y ??n hàng v?i ID {id}" });

            return Ok(new { message = "C?p nh?t ??n hàng thành công" });
        }

        /// <summary>
        /// C?p nh?t tr?ng thái ??n hàng
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, dto.NewStatus);
            if (!result)
                return NotFound(new { message = $"Không tìm th?y ??n hàng v?i ID {id}" });

            return Ok(new { message = "C?p nh?t tr?ng thái thành công" });
        }

        /// <summary>
        /// Xóa ??n hàng
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _orderService.DeleteOrderAsync(id);
            if (!result)
                return NotFound(new { message = $"Không tìm th?y ??n hàng v?i ID {id}" });

            return Ok(new { message = "Xóa ??n hàng thành công" });
        }
    }
}
