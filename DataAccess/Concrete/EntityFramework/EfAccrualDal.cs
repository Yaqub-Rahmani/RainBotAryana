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
    public class EfAccrualDal : EfEntityRepositoryBase<Accrual, RainBotContext>, IAccrualDal
    {
        public EfAccrualDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<Accrual> Query() => _context.Set<Accrual>().AsQueryable();

        public IQueryable<Accrual> QueryNoTracking() => _context.Set<Accrual>().AsNoTracking();
    }
}
