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
    public class EfFranchiseOwnerDal : EfEntityRepositoryBase<FranchiseOwner, RainBotContext>, IFranchiseOwnerDal
    {
        public EfFranchiseOwnerDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<FranchiseOwner> Query() => _context.Set<FranchiseOwner>().AsQueryable();
        public IQueryable<FranchiseOwner> QueryNoTracking() => _context.Set<FranchiseOwner>().AsNoTracking();
    }
}
