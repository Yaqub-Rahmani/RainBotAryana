using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IContractService
    {
        // Okuma
        IDataResult<List<Contract>> GetAll();
        IDataResult<Contract> GetById(Guid id);
        IDataResult<PagedResult<ContractDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<ContractDetailDto> GetDetailById(Guid id);

        // Yazma
        IResult Add(Contract contract);
        IResult Update(Contract contract);
        IResult Delete(Guid id);

        // İşlemsel
        IResult Archive(Guid id, string? reason);
        IResult Activate(Guid id);
        IDataResult<bool> CheckOverlap(Guid shopId, Guid tenantId, DateOnly start, DateOnly end, Guid? excludeId = null);
    }
}
