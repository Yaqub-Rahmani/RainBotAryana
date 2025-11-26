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
    public class EfZakatPaymentEntryDal
        : EfEntityRepositoryBase<ZakatPaymentEntry, RainBotContext>, IZakatPaymentEntryDal
    {
        public EfZakatPaymentEntryDal(RainBotContext context) : base(context) { }

        public IQueryable<ZakatPaymentEntry> Query() =>
            _context.Set<ZakatPaymentEntry>().AsQueryable();

        public IQueryable<ZakatPaymentEntry> QueryNoTracking() =>
            _context.Set<ZakatPaymentEntry>().AsNoTracking();
    }

}
