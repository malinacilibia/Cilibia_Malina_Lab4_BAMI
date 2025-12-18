using Cilibia_Malina_Lab4.Models;
using Microsoft.EntityFrameworkCore;

namespace Cilibia_Malina_Lab4.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        public DbSet<PredictionHistory> PredictionHistories { get; set; }
    }
}
