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
    public class EfContractDal : EfEntityRepositoryBase<Contract, RainBotContext>, IContractDal
    {
        public EfContractDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<Contract> Query() => _context.Set<Contract>().AsQueryable();
        public IQueryable<Contract> QueryNoTracking() => _context.Set<Contract>().AsNoTracking();
    }
}
