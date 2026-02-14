using DataAccess.Abstract;
using Entities.Dtos.AdminUsers;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfAdminUserDal : IAdminUserDal
    {
        private readonly RainBotContext _context;
        public EfAdminUserDal(RainBotContext context)
        {
            _context = context;
        }

        public IQueryable<AdminUserListItemDto> QueryListItems()
        {
            return _context.Users
                .AsNoTracking()
                .Select(u => new AdminUserListItemDto
                {
                    Id = u.Id,
                    Name = (u.FirstName ?? "") + " " + (u.LastName ?? ""),
                    Status = u.Status,

                    // Kullanıcıya atanmış claim'ler içinden "ana rol" seçimi
                    Role = (
                        from uoc in _context.UserOperationClaims
                        join oc in _context.OperationClaims on uoc.OperationClaimId equals oc.Id
                        where uoc.UserId == u.Id
                        orderby
                            oc.Name == "superadmin" ? 0 :
                            oc.Name == "manager" ? 1 :
                            oc.Name == "editor" ? 2 :
                            oc.Name == "admin" ? 3 :
                            oc.Name =="other" ? 4:99,
                            oc.Name
                        select oc.Name
                    ).FirstOrDefault() ?? "admin"
                });
        }

        public IQueryable<AdminUserDetailDto> QueryDetails()
        {
            return _context.Users
                .AsNoTracking()
                .Select(u => new AdminUserDetailDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Status = u.Status,

                    Name = (u.FirstName ?? "") + " " + (u.LastName ?? ""),

                    Role = (
                        from uoc in _context.UserOperationClaims
                        join oc in _context.OperationClaims on uoc.OperationClaimId equals oc.Id
                        where uoc.UserId == u.Id
                        orderby
                            oc.Name == "superadmin" ? 0 :
                            oc.Name == "admin" ? 1 :
                            oc.Name == "accountant" ? 2 :
                            oc.Name == "viewer" ? 3 : 99,
                            oc.Name
                        select oc.Name
                    ).FirstOrDefault() ?? "admin"
                });
        }
    }
}
