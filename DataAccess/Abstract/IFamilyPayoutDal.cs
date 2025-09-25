using Core.DataAccess;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IFamilyPayoutDal : IEntityRepository<FamilyPayout>
    {
        IQueryable<FamilyPayout> Query();
        IQueryable<FamilyPayout> QueryNoTracking();
    }
}
