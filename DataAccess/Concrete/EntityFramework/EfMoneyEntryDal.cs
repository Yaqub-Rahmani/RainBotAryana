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
    public class EfMoneyEntryDal : EfEntityRepositoryBase<MoneyEntry, RainBotContext>, IMoneyEntryDal
    {
        public EfMoneyEntryDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<MoneyEntry> Query() => _context.Set<MoneyEntry>().AsQueryable();
        public IQueryable<MoneyEntry> QueryNoTracking() => _context.Set<MoneyEntry>().AsNoTracking();
    }
}
