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
    public class EfFloorDal : EfEntityRepositoryBase<Floor, RainBotContext>, IFloorDal
    {
        public EfFloorDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<Floor> Query() => _context.Set<Floor>().AsQueryable();
        public IQueryable<Floor> QueryNoTracking() => _context.Set<Floor>().AsNoTracking();
    }
}
