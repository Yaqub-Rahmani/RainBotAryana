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
    public class EfOwnerDistributionDal : EfEntityRepositoryBase<OwnerDistribution, RainBotContext>, IOwnerDistributionDal
    {
        public EfOwnerDistributionDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<OwnerDistribution> Query() => _context.Set<OwnerDistribution>().AsQueryable();
        public IQueryable<OwnerDistribution> QueryNoTracking() => _context.Set<OwnerDistribution>().AsNoTracking();
    }
}
