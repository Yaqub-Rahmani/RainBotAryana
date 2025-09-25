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
    public class EfOwnerDistributionPolicyDal
        : EfEntityRepositoryBase<OwnerDistributionPolicy, RainBotContext>, IOwnerDistributionPolicyDal
    {
        public EfOwnerDistributionPolicyDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<OwnerDistributionPolicy> Query() => _context.Set<OwnerDistributionPolicy>().AsQueryable();
        public IQueryable<OwnerDistributionPolicy> QueryNoTracking() => _context.Set<OwnerDistributionPolicy>().AsNoTracking();
    }
}
