using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.Accrual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IAccrualService
    {
        IDataResult<List<Accrual>> GetAll(Guid? shopId = null, Guid? tenantId = null, int? year = null, int? month = null, AccrualStatus? status = null);
        IDataResult<Accrual> GetById(Guid id);
        IDataResult<AccrualDetailDto> GetDetail(Guid id);

        IResult Add(AccrualCreateDto dto, int userId);
        IResult Update(AccrualUpdateDto dto, int userId);
        IResult Delete(Guid id);

        // İşlemler
        IResult Settle(AccrualSettleDto dto, int userId); // kısmi/nihai
        IResult Reopen(Guid id, int userId);              // Settled -> Open (iş kuralına göre)
        IResult RecalculateOutstanding(Guid id);          // Amount / settlements bazlı yeniden hesap
    }
}
