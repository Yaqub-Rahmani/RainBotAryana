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
    public class EfAccrualSettlementDal : EfEntityRepositoryBase<AccrualSettlement, RainBotContext>, IAccrualSettlementDal
    {
        public EfAccrualSettlementDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<AccrualSettlement> Query() => _context.Set<AccrualSettlement>().AsQueryable();
        public IQueryable<AccrualSettlement> QueryNoTracking() => _context.Set<AccrualSettlement>().AsNoTracking();
    }
}
