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
    public class EfExpenseDal : EfEntityRepositoryBase<Expense, RainBotContext>, IExpenseDal
    {
        public EfExpenseDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<Expense> Query() => _context.Set<Expense>().AsQueryable();
        public IQueryable<Expense> QueryNoTracking() => _context.Set<Expense>().AsNoTracking();
    }
}
