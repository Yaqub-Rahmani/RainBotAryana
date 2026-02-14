using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Dtos.Expense;
using System;

namespace Business.Abstract
{
    public interface IExpenseService
    {
        IDataResult<PagedResult<ExpenseDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<ExpenseDetailDto> GetById(Guid id);

        IDataResult<decimal> GetMoneyAccountBalance(Guid moneyAccountId);

        IResult Create(ExpenseCreateFromUiDto dto, int userId);
        IResult Update(Guid id, ExpenseUpdateFromUiDto dto, int userId);
        IResult Delete(Guid id, int userId);
    }
}
