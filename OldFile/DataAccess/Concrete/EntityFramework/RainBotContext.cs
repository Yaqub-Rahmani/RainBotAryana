using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class RainBotContext : DbContext
    {
        // DesignTimeFactory için options alan bir kurucu
        public RainBotContext(DbContextOptions<RainBotContext> options) : base(options) { }

        public RainBotContext() { } // EfEntityRepositoryBase<TContext:new()> için parametresiz kurucu da dursun

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Geliştirme için hızlı başlangıç:
                optionsBuilder.UseSqlServer(
                    "Server=localhost;Database=RainBotAryanaDb;Trusted_Connection=True;TrustServerCertificate=True");
            }
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
