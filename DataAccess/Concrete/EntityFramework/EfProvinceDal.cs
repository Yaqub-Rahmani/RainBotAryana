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
    public class EfProvinceDal : EfEntityRepositoryBase<Province, RainBotContext>, IProvinceDal
    {
        public EfProvinceDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<Province> Query() => _context.Set<Province>().AsQueryable();
        public IQueryable<Province> QueryNoTracking() => _context.Set<Province>().AsNoTracking();
    }
}
