using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Order.Infrastructure.Data
{
    public static class MigrationHelper
    {
        public static void ApplyMigrations(object app)
        {
            var serviceProvider = app.GetType().GetProperty("Services")?.GetValue(app) as IServiceProvider;
            if (serviceProvider == null)
                return;

            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                
                try
                {
                    dbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    // Log exception if needed
                    Console.WriteLine($"Migration error: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
