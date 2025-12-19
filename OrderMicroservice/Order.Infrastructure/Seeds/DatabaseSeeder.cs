using Order.Infrastructure.Data;
using Order.Domain.Entities;

namespace Order.Infrastructure.Seeds
{
    /// <summary>
    /// Database Seeder - Seed initial data for development/testing
    /// S? d?ng Factory Methods t? Domain Entities
    /// </summary>
    public static class DatabaseSeeder
    {
        public static void Seed(OrderDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Check if data already exists
            if (context.Orders.Any())
            {
                Console.WriteLine("?? Database already seeded. Skipping...");
                return;
            }

            // T?o Order 1 - ?ã hoàn thành
            var order1 = OrderEntity.Create(
                tableId: 1,
                tableName: "Bàn 1",
                customerNote: "Không cay"
            );

            order1.AddOrderDetail(
                dishId: 1,
                dishName: "Ph? Bò",
                quantity: 2,
                unitPrice: 50000,
                note: "Thêm n??c m?m"
            );

            order1.AddOrderDetail(
                dishId: 2,
                dishName: "Bún Ch?",
                quantity: 1,
                unitPrice: 150000
            );

            // Update status to completed
            order1.UpdateStatus("DaXacNhan");
            order1.UpdateStatus("DangChuan");
            order1.UpdateStatus("HoanThanh");

            // T?o Order 2 - ?ang chu?n b?
            var order2 = OrderEntity.Create(
                tableId: 2,
                tableName: "Bàn 2",
                customerNote: "Ít mu?i"
            );

            order2.AddOrderDetail(
                dishId: 3,
                dishName: "C?m Gà",
                quantity: 3,
                unitPrice: 60000
            );

            // Update status to cooking
            order2.UpdateStatus("DaXacNhan");
            order2.UpdateStatus("DangChuan");

            // T?o Order 3 - Ch? xác nh?n
            var order3 = OrderEntity.Create(
                tableId: 3,
                tableName: "Bàn 3",
                customerNote: "Giao nhanh"
            );

            order3.AddOrderDetail(
                dishId: 4,
                dishName: "Bánh Mì",
                quantity: 5,
                unitPrice: 20000
            );

            // Add orders to context
            context.Orders.AddRange(order1, order2, order3);
            context.SaveChanges();

            Console.WriteLine("? Database seeded successfully with 3 orders");
        }
    }
}
