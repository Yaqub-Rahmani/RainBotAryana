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
    public class EfBiometricDal : EfEntityRepositoryBase<Biometric, RainBotContext>, IBiometricDal
    {
        public EfBiometricDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<Biometric> Query() => _context.Set<Biometric>().AsQueryable();
        public IQueryable<Biometric> QueryNoTracking() => _context.Set<Biometric>().AsNoTracking();
    }
}
