using IDriveView.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace IDriveView.HelpCode
{
    public class ApplicationContext : DbContext
    {
        string pathFileDb = Path.Combine(Settings.folderSettings, "helloapp.db");
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UsersDownload> UsersDownloads { get; set; } = null!;
        public DbSet<WatchDownload> WatchDownloads { get; set; } = null!;
        public DbSet<UsersUpload> UsersUploads { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={pathFileDb}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserDownloadList)
                .WithOne(w => w.User)
                .HasForeignKey<UsersDownload>(w => w.UserId);
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserUploadList)
                .WithOne(w => w.User)
                .HasForeignKey<UsersUpload>(w => w.UserId);
            modelBuilder.Entity<User>()
                .HasOne(u => u.WatchDownloadList)
                .WithOne(w => w.User)
                .HasForeignKey<WatchDownload>(w => w.UserId);
        }
    }
}
