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
    public class EfMarketDal : EfEntityRepositoryBase<Market, RainBotContext>, IMarketDal
    {
        public EfMarketDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<Market> Query() => _context.Set<Market>().AsQueryable();
        public IQueryable<Market> QueryNoTracking() => _context.Set<Market>().AsNoTracking();
    }
}
