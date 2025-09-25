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
    public class EfFranchiseBalanceEntryDal : EfEntityRepositoryBase<FranchiseBalanceEntry, RainBotContext>, IFranchiseBalanceEntryDal
    {
        public EfFranchiseBalanceEntryDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<FranchiseBalanceEntry> Query() => _context.Set<FranchiseBalanceEntry>().AsQueryable();
        public IQueryable<FranchiseBalanceEntry> QueryNoTracking() => _context.Set<FranchiseBalanceEntry>().AsNoTracking();
    }
}
