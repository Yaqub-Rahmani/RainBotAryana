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
    public class MarketMemberManager : IMarketMemberService
    {
        private readonly IMarketMemberDal _memberDal;

        public MarketMemberManager(IMarketMemberDal memberDal)
        {
            _memberDal = memberDal;
        }

        // ---------- Okumalar ----------
        [CacheAspect(10)]
        public IDataResult<List<MarketMember>> GetAll(bool? active = null)
        {
            var q = _memberDal.QueryNoTracking();
            if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);

            var list = q.OrderByDescending(x => x.IsActive).ThenBy(x => x.DisplayName).ToList();
            return new SuccessDataResult<List<MarketMember>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<MarketMember> GetById(Guid id)
        {
            var e = _memberDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<MarketMember>(null, "یافت نشد.") : new SuccessDataResult<MarketMember>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<MarketMemberDetailDto>> GetDetails(QueryOptions opt, bool? active = null)
        {
            opt ??= new QueryOptions();
            var q = _memberDal.QueryNoTracking();

            if (active.HasValue)
                q = q.Where(x => x.IsActive == active.Value);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => x.DisplayName.Contains(key) || (x.MemberType ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.IsActive).ThenBy(x => x.DisplayName);

            var projected = q.Select(x => new MarketMemberDetailDto
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                MemberType = x.MemberType,
                IsActive = x.IsActive
            });

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<MarketMemberDetailDto>>(
                new PagedResult<MarketMemberDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<MarketMemberDetailDto> GetDetailById(Guid id)
        {
            var dto = _memberDal.QueryNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new MarketMemberDetailDto
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    MemberType = x.MemberType,
                    IsActive = x.IsActive
                }).FirstOrDefault();

            return dto == null ? new ErrorDataResult<MarketMemberDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<MarketMemberDetailDto>(dto);
        }

        // ---------- Yazmalar ----------
        [SecuredOperation("marketmember.write,admin")]
        [ValidationAspect(typeof(MarketMemberValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMarketMemberService.Get")]
        public IResult Add(MarketMember member)
        {
            // isim çakışması (opsiyonel)
            var dup = _memberDal.Get(x => x.DisplayName.ToLower() == member.DisplayName.ToLower() && x.IsActive);
            if (dup != null) return new ErrorResult("عضو فعال با همین نام وجود دارد.");

            _memberDal.Add(member);
            return new SuccessResult("عضو بازار افزوده شد.");
        }

        [SecuredOperation("marketmember.write,admin")]
        [ValidationAspect(typeof(MarketMemberValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMarketMemberService.Get")]
        public IResult Update(MarketMember member)
        {
            var current = _memberDal.Get(x => x.Id == member.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            // isim çakışması (opsiyonel)
            var dup = _memberDal.Get(x => x.Id != member.Id &&
                                          x.DisplayName.ToLower() == member.DisplayName.ToLower() &&
                                          x.IsActive);
            if (dup != null) return new ErrorResult("عضو فعال با همین نام وجود دارد.");

            current.DisplayName = member.DisplayName;
            current.MemberType = member.MemberType;
            current.IsActive = member.IsActive;

            _memberDal.Update(current);
            return new SuccessResult("عضو بروزرسانی شد.");
        }

        [SecuredOperation("marketmember.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMarketMemberService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _memberDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            _memberDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        [SecuredOperation("marketmember.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMarketMemberService.Get")]
        public IResult Activate(Guid id)
        {
            var e = _memberDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.IsActive = true;
            _memberDal.Update(e);
            return new SuccessResult("فعال شد.");
        }

        [SecuredOperation("marketmember.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMarketMemberService.Get")]
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
