using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.AccrualSettlement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IAccrualSettlementService
    {
        // Okuma
        IDataResult<List<AccrualSettlement>> GetAll(Guid? accrualId = null);
        IDataResult<AccrualSettlement> GetById(Guid id);
        IDataResult<AccrualSettlementDetailDto> GetDetail(Guid id);

        IResult Add(AccrualSettlementCreateDto dto, int userId);
        IResult Update(AccrualSettlementUpdateDto dto, int userId);
        IResult Delete(Guid id, int userId);
    }
}
