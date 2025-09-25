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
    public class EfFamilyBalanceEntryDal : EfEntityRepositoryBase<FamilyBalanceEntry, RainBotContext>, IFamilyBalanceEntryDal
    {
        public EfFamilyBalanceEntryDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<FamilyBalanceEntry> Query() => _context.Set<FamilyBalanceEntry>().AsQueryable();
        public IQueryable<FamilyBalanceEntry> QueryNoTracking() => _context.Set<FamilyBalanceEntry>().AsNoTracking();
    }
}
