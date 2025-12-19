using Order.Application.DTOs;
using Order.Application.Services;
using Order.Infrastructure.Repositories;
using Order.Domain.Entities;
using Moq;
using Xunit;

namespace Order.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IOrderDetailRepository> _mockDetailRepository;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockDetailRepository = new Mock<IOrderDetailRepository>();
            _orderService = new OrderService(_mockOrderRepository.Object, _mockDetailRepository.Object);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ReturnsAllOrders()
        {
            // Arrange
            var orders = new List<OrderEntity>
            {
                new OrderEntity
                {
                    Id = 1,
                    OrderNumber = "ORD-001",
                    TableId = 1,
                    TableName = "Bàn 1",
                    Status = "ChoXacNhan"
                }
            };

            _mockOrderRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            Assert.NotEmpty(result);
            Assert.Single(result);
            Assert.Equal("ORD-001", result.First().OrderNumber);
        }

        [Fact]
        public async Task CreateOrderAsync_CreatesOrderSuccessfully()
        {
            // Arrange
            var createDto = new CreateOrderDto
            {
                TableId = 1,
                TableName = "Bàn 1",
                Items = new List<CreateOrderDetailDto>
                {
                    new CreateOrderDetailDto
                    {
                        DishId = 1,
                        DishName = "Ph? Bò",
                        Quantity = 2,
                        UnitPrice = 50000
                    }
                }
            };

            var createdOrder = new OrderEntity
            {
                Id = 1,
                OrderNumber = "ORD-240101120000",
                TableId = 1,
                TableName = "Bàn 1",
                TotalAmount = 100000,
                Status = "ChoXacNhan",
                CreatedAt = DateTime.Now,
                OrderDetails = new List<OrderDetailEntity>
                {
                    new OrderDetailEntity
                    {
                        DishId = 1,
                        DishName = "Ph? Bò",
                        Quantity = 2,
                        UnitPrice = 50000,
                        TotalPrice = 100000
                    }
                }
            };

            _mockOrderRepository.Setup(r => r.CreateAsync(It.IsAny<OrderEntity>()))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _orderService.CreateOrderAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100000, result.TotalAmount);
            Assert.Equal("ChoXacNhan", result.Status);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_UpdatesStatusSuccessfully()
        {
            // Arrange
            var orderId = 1;
            var newStatus = "DaXacNhan";

            var order = new OrderEntity
            {
                Id = orderId,
                Status = "ChoXacNhan"
            };

            _mockOrderRepository.Setup(r => r.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            _mockOrderRepository.Setup(r => r.UpdateAsync(It.IsAny<OrderEntity>()))
                .ReturnsAsync(true);

            // Act
            var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);

            // Assert
            Assert.True(result);
            _mockOrderRepository.Verify(r => r.UpdateAsync(It.IsAny<OrderEntity>()), Times.Once);
        }

        [Fact]
        public async Task DeleteOrderAsync_DeletesOrderSuccessfully()
        {
            // Arrange
            var orderId = 1;

            _mockOrderRepository.Setup(r => r.DeleteAsync(orderId))
                .ReturnsAsync(true);

            // Act
            var result = await _orderService.DeleteOrderAsync(orderId);

            // Assert
            Assert.True(result);
        }
    }
}
