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

    public class EfMoneyAccountDal : EfEntityRepositoryBase<MoneyAccount, RainBotContext>, IMoneyAccountDal
    {
        public EfMoneyAccountDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<MoneyAccount> Query() => _context.Set<MoneyAccount>().AsQueryable();
        public IQueryable<MoneyAccount> QueryNoTracking() => _context.Set<MoneyAccount>().AsNoTracking();
    }
}
