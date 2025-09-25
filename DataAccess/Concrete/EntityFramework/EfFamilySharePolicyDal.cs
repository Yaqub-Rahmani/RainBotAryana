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
    public class EfFamilySharePolicyDal : EfEntityRepositoryBase<FamilySharePolicy, RainBotContext>, IFamilySharePolicyDal
    {
        public EfFamilySharePolicyDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<FamilySharePolicy> Query() => _context.Set<FamilySharePolicy>().AsQueryable();
        public IQueryable<FamilySharePolicy> QueryNoTracking() => _context.Set<FamilySharePolicy>().AsNoTracking();
    }
}
