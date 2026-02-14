using Entities.Dtos.AdminUsers;
using System.Linq;

namespace DataAccess.Abstract
{
    public interface IAdminUserDal
    {
        IQueryable<AdminUserListItemDto> QueryListItems();
        IQueryable<AdminUserDetailDto> QueryDetails();
    }
}
