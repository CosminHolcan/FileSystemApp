using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class FileSystemAppDbContext : DbContext
    {
        public FileSystemAppDbContext() { }
        public FileSystemAppDbContext(DbContextOptions<FileSystemAppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<AppFile> AppFiles { get; set; }
        public DbSet<FileVersion> FileVersions { get; set; }
        public DbSet<StorageAccount> StorageAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppFile>()
                .HasMany(f => f.Versions)
                .WithOne(v => v.OriginalFile)
                .HasForeignKey(v => v.OriginalFileId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
