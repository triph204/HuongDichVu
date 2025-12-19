using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs;
using Order.Application.Services;
using Order.Application.Validators;

namespace Order.API.Controllers
{
    /// <summary>
    /// Orders Controller - API Layer
    /// Áp d?ng RESTful API best practices
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// L?y danh sách t?t c? ??n hàng
        /// </summary>
        /// <returns>Danh sách ??n hàng</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
        {
            _logger.LogInformation("Getting all orders");
            
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// L?y chi ti?t 1 ??n hàng theo ID
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Thông tin ??n hàng</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetById(int id)
        {
            _logger.LogInformation("Getting order with ID: {OrderId}", id);
            
            var order = await _orderService.GetOrderByIdAsync(id);
            
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", id);
                return NotFound(new { message = $"Không tìm th?y ??n hàng v?i ID {id}" });
            }

            return Ok(order);
        }

        /// <summary>
        /// L?y các ??n hàng theo tr?ng thái
        /// </summary>
        /// <param name="status">Tr?ng thái ??n hàng</param>
        /// <returns>Danh sách ??n hàng v?i tr?ng thái t??ng ?ng</returns>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetByStatus(string status)
        {
            _logger.LogInformation("Getting orders with status: {Status}", status);

            // Validate status
            var (isValid, error) = OrderStatusValidator.Validate(status);
            if (!isValid)
            {
                _logger.LogWarning("Invalid status: {Status}. Error: {Error}", status, error);
                return BadRequest(new { message = error });
            }

            var orders = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(orders);
        }

        /// <summary>
        /// L?y các ??n hàng theo bàn ?n
        /// </summary>
        /// <param name="tableId">Table ID</param>
        /// <returns>Danh sách ??n hàng c?a bàn</returns>
        [HttpGet("table/{tableId}")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetByTableId(int tableId)
        {
            _logger.LogInformation("Getting orders for table: {TableId}", tableId);
            
            var orders = await _orderService.GetOrdersByTableIdAsync(tableId);
            return Ok(orders);
        }

        /// <summary>
        /// T?o ??n hàng m?i
        /// </summary>
        /// <param name="dto">Thông tin ??n hàng</param>
        /// <returns>??n hàng v?a t?o</returns>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
        {
            _logger.LogInformation("Creating new order for table: {TableName}", dto.TableName);

            // Validate DTO
            var (isValid, errors) = CreateOrderDtoValidator.Validate(dto);
            if (!isValid)
            {
                _logger.LogWarning("Validation failed for CreateOrderDto. Errors: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "D? li?u không h?p l?", errors });
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(dto);
                
                _logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);
                
                return CreatedAtAction(
                    nameof(GetById), 
                    new { id = order.Id }, 
                    order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for table: {TableName}", dto.TableName);
                return BadRequest(new { message = "L?i khi t?o ??n hàng", error = ex.Message });
            }
        }

        /// <summary>
        /// C?p nh?t thông tin ??n hàng
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="dto">Thông tin c?p nh?t</param>
        /// <returns>K?t qu? c?p nh?t</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto dto)
        {
            _logger.LogInformation("Updating order with ID: {OrderId}", id);

            try
            {
                var result = await _orderService.UpdateOrderAsync(id, dto);
                
                if (!result)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for update", id);
                    return NotFound(new { message = $"Không tìm th?y ??n hàng v?i ID {id}" });
                }

                _logger.LogInformation("Order {OrderId} updated successfully", id);
                return Ok(new { message = "C?p nh?t ??n hàng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order with ID: {OrderId}", id);
                return BadRequest(new { message = "L?i khi c?p nh?t ??n hàng", error = ex.Message });
            }
        }

        /// <summary>
        /// C?p nh?t tr?ng thái ??n hàng
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="dto">Tr?ng thái m?i</param>
        /// <returns>K?t qu? c?p nh?t</returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            _logger.LogInformation("Updating status for order {OrderId} to {NewStatus}", id, dto.NewStatus);

            // Validate status
            var (isValid, error) = OrderStatusValidator.Validate(dto.NewStatus);
            if (!isValid)
            {
                _logger.LogWarning("Invalid status: {Status}. Error: {Error}", dto.NewStatus, error);
                return BadRequest(new { message = error });
            }

            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(id, dto.NewStatus);
                
                if (!result)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found or invalid status transition", id);
                    return NotFound(new { message = $"Không tìm th?y ??n hàng ho?c không th? chuy?n tr?ng thái" });
                }

                _logger.LogInformation("Order {OrderId} status updated to {NewStatus}", id, dto.NewStatus);
                return Ok(new { message = "C?p nh?t tr?ng thái thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId}", id);
                return BadRequest(new { message = "L?i khi c?p nh?t tr?ng thái", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa ??n hàng
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>K?t qu? xóa</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting order with ID: {OrderId}", id);

            var result = await _orderService.DeleteOrderAsync(id);
            
            if (!result)
            {
                _logger.LogWarning("Order with ID {OrderId} not found for deletion", id);
                return NotFound(new { message = $"Không tìm th?y ??n hàng v?i ID {id}" });
            }

            _logger.LogInformation("Order {OrderId} deleted successfully", id);
            return Ok(new { message = "Xóa ??n hàng thành công" });
        }
    }
}
