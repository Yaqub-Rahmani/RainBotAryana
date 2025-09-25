using Core.Utilities.Security.Hashing;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;

namespace WebAPI.Seed
{
    public static class DbInitializer
    {
        public static void Initialize(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<RainBotContext>();
            ctx.Database.Migrate();

            if (!ctx.OperationClaims.Any())
            {
                ctx.OperationClaims.AddRange(
                    new OperationClaim { Name = "admin" },
                    new OperationClaim { Name = "product.write" }  // örnek spesifik yetki
                );
                ctx.SaveChanges();
            }

            if (!ctx.Users.Any(u => u.Email == "admin@shop.local"))
            {
                HashingHelper.CreatePasswordHash("Admin#123!", out var hash, out var salt);
                var admin = new User
                {
                    FirstName = "System",
                    LastName = "Admin",
                    Email = "admin@shop.local",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Status = true
                };
                ctx.Users.Add(admin);
                ctx.SaveChanges();

                var adminClaimId = ctx.OperationClaims.First(x => x.Name == "admin").Id;
                var writeClaimId = ctx.OperationClaims.First(x => x.Name == "product.write").Id;

                ctx.UserOperationClaims.AddRange(
                    new UserOperationClaim { UserId = admin.Id, OperationClaimId = adminClaimId },
                    new UserOperationClaim { UserId = admin.Id, OperationClaimId = writeClaimId }
                );
                ctx.SaveChanges();
            }

            if (!ctx.Provinces.Any())
            {
                ctx.Provinces.AddRange(
                    new Province { Name = "Kabul", Code = "KBL", IsActive = true },
                    new Province { Name = "Herat", Code = "HER", IsActive = true },
                    new Province { Name = "Mazar-i-Sharif", Code = "MZR", IsActive = true }
                );
                ctx.SaveChanges();
            }


        }
    }
}
