using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Dtos.AdminUsers;
using System.Collections.Generic;

namespace Business.Abstract
{
    public interface IAdminUserService
    {
        IDataResult<PagedResult<AdminUserDetailDto>> GetDetails(QueryOptions opt, bool? onlyActive = null);
        IDataResult<List<AdminUserDetailDto>> GetAll(bool? onlyActive = null);
        IDataResult<AdminUserDetailDto> GetById(int id);

        IResult Add(AdminUserCreateDto dto);
        IResult Update(int id, AdminUserUpdateDto dto);
        IResult Delete(int id);
    }
}
