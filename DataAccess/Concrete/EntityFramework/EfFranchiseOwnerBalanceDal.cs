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
    public class EfFranchiseOwnerBalanceDal
        : EfEntityRepositoryBase<FranchiseOwnerBalance, RainBotContext>, IFranchiseOwnerBalanceDal
    {
        public EfFranchiseOwnerBalanceDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<FranchiseOwnerBalance> Query()
            => _context.Set<FranchiseOwnerBalance>().AsQueryable();

        public IQueryable<FranchiseOwnerBalance> QueryNoTracking()
            => _context.Set<FranchiseOwnerBalance>().AsNoTracking();
    }
}
