using Microsoft.EntityFrameworkCore;

namespace Heronest.Database
{
    public class AppDbContext : DbContext
    {
        private string connectionString = string.Empty;

        public AppDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(this.connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
