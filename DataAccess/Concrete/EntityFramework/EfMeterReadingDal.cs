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
    public class EfMeterReadingDal : EfEntityRepositoryBase<MeterReading, RainBotContext>, IMeterReadingDal
    {
        public EfMeterReadingDal(RainBotContext ctx) : base(ctx) { }
        public IQueryable<MeterReading> Query() => _context.Set<MeterReading>().AsQueryable();
        public IQueryable<MeterReading> QueryNoTracking() => _context.Set<MeterReading>().AsNoTracking();
    }
}
