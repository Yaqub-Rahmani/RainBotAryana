using Business.ValidationRules.FluentValidation.FamilyMember;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.FamilyMember;
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
        IDataResult<List<FamilyMemberDto>> GetAll(bool includeInactive = false);
        IDataResult<FamilyMemberDto> GetById(Guid id);


        IDataResult<List<FamilyMemberViewDto>> GetAllFamilyView(bool? active = null);

        // Profile (detail screen)
        Task<IResult> UpsertFingerprintAsync(Guid id, FamilyMemberFingerprintUpsertDto dto);


        IDataResult<FamilyMemberProfileDto> GetProfile(Guid familyMemberId);
        IResult Add(FamilyMemberCreateDto dto);
        IResult Update(FamilyMemberUpdateDto dto);
        IResult Delete(Guid id);
    }
}
