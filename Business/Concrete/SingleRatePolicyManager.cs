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
    public class SingleRatePolicyManager : ISingleRatePolicyService
    {
        private readonly ISingleRatePolicyDal _dal;
        public SingleRatePolicyManager(ISingleRatePolicyDal dal) { _dal = dal; }

        // Helpers
        private static SingleRatePolicyDetailDto Project(SingleRatePolicy p) => new()
        {
            Id = p.Id,
            Type = p.Type.ToString(),
            RatePercent = p.RatePercent,
            EffectiveFrom = p.EffectiveFrom,
            EffectiveTo = p.EffectiveTo,
            Notes = p.Notes
        };

        private bool Overlaps(SingleRateType type, DateOnly from, DateOnly? to, Guid? excludeId = null)
        {
            var q = _dal.QueryNoTracking().Where(x => x.Type == type);
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);

            // Kural: [p.From, p.To]  ile  [from, to] çakışır ⇔
            // p.From <= (to ?? +∞)  VE  (p.To == null || p.To >= from)
            return q.Any(p =>
                p.EffectiveFrom <= (to ?? p.EffectiveFrom) // to null ise bu şart p.EffectiveFrom <= +∞ olarak ele alınır
                && (p.EffectiveTo == null || p.EffectiveTo >= from)
            );
        }


        // Reads
        [CacheAspect(10)]
        public IDataResult<List<SingleRatePolicy>> GetAll(SingleRateType? type = null)
        {
            var q = _dal.QueryNoTracking();
            if (type.HasValue) q = q.Where(x => x.Type == type.Value);
            return new SuccessDataResult<List<SingleRatePolicy>>(q.OrderByDescending(x => x.EffectiveFrom).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<SingleRatePolicy> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<SingleRatePolicy>(null, "یافت نشد.")
                             : new SuccessDataResult<SingleRatePolicy>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<SingleRatePolicyDetailDto>> GetDetails(QueryOptions opt, SingleRateType? type = null)
        {
            opt ??= new QueryOptions();
            var q = _dal.QueryNoTracking();
            if (type.HasValue) q = q.Where(x => x.Type == type.Value);

            q = q.ApplyFilters(opt.Filters);

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

            return new SuccessDataResult<PagedResult<SingleRatePolicyDetailDto>>(
                new PagedResult<SingleRatePolicyDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<SingleRatePolicyDetailDto> GetDetailById(Guid id)
        {
            var dto = _dal.QueryNoTracking().Where(x => x.Id == id).Select(Project).FirstOrDefault();
            return dto == null ? new ErrorDataResult<SingleRatePolicyDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<SingleRatePolicyDetailDto>(dto);
        }

        [CacheAspect(5)]
        public IDataResult<SingleRatePolicyDetailDto> GetActive(SingleRateType type, DateOnly onDate)
        {
            var dto = _dal.QueryNoTracking()
                .Where(p => p.Type == type && p.EffectiveFrom <= onDate && (!p.EffectiveTo.HasValue || p.EffectiveTo.Value >= onDate))
                .OrderByDescending(p => p.EffectiveFrom)
                .Select(Project)
                .FirstOrDefault();

            return dto == null
                ? new ErrorDataResult<SingleRatePolicyDetailDto>(null, "سیاست فعالی برای این تاریخ یافت نشد.")
                : new SuccessDataResult<SingleRatePolicyDetailDto>(dto);
        }

        // Writes
        [SecuredOperation("singleratepolicy.write,admin")]
        [ValidationAspect(typeof(SingleRatePolicyValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ISingleRatePolicyService.Get")]
        public IResult Add(SingleRatePolicy policy)
        {
            if (Overlaps(policy.Type, policy.EffectiveFrom, policy.EffectiveTo))
                return new ErrorResult("تداخل بازهٔ زمانی برای این نوع سیاست وجود دارد.");
            _dal.Add(policy);
            return new SuccessResult("سیاست ثبت شد.");
        }

        [SecuredOperation("singleratepolicy.write,admin")]
        [ValidationAspect(typeof(SingleRatePolicyValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ISingleRatePolicyService.Get")]
        public IResult Update(SingleRatePolicy policy)
        {
            var current = _dal.Get(x => x.Id == policy.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            if (Overlaps(policy.Type, policy.EffectiveFrom, policy.EffectiveTo, policy.Id))
                return new ErrorResult("تداخل بازهٔ زمانی برای این نوع سیاست وجود دارد.");

            current.Type = policy.Type;
            current.RatePercent = policy.RatePercent;
            current.EffectiveFrom = policy.EffectiveFrom;
            current.EffectiveTo = policy.EffectiveTo;
            current.Notes = policy.Notes;

            _dal.Update(current);
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("singleratepolicy.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ISingleRatePolicyService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        // Simulation
        public IDataResult<object> Simulate(SingleRateType type, decimal baseAmount, DateOnly? onDate = null)
        {
            if (baseAmount < 0) return new ErrorDataResult<object>(null, "مبلغ منفی مجاز نیست.");

            SingleRatePolicy? p;
            if (onDate.HasValue)
            {
                var r = GetActive(type, onDate.Value);
                if (!r.Success) return new ErrorDataResult<object>(null, r.Message);
                p = new SingleRatePolicy { RatePercent = r.Data.RatePercent, Type = type };
            }
            else
            {
                p = _dal.QueryNoTracking()
                        .Where(x => x.Type == type)
                        .OrderByDescending(x => x.EffectiveFrom)
                        .FirstOrDefault();
                if (p == null) return new ErrorDataResult<object>(null, "سیاستی برای این نوع یافت نشد.");
            }

            decimal applied = Math.Round(baseAmount * p.RatePercent / 100m, 2, MidpointRounding.AwayFromZero);

            return new SuccessDataResult<object>(new
            {
                type = type.ToString(),
                baseAmount,
                ratePercent = p.RatePercent,
                result = applied
            });
        }
    }
}
