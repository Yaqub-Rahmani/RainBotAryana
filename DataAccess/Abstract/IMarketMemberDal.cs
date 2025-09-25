using Core.DataAccess;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IMarketMemberDal : IEntityRepository<MarketMember>
    {
        IQueryable<MarketMember> Query();
        IQueryable<MarketMember> QueryNoTracking();
    }
}
