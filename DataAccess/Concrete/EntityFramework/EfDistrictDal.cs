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
    public class EfDistrictDal : EfEntityRepositoryBase<District, RainBotContext>, IDistrictDal
    {
        public EfDistrictDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<District> Query() => _context.Set<District>().AsQueryable();
        public IQueryable<District> QueryNoTracking() => _context.Set<District>().AsNoTracking();
    }
}
