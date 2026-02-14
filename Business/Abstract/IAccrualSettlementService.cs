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
        IDataResult<AccrualSettlementDto> GetById(Guid id);
        IDataResult<List<AccrualSettlementDto>> GetByAccrual(Guid accrualId);
        IDataResult<List<AccrualSettlementDto>> GetByAllocation(Guid collectionAllocationId);

        IResult Add(AccrualSettlementCreateDto dto, int userId);
    }
}
