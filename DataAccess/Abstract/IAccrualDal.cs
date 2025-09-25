using Core.DataAccess;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface IAccrualDal : IEntityRepository<Accrual>
    {
        IQueryable<Accrual> Query();           // zengin sorgular için
        IQueryable<Accrual> QueryNoTracking(); // performans
    }
}
