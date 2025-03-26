using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using server.DAL.Entities;

namespace server.DAL
{
    public class FileSystemAppDbContext : DbContext
    {
        public FileSystemAppDbContext() { }

        public FileSystemAppDbContext(DbContextOptions<FileSystemAppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<AppFile> AppFiles { get; set; }

        public DbSet<FileVersion> FileVersions { get; set; }

        public DbSet<StorageAccount> StorageAccounts { get; set; }
    }
}
