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
    public interface IFamilyMemberService
    {
        // Okumalar
        IDataResult<List<FamilyMember>> GetAll(bool? active = null);
        IDataResult<FamilyMember> GetById(Guid id);
        IDataResult<PagedResult<FamilyMemberDetailDto>> GetDetails(QueryOptions opt, bool? active = null);
        IDataResult<FamilyMemberDetailDto> GetDetailById(Guid id);

        // Yazmalar
        IResult Add(FamilyMember member);
        IResult Update(FamilyMember member);
        IResult Delete(Guid id);

        // İşlemsel
        IResult Activate(Guid id);
        IResult Deactivate(Guid id);
    }
}
