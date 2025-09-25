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
    public interface IFamilyPayoutLineService
    {
        // Okuma
        IDataResult<List<FamilyPayoutLine>> GetAll(Guid? payoutId = null);
        IDataResult<FamilyPayoutLine> GetById(Guid id);
        IDataResult<PagedResult<FamilyPayoutLineDetailDto>> GetDetails(QueryOptions opt, Guid? payoutId = null);
        IDataResult<FamilyPayoutLineDetailDto> GetDetailById(Guid id);

        // Yazma
        IResult Add(FamilyPayoutLine line);        // BalanceEntry (In) oluşturur, header toplamını günceller
        IResult Update(FamilyPayoutLine line);     // BalanceEntry’yi günceller, header toplamını düzeltir
        IResult Delete(Guid id);                   // BalanceEntry’yi kaldırır, header toplamını düzeltir
    }
}
