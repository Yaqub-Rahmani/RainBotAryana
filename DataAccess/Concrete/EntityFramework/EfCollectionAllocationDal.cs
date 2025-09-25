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
    public class EfCollectionAllocationDal : EfEntityRepositoryBase<CollectionAllocation, RainBotContext>, ICollectionAllocationDal
    {
        public EfCollectionAllocationDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<CollectionAllocation> Query() => _context.Set<CollectionAllocation>().AsQueryable();
        public IQueryable<CollectionAllocation> QueryNoTracking() => _context.Set<CollectionAllocation>().AsNoTracking();
    }
}
