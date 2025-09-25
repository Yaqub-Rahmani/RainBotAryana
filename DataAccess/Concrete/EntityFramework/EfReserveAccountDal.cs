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
    public class EfReserveAccountDal : EfEntityRepositoryBase<ReserveAccount, RainBotContext>, IReserveAccountDal
    {
        public EfReserveAccountDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<ReserveAccount> Query() => _context.Set<ReserveAccount>().AsQueryable();
        public IQueryable<ReserveAccount> QueryNoTracking() => _context.Set<ReserveAccount>().AsNoTracking();
    }
}
