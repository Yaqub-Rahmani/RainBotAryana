using Core.DataAccess;
using Entities.Concrete;
using System.Linq;

namespace DataAccess.Abstract
{
    public interface IEmployeeTransactionDal : IEntityRepository<EmployeeTransaction>
    {
        IQueryable<EmployeeTransaction> Query();
        IQueryable<EmployeeTransaction> QueryNoTracking();
    }
}
