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
    public class EfCollectionDal : EfEntityRepositoryBase<Collection, RainBotContext>, ICollectionDal
    {
        public EfCollectionDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<Collection> Query() => _context.Set<Collection>().AsQueryable();
        public IQueryable<Collection> QueryNoTracking() => _context.Set<Collection>().AsNoTracking();
    }
}
