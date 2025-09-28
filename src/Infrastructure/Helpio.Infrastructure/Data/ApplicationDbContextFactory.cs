using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Helpio.Ir.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            // Try to get connection string from environment variable first
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            
            // If not found, build configuration from appsettings
            if (string.IsNullOrEmpty(connectionString))
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .Build();
                
                connectionString = configuration.GetConnectionString("DefaultConnection");
            }
            
            // If still no connection string, use a default for Docker
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=helpio-sql,1433;Database=HelpioDB;User Id=helpio_app;Password=DefaultPassword123!;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
            }
            
            optionsBuilder.UseSqlServer(connectionString);
            
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}