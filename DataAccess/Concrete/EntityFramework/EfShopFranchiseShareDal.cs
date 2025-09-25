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
    public class EfShopFranchiseShareDal : EfEntityRepositoryBase<ShopFranchiseShare, RainBotContext>, IShopFranchiseShareDal
    {
        public EfShopFranchiseShareDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<ShopFranchiseShare> Query() => _context.Set<ShopFranchiseShare>().AsQueryable();
        public IQueryable<ShopFranchiseShare> QueryNoTracking() => _context.Set<ShopFranchiseShare>().AsNoTracking();
    }
}
