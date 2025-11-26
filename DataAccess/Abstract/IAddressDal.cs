using Core.DataAccess;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IAddressDal : IEntityRepository<Address>
    {
        IQueryable<Address> Query();
        IQueryable<Address> QueryNoTracking();
        IList<Address> GetByOwner(MediaOwnerType ownerType, Guid ownerId);
    }
}
