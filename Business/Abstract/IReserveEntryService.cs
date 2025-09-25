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
    public interface IReserveEntryService
    {
        // Okumalar
        IDataResult<List<ReserveEntry>> GetAll(Guid? accountId = null, ReserveEntryDirection? dir = null, DateTime? fromUtc = null, DateTime? toUtc = null);
        IDataResult<ReserveEntry> GetById(Guid id);
        IDataResult<PagedResult<ReserveEntryDetailDto>> GetDetails(QueryOptions opt, Guid? accountId = null, ReserveEntryDirection? dir = null, DateTime? fromUtc = null, DateTime? toUtc = null);
        IDataResult<ReserveEntryDetailDto> GetDetailById(Guid id);

        // Bakiye
        IDataResult<ReserveAccountBalanceDto> GetBalance(Guid accountId, DateTime? fromUtc = null, DateTime? toUtc = null);

        // Yazmalar
        IResult Add(ReserveEntry entry, bool allowOverdraw = false);
        IResult Update(ReserveEntry entry, bool allowOverdraw = false);
        IResult Delete(Guid id);
    }
}
