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
    public class EfEmployeeWorkInfoDal : EfEntityRepositoryBase<EmployeeWorkInfo, RainBotContext>, IEmployeeWorkInfoDal
    {
        public EfEmployeeWorkInfoDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<EmployeeWorkInfo> Query() => _context.Set<EmployeeWorkInfo>().AsQueryable();
        public IQueryable<EmployeeWorkInfo> QueryNoTracking() => _context.Set<EmployeeWorkInfo>().AsNoTracking();
    }
}
