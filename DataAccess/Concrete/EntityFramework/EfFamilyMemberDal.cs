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
    public class EfFamilyMemberDal : EfEntityRepositoryBase<FamilyMember, RainBotContext>, IFamilyMemberDal
    {
        public EfFamilyMemberDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<FamilyMember> Query() => _context.Set<FamilyMember>().AsQueryable();
        public IQueryable<FamilyMember> QueryNoTracking() => _context.Set<FamilyMember>().AsNoTracking();
    }
}
