using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    // dotnet ef komutları çalışırken DbContext'i oluşturmak için kullanılır
    public class RainBotContextFactory : IDesignTimeDbContextFactory<RainBotContext>
    {
        public RainBotContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RainBotContext>();
            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=RainBotAryanaDb;Trusted_Connection=True;TrustServerCertificate=True");

            return new RainBotContext(optionsBuilder.Options);
        }
    }
}
