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
    public class FamilySharePolicyManager : IFamilySharePolicyService
    {
        private readonly IFamilySharePolicyDal _dal;

        public FamilySharePolicyManager(IFamilySharePolicyDal dal) { _dal = dal; }

        // ------------- Okumalar -------------

        [CacheAspect(10)]
        public IDataResult<List<FamilySharePolicy>> GetAll()
            => new SuccessDataResult<List<FamilySharePolicy>>(
                _dal.QueryNoTracking().OrderByDescending(x => x.EffectiveFrom).ToList()
            );

        [CacheAspect(10)]
        public IDataResult<FamilySharePolicy> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<FamilySharePolicy>(null, "یافت نشد.") : new SuccessDataResult<FamilySharePolicy>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<FamilySharePolicyDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();
            var q = _dal.QueryNoTracking();

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.Notes ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.EffectiveFrom);

            var projected = q.Select(p => new FamilySharePolicyDetailDto
            {
                Id = p.Id,
                ParentPercent = p.ParentPercent,
                FemaleWeight = p.FemaleWeight,
                MaleWeight = p.MaleWeight,
                EffectiveFrom = p.EffectiveFrom,
                EffectiveTo = p.EffectiveTo,
                Notes = p.Notes
            });

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<FamilySharePolicyDetailDto>>(
                new PagedResult<FamilySharePolicyDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<FamilySharePolicyDetailDto> GetDetailById(Guid id)
        {
            var dto = _dal.QueryNoTracking().Where(x => x.Id == id)
                .Select(p => new FamilySharePolicyDetailDto
                {
                    Id = p.Id,
                    ParentPercent = p.ParentPercent,
                    FemaleWeight = p.FemaleWeight,
                    MaleWeight = p.MaleWeight,
                    EffectiveFrom = p.EffectiveFrom,
                    EffectiveTo = p.EffectiveTo,
                    Notes = p.Notes
                })
                .FirstOrDefault();

            return dto == null ? new ErrorDataResult<FamilySharePolicyDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<FamilySharePolicyDetailDto>(dto);
        }

        // ------------- Aktif politika -------------
        [CacheAspect(5)]
        public IDataResult<FamilySharePolicyDetailDto> GetActiveForDate(DateOnly onDate)
        {
            var dto = _dal.QueryNoTracking()
                .Where(p => p.EffectiveFrom <= onDate && (!p.EffectiveTo.HasValue || p.EffectiveTo.Value >= onDate))
                .OrderByDescending(p => p.EffectiveFrom)
                .Select(p => new FamilySharePolicyDetailDto
                {
                    Id = p.Id,
                    ParentPercent = p.ParentPercent,
                    FemaleWeight = p.FemaleWeight,
                    MaleWeight = p.MaleWeight,
                    EffectiveFrom = p.EffectiveFrom,
                    EffectiveTo = p.EffectiveTo,
                    Notes = p.Notes
                }).FirstOrDefault();

            return dto == null
                ? new ErrorDataResult<FamilySharePolicyDetailDto>(null, "برای این تاریخ سیاست فعالی یافت نشد.")
                : new SuccessDataResult<FamilySharePolicyDetailDto>(dto);
        }

        // ------------- Yazmalar -------------

        private bool HasOverlap(DateOnly from, DateOnly? to, Guid? excludeId = null)
        {
            var q = _dal.QueryNoTracking();
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);

            // Çakışma koşulu (EF-çevirilebilir): 
            // [p.From, p.To]  ×  [from, to]  ⇔  (to == null || p.From <= to) && (p.To == null || p.To >= from)
            return q.Any(p =>
                (to == null || p.EffectiveFrom <= to.Value) &&
                (p.EffectiveTo == null || p.EffectiveTo >= from)
            );
        }


        [SecuredOperation("familysharepolicy.write,admin")]
        [ValidationAspect(typeof(FamilySharePolicyValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilySharePolicyService.Get")]
        public IResult Add(FamilySharePolicy policy)
        {
            try { policy.ValidateOrThrow(); } catch (Exception ex) { return new ErrorResult(ex.Message); }

            if (HasOverlap(policy.EffectiveFrom, policy.EffectiveTo))
                return new ErrorResult("تداخل بازهٔ زمانی با سیاست‌های موجود.");

            _dal.Add(policy);
            return new SuccessResult("سیاست ثبت شد.");
        }

        [SecuredOperation("familysharepolicy.write,admin")]
        [ValidationAspect(typeof(FamilySharePolicyValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilySharePolicyService.Get")]
        public IResult Update(FamilySharePolicy policy)
        {
            var current = _dal.Get(x => x.Id == policy.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            try { policy.ValidateOrThrow(); } catch (Exception ex) { return new ErrorResult(ex.Message); }

            if (HasOverlap(policy.EffectiveFrom, policy.EffectiveTo, policy.Id))
                return new ErrorResult("تداخل بازهٔ زمانی با سیاست‌های موجود.");

            current.ParentPercent = policy.ParentPercent;
            current.FemaleWeight = policy.FemaleWeight;
            current.MaleWeight = policy.MaleWeight;
            current.EffectiveFrom = policy.EffectiveFrom;
            current.EffectiveTo = policy.EffectiveTo;
            current.Notes = policy.Notes;

            _dal.Update(current);
            return new SuccessResult("سیاست بروزرسانی شد.");
        }

        [SecuredOperation("familysharepolicy.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilySharePolicyService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        // ------------- Simülasyon -------------
        // toplam = parentPart + remain;  parentPart = total * (ParentPercent/100)
        // remain ağırlıklarla paylaştırılır.
        public IDataResult<object> Simulate(decimal totalAmount, int femaleCount, int maleCount, DateOnly? onDate = null)
        {
            if (totalAmount < 0) return new ErrorDataResult<object>(null, "مبلغ منفی نیست.");
            if (femaleCount < 0 || maleCount < 0) return new ErrorDataResult<object>(null, "تعداد منفی نیست.");

            FamilySharePolicy policy;
            if (onDate.HasValue)
            {
                var r = GetActiveForDate(onDate.Value);
                if (!r.Success) return new ErrorDataResult<object>(null, r.Message);
                policy = new FamilySharePolicy
                {
                    ParentPercent = r.Data.ParentPercent,
                    FemaleWeight = r.Data.FemaleWeight,
                    MaleWeight = r.Data.MaleWeight
                };
            }
            else
            {
                // En son politika (EffectiveFrom en yeni)
                var p = _dal.QueryNoTracking().OrderByDescending(x => x.EffectiveFrom).FirstOrDefault();
                if (p == null) return new ErrorDataResult<object>(null, "هیچ سیاستی ثبت نشده است.");
                policy = p;
            }

            var parentPart = Math.Round(totalAmount * (policy.ParentPercent / 100m), 2, MidpointRounding.AwayFromZero);
            var remain = Math.Round(totalAmount - parentPart, 2, MidpointRounding.AwayFromZero);

            var totalWeight = (policy.FemaleWeight * femaleCount) + (policy.MaleWeight * maleCount);
            decimal unit = totalWeight > 0 ? Math.Round(remain / totalWeight, 6, MidpointRounding.AwayFromZero) : 0m;

            return new SuccessDataResult<object>(new
            {
                totalAmount,
                parentPercent = policy.ParentPercent,
                parentPart,
                remain,
                femaleWeight = policy.FemaleWeight,
                maleWeight = policy.MaleWeight,
                femaleCount,
                maleCount,
                unitPerWeight = unit,
                perFemale = Math.Round(unit * policy.FemaleWeight, 2, MidpointRounding.AwayFromZero),
                perMale = Math.Round(unit * policy.MaleWeight, 2, MidpointRounding.AwayFromZero)
            });
        }
    }
}
