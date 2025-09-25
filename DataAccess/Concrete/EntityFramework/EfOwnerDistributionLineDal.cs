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
    public class EfOwnerDistributionLineDal : EfEntityRepositoryBase<OwnerDistributionLine, RainBotContext>, IOwnerDistributionLineDal
    {
        public EfOwnerDistributionLineDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<OwnerDistributionLine> Query() => _context.Set<OwnerDistributionLine>().AsQueryable();
        public IQueryable<OwnerDistributionLine> QueryNoTracking() => _context.Set<OwnerDistributionLine>().AsNoTracking();
    }
}
