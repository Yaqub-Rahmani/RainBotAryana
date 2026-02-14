using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Validation;
using Core.Extensions;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete;
using Entities.Dtos.AdminUsers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    public class AdminUserManager : IAdminUserService
    {
        private readonly IAdminUserDal _adminUserDal;
        private readonly RainBotContext _ctx;

        private static readonly string[] RoleClaims = { "superadmin", "admin", "accountant", "viewer" };

        public AdminUserManager(IAdminUserDal adminUserDal, RainBotContext ctx)
        {
            _adminUserDal = adminUserDal;
            _ctx = ctx;
        }

        public IDataResult<PagedResult<AdminUserDetailDto>> GetDetails(QueryOptions opt, bool? onlyActive = null)
        {
            opt ??= new QueryOptions();

            var q = _adminUserDal.QueryDetails();

            if (onlyActive.HasValue)
                q = q.Where(x => x.Status == onlyActive.Value);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x =>
                    x.Name.Contains(key) ||
                    x.Email.Contains(key) ||
                    (x.PhoneNumber != null && x.PhoneNumber.Contains(key)) ||
                    x.Role.Contains(key)
                );
            }

            q = q.ApplyFilters(opt.Filters);
            q = q.ApplySort(opt.Sort);

            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderBy(x => x.Name);

            var total = q.Count();
            var items = q.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<AdminUserDetailDto>>(
                new PagedResult<AdminUserDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                }
            );
        }

        public IDataResult<List<AdminUserDetailDto>> GetAll(bool? onlyActive = null)
        {
            var q = _adminUserDal.QueryDetails();

            if (onlyActive.HasValue)
                q = q.Where(x => x.Status == onlyActive.Value);

            return new SuccessDataResult<List<AdminUserDetailDto>>(q.OrderBy(x => x.Name).ToList());
        }

        public IDataResult<AdminUserDetailDto> GetById(int id)
        {
            var item = _adminUserDal.QueryDetails().FirstOrDefault(x => x.Id == id);
            return item == null
                ? new ErrorDataResult<AdminUserDetailDto>(null, "یافت نشد.")
                : new SuccessDataResult<AdminUserDetailDto>(item);
        }

        [SecuredOperation("admin,superadmin")]
        [ValidationAspect(typeof(AdminUserCreateDtoValidator))]
        [TransactionScopeAspect]
        public IResult Add(AdminUserCreateDto dto)
        {
            var email = (dto.Email ?? "").Trim();
            if (_ctx.Users.Any(x => x.Email == email))
                return new ErrorResult("این ایمیل قبلاً ثبت شده است.");

            var role = NormalizeRole(dto.Role);
            EnsureRoleClaimsExist();

            HashingHelper.CreatePasswordHash(dto.Password, out var hash, out var salt);

            var user = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Status = dto.Status
            };

            _ctx.Users.Add(user);
            _ctx.SaveChanges();

            AssignRoleClaims(user.Id, role);

            return new SuccessResult("ثبت شد.");
        }

        [SecuredOperation("admin,superadmin")]
        [ValidationAspect(typeof(AdminUserUpdateDtoValidator))]
        [TransactionScopeAspect]
        public IResult Update(int id, AdminUserUpdateDto dto)
        {
            var user = _ctx.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
                return new ErrorResult("یافت نشد.");

            var email = (dto.Email ?? "").Trim();
            if (_ctx.Users.Any(x => x.Id != id && x.Email == email))
                return new ErrorResult("این ایمیل قبلاً ثبت شده است.");

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Email = email;
            user.Status = dto.Status;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                HashingHelper.CreatePasswordHash(dto.Password.Trim(), out var hash, out var salt);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            var role = NormalizeRole(dto.Role);
            EnsureRoleClaimsExist();

            // sadece role claim setini temizle (diğer permission claim'lere dokunma)
            RemoveExistingRoleClaims(id);

            // yeni rol claimlerini ata
            AssignRoleClaims(id, role);

            _ctx.SaveChanges();
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("admin,superadmin")]
        public IResult Delete(int id)
        {
            var user = _ctx.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
                return new ErrorResult("یافت نشد.");

            user.Status = false;
            _ctx.SaveChanges();
            return new SuccessResult("غیرفعال شد.");
        }

        // ---------------- helpers ----------------

        private static string NormalizeRole(string? role)
        {
            var r = (role ?? "admin").Trim().ToLowerInvariant();
            return RoleClaims.Contains(r) ? r : "admin";
        }

        private void EnsureRoleClaimsExist()
        {
            foreach (var name in RoleClaims)
            {
                if (!_ctx.OperationClaims.Any(x => x.Name == name))
                    _ctx.OperationClaims.Add(new OperationClaim { Name = name });
            }
            _ctx.SaveChanges();
        }

        private static List<string> ResolveRoleClaims(string role)
        {
            // superadmin seçilirse, admin + superadmin birlikte veriyoruz (ihtiyaca göre değiştirilebilir)
            return role switch
            {
                "superadmin" => new List<string> { "admin", "superadmin" },
                "accountant" => new List<string> { "accountant" },
                "viewer" => new List<string> { "viewer" },
                _ => new List<string> { "admin" }
            };
        }

        private void RemoveExistingRoleClaims(int userId)
        {
            var roleClaimIds = _ctx.OperationClaims
                .Where(x => RoleClaims.Contains(x.Name))
                .Select(x => x.Id)
                .ToList();

            var existing = _ctx.UserOperationClaims
                .Where(x => x.UserId == userId && roleClaimIds.Contains(x.OperationClaimId))
                .ToList();

            if (existing.Count > 0)
                _ctx.UserOperationClaims.RemoveRange(existing);
        }

        private void AssignRoleClaims(int userId, string role)
        {
            var claimNames = ResolveRoleClaims(role);

            var claimIds = _ctx.OperationClaims
                .Where(x => claimNames.Contains(x.Name))
                .Select(x => x.Id)
                .ToList();

            var existingIds = _ctx.UserOperationClaims
                .Where(x => x.UserId == userId)
                .Select(x => x.OperationClaimId)
                .ToList();

            foreach (var cid in claimIds.Distinct())
            {
                if (!existingIds.Contains(cid))
                    _ctx.UserOperationClaims.Add(new UserOperationClaim { UserId = userId, OperationClaimId = cid });
            }

            _ctx.SaveChanges();
        }
    }
}
