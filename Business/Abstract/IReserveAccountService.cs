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
    public interface IReserveAccountService
    {
        // Okuma
        IDataResult<List<ReserveAccount>> GetAll(ReserveAccountType? type = null, bool? active = null);
        IDataResult<ReserveAccount> GetById(Guid id);
        IDataResult<PagedResult<ReserveAccountDetailDto>> GetDetails(QueryOptions opt, ReserveAccountType? type = null, bool? active = null);
        IDataResult<ReserveAccountDetailDto> GetDetailById(Guid id);

        // Özet / Rapor
        IDataResult<List<ReserveAccountSummaryDto>> GetBalanceByAccount(DateTime? fromUtc = null, DateTime? toUtc = null);
        IDataResult<List<ReserveAccountSummaryDto>> GetBalanceByType(DateTime? fromUtc = null, DateTime? toUtc = null);

        // Yazma
        IResult Add(ReserveAccount account);
        IResult Update(ReserveAccount account);
        IResult Delete(Guid id);

        // İşlemsel
        IResult Activate(Guid id);
        IResult Deactivate(Guid id);
    }
}
