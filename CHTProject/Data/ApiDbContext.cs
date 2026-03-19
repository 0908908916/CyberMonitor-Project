using Microsoft.EntityFrameworkCore;
using CHTProject.Models;

namespace CHTProject.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }

        public DbSet<LogItem> Logs { get; set; } // 資料庫裡的資料表
    }
}