using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Dtos.employee;
using System;

namespace Business.Abstract
{
    public interface IEmployeeTransactionService
    {
        IDataResult<PagedResult<EmployeeTransactionListItemDto>> GetByEmployee(Guid employeeId, QueryOptions? opt, int? year = null, int? month = null);
        IResult Add(EmployeeTransactionCreateDto dto, int userId);
        IResult Delete(Guid id);
    }
}
