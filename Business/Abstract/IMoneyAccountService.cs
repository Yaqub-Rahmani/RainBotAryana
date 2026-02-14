using Core.Utilities.Results;
using Entities.Dtos.MoneyAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IMoneyAccountService
    {
        IDataResult<List<MoneyAccountDto>> GetAll(bool includeInactive = false);
        IDataResult<MoneyAccountDto> GetById(Guid id);

        IResult Add(MoneyAccountCreateDto dto);
        IResult Update(MoneyAccountUpdateDto dto);
        IResult Delete(Guid id);       // Soft delete: IsActive = false
    }
}
