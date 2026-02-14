using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfShopDal : EfEntityRepositoryBase<Shop, RainBotContext>, IShopDal
    {
        public EfShopDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<Shop> Query() => _context.Set<Shop>().AsQueryable();

        public IQueryable<Shop> QueryNoTracking() => _context.Set<Shop>().AsNoTracking();

        public IQueryable<Shop> QueryWithIncludes()
        {
            // Navigation isimlerini kendi entity'ne göre düzelt:
            return QueryNoTracking()
                .Include(x => x.Market)
                .Include(x => x.Floor)
                .Include(x => x.Contracts)        // Shop.Contracts
                    .ThenInclude(c => c.Tenant)   // Contract.Tenant
                .Include(x => x.ShopFranchiseShares)  // ShopFranchiseShare koleksiyonu
                    .ThenInclude(fs => fs.FranchiseOwner);
        }
    }
}
