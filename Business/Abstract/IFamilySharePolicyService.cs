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
    public interface IFamilySharePolicyService
    {
        // Okumalar
        IDataResult<List<FamilySharePolicy>> GetAll();
        IDataResult<FamilySharePolicy> GetById(Guid id);
        IDataResult<PagedResult<FamilySharePolicyDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<FamilySharePolicyDetailDto> GetDetailById(Guid id);

        // Aktif politika bul
        IDataResult<FamilySharePolicyDetailDto> GetActiveForDate(DateOnly onDate);

        // Yazmalar
        IResult Add(FamilySharePolicy policy);
        IResult Update(FamilySharePolicy policy);
        IResult Delete(Guid id);

        // Simülasyon (isteğe bağlı)
        IDataResult<object> Simulate(decimal totalAmount, int femaleCount, int maleCount, DateOnly? onDate = null);
    }
}
