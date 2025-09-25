using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Extensions;

namespace Business.Concrete
{
    public class FamilyMemberManager : IFamilyMemberService
    {
        private readonly IFamilyMemberDal _memberDal;

        public FamilyMemberManager(IFamilyMemberDal memberDal)
        {
            _memberDal = memberDal;
        }

        // ---------- Okumalar ----------

        [CacheAspect(10)]
        public IDataResult<List<FamilyMember>> GetAll(bool? active = null)
        {
            var q = _memberDal.QueryNoTracking();
            if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);
            var list = q.OrderByDescending(x => x.IsActive).ThenBy(x => x.DisplayName).ToList();
            return new SuccessDataResult<List<FamilyMember>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<FamilyMember> GetById(Guid id)
        {
            var e = _memberDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<FamilyMember>(null, "یافت نشد.") : new SuccessDataResult<FamilyMember>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<FamilyMemberDetailDto>> GetDetails(QueryOptions opt, bool? active = null)
        {
            opt ??= new QueryOptions();
            var q = _memberDal.QueryNoTracking();

            if (active.HasValue)
                q = q.Where(x => x.IsActive == active.Value);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => x.DisplayName.Contains(key) || (x.RelationTag ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.IsActive).ThenBy(x => x.DisplayName);

            var projected = q.Select(x => new FamilyMemberDetailDto
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                Gender = x.Gender.ToString(),
                RelationTag = x.RelationTag,
                IsActive = x.IsActive
            });

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<FamilyMemberDetailDto>>(
                new PagedResult<FamilyMemberDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<FamilyMemberDetailDto> GetDetailById(Guid id)
        {
            var dto = _memberDal.QueryNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new FamilyMemberDetailDto
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    Gender = x.Gender.ToString(),
                    RelationTag = x.RelationTag,
                    IsActive = x.IsActive
                })
                .FirstOrDefault();

            return dto == null ? new ErrorDataResult<FamilyMemberDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<FamilyMemberDetailDto>(dto);
        }

        // ---------- Yazmalar ----------

        [SecuredOperation("familymember.write,admin")]
        [ValidationAspect(typeof(FamilyMemberValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyMemberService.Get")]
        public IResult Add(FamilyMember member)
        {
            // aynı ad + aktif olan var mı (opsiyonel kontrol)
            var exists = _memberDal.Get(x => x.DisplayName.ToLower() == member.DisplayName.ToLower() && x.IsActive);
            if (exists != null) return new ErrorResult("عضو فعال با همین نام وجود دارد.");

            _memberDal.Add(member);
            return new SuccessResult("عضو خانواده افزوده شد.");
        }

        [SecuredOperation("familymember.write,admin")]
        [ValidationAspect(typeof(FamilyMemberValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyMemberService.Get")]
        public IResult Update(FamilyMember member)
        {
            var current = _memberDal.Get(x => x.Id == member.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            // isim çakışması (opsiyonel)
            var conflict = _memberDal.Get(x => x.Id != member.Id &&
                                               x.DisplayName.ToLower() == member.DisplayName.ToLower() &&
                                               x.IsActive);
            if (conflict != null) return new ErrorResult("عضو فعال با همین نام وجود دارد.");

            current.DisplayName = member.DisplayName;
            current.Gender = member.Gender;
            current.RelationTag = member.RelationTag;
            current.IsActive = member.IsActive;

            _memberDal.Update(current);
            return new SuccessResult("عضو بروزرسانی شد.");
        }

        [SecuredOperation("familymember.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyMemberService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _memberDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            _memberDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        [SecuredOperation("familymember.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyMemberService.Get")]
        public IResult Activate(Guid id)
        {
            var e = _memberDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.IsActive = true;
            _memberDal.Update(e);
            return new SuccessResult("فعال شد.");
        }

        [SecuredOperation("familymember.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyMemberService.Get")]
        public IResult Deactivate(Guid id)
        {
            var e = _memberDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.IsActive = false;
            _memberDal.Update(e);
            return new SuccessResult("غیرفعال شد.");
        }
    }
}
