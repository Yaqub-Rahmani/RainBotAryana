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
    public class EfEmployeeDal : EfEntityRepositoryBase<Employee, RainBotContext>, IEmployeeDal
    {
        public EfEmployeeDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<Employee> Query() => _context.Set<Employee>().AsQueryable();
        public IQueryable<Employee> QueryNoTracking() => _context.Set<Employee>().AsNoTracking();
    }
}
