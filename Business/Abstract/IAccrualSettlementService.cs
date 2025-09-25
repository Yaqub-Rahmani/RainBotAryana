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
    public interface IAccrualSettlementService
    {
        // Okuma
        IDataResult<List<AccrualSettlement>> GetAll();
        IDataResult<AccrualSettlement> GetById(Guid id);
        IDataResult<PagedResult<AccrualSettlementDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<AccrualSettlementDetailDto> GetDetailById(Guid id);

        // Yazma
        IResult Add(AccrualSettlement settlement);     // accrual bakiyesini düşürür
        IResult Delete(Guid id);                       // accrual bakiyesini geri arttırır (undo)
        IResult UpdateNote(Guid id, string? notes);    // sadece not güncelle
    }
}
