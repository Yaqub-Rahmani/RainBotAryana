using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfEmployeeTransactionDal : EfEntityRepositoryBase<EmployeeTransaction, RainBotContext>, IEmployeeTransactionDal
    {
        public EfEmployeeTransactionDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<EmployeeTransaction> Query() => _context.Set<EmployeeTransaction>().AsQueryable();
        public IQueryable<EmployeeTransaction> QueryNoTracking() => _context.Set<EmployeeTransaction>().AsNoTracking();
    }
}
