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
    public class EfTenantDal : EfEntityRepositoryBase<Tenant, RainBotContext>, ITenantDal
    {
        public EfTenantDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<Tenant> Query() => _context.Set<Tenant>().AsQueryable();
        public IQueryable<Tenant> QueryNoTracking() => _context.Set<Tenant>().AsNoTracking();
    }
}
