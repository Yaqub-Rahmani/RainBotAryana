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
    public class EfAddressDal : EfEntityRepositoryBase<Address, RainBotContext>, IAddressDal
    {
        public EfAddressDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<Address> Query() => _context.Set<Address>().AsQueryable();
        public IQueryable<Address> QueryNoTracking() => _context.Set<Address>().AsNoTracking();

        public IList<Address> GetByOwner(MediaOwnerType ownerType, Guid ownerId)
            => _context.Set<Address>().Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId).ToList();
    }
}
