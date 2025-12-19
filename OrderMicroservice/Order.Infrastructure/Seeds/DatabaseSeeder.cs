using Order.Infrastructure.Data;
using Order.Domain.Entities;

namespace Order.Infrastructure.Seeds
{
    public static class DatabaseSeeder
    {
        public static void Seed(OrderDbContext context)
        {
            // Check if data already exists
            if (context.Orders.Any())
            {
                return;
            }

            var orders = new List<OrderEntity>
            {
                new OrderEntity
                {
                    OrderNumber = "ORD-240101000001",
                    TableId = 1,
                    TableName = "Bàn 1",
                    TotalAmount = 250000,
                    Status = "HoanThanh",
                    CustomerNote = "Không cay",
                    CreatedAt = DateTime.Now.AddHours(-2),
                    UpdatedAt = DateTime.Now.AddHours(-1),
                    CompletedAt = DateTime.Now.AddMinutes(-30),
                    OrderDetails = new List<OrderDetailEntity>
                    {
                        new OrderDetailEntity
                        {
                            DishId = 1,
                            DishName = "Ph? Bò",
                            Quantity = 2,
                            UnitPrice = 50000,
                            TotalPrice = 100000,
                            DishNote = "Thêm n??c m?m"
                        },
                        new OrderDetailEntity
                        {
                            DishId = 2,
                            DishName = "Bún Ch?",
                            Quantity = 1,
                            UnitPrice = 150000,
                            TotalPrice = 150000
                        }
                    }
                },
                new OrderEntity
                {
                    OrderNumber = "ORD-240101000002",
                    TableId = 2,
                    TableName = "Bàn 2",
                    TotalAmount = 180000,
                    Status = "DangChuan",
                    CustomerNote = "Ít mu?i",
                    CreatedAt = DateTime.Now.AddMinutes(-30),
                    UpdatedAt = DateTime.Now.AddMinutes(-10),
                    OrderDetails = new List<OrderDetailEntity>
                    {
                        new OrderDetailEntity
                        {
                            DishId = 3,
                            DishName = "C?m Gà",
                            Quantity = 3,
                            UnitPrice = 60000,
                            TotalPrice = 180000
                        }
                    }
                }
            };

            context.Orders.AddRange(orders);
            context.SaveChanges();

            Console.WriteLine("? Database seeded successfully");
        }
    }
}
