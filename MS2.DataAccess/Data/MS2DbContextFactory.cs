using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MS2.DataAccess.Data;

/// <summary>
/// Factory for EF Core Tools (migrations, scaffold, etc.)
/// </summary>
public class MS2DbContextFactory : IDesignTimeDbContextFactory<MS2DbContext>
{
    public MS2DbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Build options
        var optionsBuilder = new DbContextOptionsBuilder<MS2DbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new MS2DbContext(optionsBuilder.Options);
    }
}