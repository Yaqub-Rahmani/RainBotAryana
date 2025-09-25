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
    public class EfFamilyPayoutLineDal : EfEntityRepositoryBase<FamilyPayoutLine, RainBotContext>, IFamilyPayoutLineDal
    {
        public EfFamilyPayoutLineDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<FamilyPayoutLine> Query() => _context.Set<FamilyPayoutLine>().AsQueryable();
        public IQueryable<FamilyPayoutLine> QueryNoTracking() => _context.Set<FamilyPayoutLine>().AsNoTracking();
    }
}
