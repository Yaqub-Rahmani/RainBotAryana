using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
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
using Core.Aspects.Autofac.Validation;

namespace Business.Concrete
{
    public class OwnerDistributionPolicyManager : IOwnerDistributionPolicyService
    {
        private readonly IOwnerDistributionPolicyDal _dal;

        public OwnerDistributionPolicyManager(IOwnerDistributionPolicyDal dal) { _dal = dal; }

        // ---- helpers ----
        private bool Overlaps(DateOnly from, DateOnly? to, Guid? excludeId = null)
        {
            var q = _dal.QueryNoTracking();
            if (excludeId.HasValue)
                q = q.Where(x => x.Id != excludeId.Value);

            // Çakışma: [p.From, p.To] × [from, to]
            // (to == null || p.From <= to) && (p.To == null || p.To >= from)
            return q.Any(p =>
                (to == null || p.EffectiveFrom <= to.Value) &&
                (p.EffectiveTo == null || p.EffectiveTo >= from)
            );
        }


        private static OwnerDistributionPolicyDetailDto Project(OwnerDistributionPolicy p) => new()
        {
            Id = p.Id,
            RateTax = p.RateTax,
            RateZakat = p.RateZakat,
            RateCharity = p.RateCharity,
            RateOwnerReserve = p.RateOwnerReserve,
            RateFamilyPool = p.RateFamilyPool,
            EffectiveFrom = p.EffectiveFrom,
            EffectiveTo = p.EffectiveTo,
            Notes = p.Notes
        };

        // ---- reads ----
        [CacheAspect(10)]
        public IDataResult<List<OwnerDistributionPolicy>> GetAll()
            => new SuccessDataResult<List<OwnerDistributionPolicy>>(
                _dal.QueryNoTracking().OrderByDescending(x => x.EffectiveFrom).ToList());

        [CacheAspect(10)]
        public IDataResult<OwnerDistributionPolicy> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<OwnerDistributionPolicy>(null, "یافت نشد.")
                             : new SuccessDataResult<OwnerDistributionPolicy>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<OwnerDistributionPolicyDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();
            var q = _dal.QueryNoTracking().ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.Notes ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.EffectiveFrom);

            var proj = q.Select(x => Project(x));
            var total = proj.Count();
            var items = proj.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<OwnerDistributionPolicyDetailDto>>(
                new PagedResult<OwnerDistributionPolicyDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<OwnerDistributionPolicyDetailDto> GetDetailById(Guid id)
        {
            var dto = _dal.QueryNoTracking().Where(x => x.Id == id).Select(Project).FirstOrDefault();
            return dto == null ? new ErrorDataResult<OwnerDistributionPolicyDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<OwnerDistributionPolicyDetailDto>(dto);
        }

        [CacheAspect(5)]
        public IDataResult<OwnerDistributionPolicyDetailDto> GetActiveForDate(DateOnly onDate)
        {
            var dto = _dal.QueryNoTracking()
                .Where(p => p.EffectiveFrom <= onDate && (!p.EffectiveTo.HasValue || p.EffectiveTo.Value >= onDate))
                .OrderByDescending(p => p.EffectiveFrom)
                .Select(Project)
                .FirstOrDefault();

            return dto == null
                ? new ErrorDataResult<OwnerDistributionPolicyDetailDto>(null, "برای این تاریخ سیاست فعالی یافت نشد.")
                : new SuccessDataResult<OwnerDistributionPolicyDetailDto>(dto);
        }

        // ---- writes ----
        [SecuredOperation("ownerdistributionpolicy.write,admin")]
        [ValidationAspect(typeof(OwnerDistributionPolicyValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IOwnerDistributionPolicyService.Get")]
        public IResult Add(OwnerDistributionPolicy policy)
        {
            try { policy.ValidateOrThrow(); } catch (Exception ex) { return new ErrorResult(ex.Message); }
            if (Overlaps(policy.EffectiveFrom, policy.EffectiveTo))
                return new ErrorResult("تداخل بازهٔ زمانی با سیاست‌های موجود.");

            _dal.Add(policy);
            return new SuccessResult("سیاست ثبت شد.");
        }

        [SecuredOperation("ownerdistributionpolicy.write,admin")]
        [ValidationAspect(typeof(OwnerDistributionPolicyValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IOwnerDistributionPolicyService.Get")]
        public IResult Update(OwnerDistributionPolicy policy)
        {
            var current = _dal.Get(x => x.Id == policy.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            try { policy.ValidateOrThrow(); } catch (Exception ex) { return new ErrorResult(ex.Message); }
            if (Overlaps(policy.EffectiveFrom, policy.EffectiveTo, policy.Id))
                return new ErrorResult("تداخل بازهٔ زمانی با سیاست‌های موجود.");

            current.RateTax = policy.RateTax;
            current.RateZakat = policy.RateZakat;
            current.RateCharity = policy.RateCharity;
            current.RateOwnerReserve = policy.RateOwnerReserve;
            current.RateFamilyPool = policy.RateFamilyPool;
            current.EffectiveFrom = policy.EffectiveFrom;
            current.EffectiveTo = policy.EffectiveTo;
            current.Notes = policy.Notes;

            _dal.Update(current);
            return new SuccessResult("سیاست بروزرسانی شد.");
        }

        [SecuredOperation("ownerdistributionpolicy.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IOwnerDistributionPolicyService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        // ---- Simulation ----
        public IDataResult<object> Simulate(decimal ownerAmount, DateOnly? onDate = null)
        {
            if (ownerAmount < 0) return new ErrorDataResult<object>(null, "مبلغ منفی نیست.");

            OwnerDistributionPolicy policy;
            if (onDate.HasValue)
            {
                var r = GetActiveForDate(onDate.Value);
                if (!r.Success) return new ErrorDataResult<object>(null, r.Message);
                policy = new OwnerDistributionPolicy
                {
                    RateTax = r.Data.RateTax,
                    RateZakat = r.Data.RateZakat,
                    RateCharity = r.Data.RateCharity,
                    RateOwnerReserve = r.Data.RateOwnerReserve,
                    RateFamilyPool = r.Data.RateFamilyPool
                };
            }
            else
            {
                var p = _dal.QueryNoTracking().OrderByDescending(x => x.EffectiveFrom).FirstOrDefault();
                if (p == null) return new ErrorDataResult<object>(null, "هیچ سیاستی ثبت نشده است.");
                policy = p;
            }

            decimal part(decimal rate) => Math.Round(ownerAmount * rate / 100m, 2, MidpointRounding.AwayFromZero);

            return new SuccessDataResult<object>(new
            {
                ownerAmount,
                policy.RateTax,
                policy.RateZakat,
                policy.RateCharity,
                policy.RateOwnerReserve,
                policy.RateFamilyPool,
                amountTax = part(policy.RateTax),
                amountZakat = part(policy.RateZakat),
                amountCharity = part(policy.RateCharity),
                amountOwnerReserve = part(policy.RateOwnerReserve),
                amountFamilyPool = part(policy.RateFamilyPool),
            });
        }
    }
}
