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
    public class EfPhoneDal : EfEntityRepositoryBase<Phone, RainBotContext>, IPhoneDal
    {
        public EfPhoneDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<Phone> Query() => _context.Set<Phone>().AsQueryable();
        public IQueryable<Phone> QueryNoTracking() => _context.Set<Phone>().AsNoTracking();
    }
}
