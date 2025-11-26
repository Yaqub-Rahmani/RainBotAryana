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
    public class EfCharityPaymentEntryDal
    : EfEntityRepositoryBase<CharityPaymentEntry, RainBotContext>, ICharityPaymentEntryDal
    {
        public EfCharityPaymentEntryDal(RainBotContext context) : base(context) { }

        public IQueryable<CharityPaymentEntry> Query()
            => _context.Set<CharityPaymentEntry>().AsQueryable();

        public IQueryable<CharityPaymentEntry> QueryNoTracking()
            => _context.Set<CharityPaymentEntry>().AsNoTracking();
    }
}
