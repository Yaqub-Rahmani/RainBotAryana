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
    public class EfDollarRateDal : EfEntityRepositoryBase<DollarRate, RainBotContext>, IDollarRateDal
    {
        public EfDollarRateDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<DollarRate> Query() => _context.Set<DollarRate>().AsQueryable();
        public IQueryable<DollarRate> QueryNoTracking() => _context.Set<DollarRate>().AsNoTracking();
    }
}
