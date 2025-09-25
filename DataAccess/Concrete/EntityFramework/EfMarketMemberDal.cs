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
    public class EfMarketMemberDal : EfEntityRepositoryBase<MarketMember, RainBotContext>, IMarketMemberDal
    {
        public EfMarketMemberDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<MarketMember> Query() => _context.Set<MarketMember>().AsQueryable();
        public IQueryable<MarketMember> QueryNoTracking() => _context.Set<MarketMember>().AsNoTracking();
    }
}
