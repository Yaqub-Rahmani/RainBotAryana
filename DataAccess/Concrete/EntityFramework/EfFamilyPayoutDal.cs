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
    public class EfFamilyPayoutDal : EfEntityRepositoryBase<FamilyPayout, RainBotContext>, IFamilyPayoutDal
    {
        public EfFamilyPayoutDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<FamilyPayout> Query() => _context.Set<FamilyPayout>().AsQueryable();
        public IQueryable<FamilyPayout> QueryNoTracking() => _context.Set<FamilyPayout>().AsNoTracking();
    }
}
