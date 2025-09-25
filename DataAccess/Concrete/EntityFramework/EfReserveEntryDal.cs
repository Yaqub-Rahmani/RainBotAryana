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
    public class EfReserveEntryDal : EfEntityRepositoryBase<ReserveEntry, RainBotContext>, IReserveEntryDal
    {
        public EfReserveEntryDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<ReserveEntry> Query() => _context.Set<ReserveEntry>().AsQueryable();
        public IQueryable<ReserveEntry> QueryNoTracking() => _context.Set<ReserveEntry>().AsNoTracking();
    }
}
