using Core.DataAccess;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IFranchiseBalanceEntryDal : IEntityRepository<FranchiseBalanceEntry>
    {
        IQueryable<FranchiseBalanceEntry> Query();
        IQueryable<FranchiseBalanceEntry> QueryNoTracking();
    }
}
