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
    public class EfSingleRatePolicyDal
        : EfEntityRepositoryBase<SingleRatePolicy, RainBotContext>, ISingleRatePolicyDal
    {
        public EfSingleRatePolicyDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<SingleRatePolicy> Query() => _context.Set<SingleRatePolicy>().AsQueryable();
        public IQueryable<SingleRatePolicy> QueryNoTracking() => _context.Set<SingleRatePolicy>().AsNoTracking();
    }
}
