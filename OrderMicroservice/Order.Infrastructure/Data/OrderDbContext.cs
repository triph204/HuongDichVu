using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<OrderEntity> Orders { get; set; } = null!;
        public DbSet<OrderDetailEntity> OrderDetails { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Order entity
            modelBuilder.Entity<OrderEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TableName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
                entity.Property(e => e.CustomerNote).HasMaxLength(500);
                entity.HasMany(e => e.OrderDetails).WithOne(d => d.Order).HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OrderDetail entity
            modelBuilder.Entity<OrderDetailEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DishName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
                entity.Property(e => e.DishNote).HasMaxLength(300);
            });
        }
    }
}
