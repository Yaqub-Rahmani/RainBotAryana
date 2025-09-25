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
    public interface IFranchiseOwnerService
    {
        // Okuma
        IDataResult<List<FranchiseOwner>> GetAll(bool? active = null);
        IDataResult<FranchiseOwner> GetById(Guid id);
        IDataResult<PagedResult<FranchiseOwnerDetailDto>> GetDetails(QueryOptions opt, bool? active = null);
        IDataResult<FranchiseOwnerDetailDto> GetDetailById(Guid id);

        // Yazma
        IResult Add(FranchiseOwner owner);
        IResult Update(FranchiseOwner owner);
        IResult Delete(Guid id);

        // İşlemsel
        IResult Activate(Guid id);
        IResult Deactivate(Guid id);
    }
}
