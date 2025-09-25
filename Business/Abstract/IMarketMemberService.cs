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
    public interface IMarketMemberService
    {
        // Okumalar
        IDataResult<List<MarketMember>> GetAll(bool? active = null);
        IDataResult<MarketMember> GetById(Guid id);
        IDataResult<PagedResult<MarketMemberDetailDto>> GetDetails(QueryOptions opt, bool? active = null);
        IDataResult<MarketMemberDetailDto> GetDetailById(Guid id);

        // Yazmalar
        IResult Add(MarketMember member);
        IResult Update(MarketMember member);
        IResult Delete(Guid id);

        // İşlemsel
        IResult Activate(Guid id);
        IResult Deactivate(Guid id);
    }
}
