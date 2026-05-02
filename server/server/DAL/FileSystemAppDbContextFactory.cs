using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace server.DAL
{
    // Design-time factory used by EF Core tools (migrations, update-database) to create the DbContext
    // It looks for a connection string named "DbConnection" in appsettings.json/appsettings.Development.json
    // or in the environment variable "DbConnection". This avoids relying on the application's
    // Program.cs service provider at design time.
    public class FileSystemAppDbContextFactory : IDesignTimeDbContextFactory<FileSystemAppDbContext>
    {
        public FileSystemAppDbContext CreateDbContext(string[] args)
        {
            // Determine base path. EF tooling sets the working directory to the project directory
            // when running design-time services, so Directory.GetCurrentDirectory() should work.
            string basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                                .SetBasePath(basePath)
                                .AddJsonFile("appsettings.json", optional: true)
                                .AddJsonFile("appsettings.Development.json", optional: true)
                                .AddEnvironmentVariables()
                                .Build();

            string? connectionString = config.GetConnectionString("DbConnection") ?? Environment.GetEnvironmentVariable("DbConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DbConnection' not found in configuration or environment variables.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<FileSystemAppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new FileSystemAppDbContext(optionsBuilder.Options);
        }
    }
}
