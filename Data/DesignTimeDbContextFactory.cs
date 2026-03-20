using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RegentHealth.Data
{
    // EF Tools uses this class at design time (Add-Migration, Update-Database)
    // instead of trying to launch the WPF application
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=regenthealth.db")
                .Options;

            return new AppDbContext(options);
        }
    }
}