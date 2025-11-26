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
    public class EfTaxPaymentEntryDal
        : EfEntityRepositoryBase<TaxPaymentEntry, RainBotContext>, ITaxPaymentEntryDal
    {
        public EfTaxPaymentEntryDal(RainBotContext context) : base(context) { }

        public IQueryable<TaxPaymentEntry> Query()
            => _context.Set<TaxPaymentEntry>().AsQueryable();

        public IQueryable<TaxPaymentEntry> QueryNoTracking()
            => _context.Set<TaxPaymentEntry>().AsNoTracking();
    }
}
