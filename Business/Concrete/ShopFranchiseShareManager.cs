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
    public class ShopFranchiseShareManager : IShopFranchiseShareService
    {
        private readonly IShopFranchiseShareDal _dal;
        private readonly IShopDal _shopDal;
        private readonly IFranchiseOwnerDal _ownerDal;

        public ShopFranchiseShareManager(IShopFranchiseShareDal dal, IShopDal shopDal, IFranchiseOwnerDal ownerDal)
        {
            _dal = dal;
            _shopDal = shopDal;
            _ownerDal = ownerDal;
        }

        // ---------- Helpers ----------
        private static bool Overlaps(DateOnly from1, DateOnly? to1, DateOnly from2, DateOnly? to2)
        {
            var a2 = to1 ?? DateOnly.MaxValue;
            var b2 = to2 ?? DateOnly.MaxValue;
            return !(b2 < from1 || from2 > a2);
        }

        //private bool HasOverlapForOwner(ShopFranchiseShare e, Guid? excludeId = null)
        //{

        //    var q = _dal.QueryNoTracking().Where(x => x.ShopId == e.ShopId && x.FranchiseOwnerId == e.FranchiseOwnerId);
        //    if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
        //    return q.Any(x => Overlaps(e.EffectiveFrom, e.EffectiveTo, x.EffectiveFrom, x.EffectiveTo));
        //}

        private bool HasOverlapForOwner(ShopFranchiseShare e, Guid? excludeId = null)
        {
            var from = e.EffectiveFrom;
            var to = e.EffectiveTo ?? DateOnly.MaxValue;

            var q = _dal.QueryNoTracking()
                .Where(x => x.ShopId == e.ShopId && x.FranchiseOwnerId == e.FranchiseOwnerId);

            if (excludeId.HasValue)
                q = q.Where(x => x.Id != excludeId.Value);

            // ⛔ Overlaps(...) YOK
            // ✅ Tamamen translatable koşul:
            return q.Any(x =>
                x.EffectiveFrom <= to &&
                from <= (x.EffectiveTo ?? DateOnly.MaxValue)
            );
        }


        private decimal TotalPercentOn(Guid shopId, DateOnly on, Guid? excludeId = null)
        {
            var q = _dal.QueryNoTracking().Where(x => x.ShopId == shopId
                && x.EffectiveFrom <= on && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= on));
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
            return q.Sum(x => (decimal?)x.SharePercent) ?? 0m;
        }

        private ShopFranchiseShareDetailDto Project(ShopFranchiseShare s, DateOnly? on = null)
        {
            var shop = _shopDal.QueryNoTracking().FirstOrDefault(x => x.Id == s.ShopId);
            var owner = _ownerDal.QueryNoTracking().FirstOrDefault(x => x.Id == s.FranchiseOwnerId);
            bool isActive = on.HasValue ? (s.EffectiveFrom <= on.Value && (!s.EffectiveTo.HasValue || s.EffectiveTo.Value >= on.Value)) : false;
            return new ShopFranchiseShareDetailDto
            {
                Id = s.Id,
                ShopId = s.ShopId,
                ShopName = shop?.Name ?? shop?.Id.ToString() ?? "",
                FranchiseOwnerId = s.FranchiseOwnerId,
                FranchiseOwnerName = owner?.DisplayName ?? "",
                SharePercent = s.SharePercent,
                EffectiveFrom = s.EffectiveFrom,
                EffectiveTo = s.EffectiveTo,
                Notes = s.Notes,
                IsActiveOn = isActive
            };
        }

        // ---------- Reads ----------
        [CacheAspect(10)]
        public IDataResult<List<ShopFranchiseShare>> GetAll(Guid? shopId = null, Guid? franchiseOwnerId = null)
        {
            var q = _dal.QueryNoTracking();
            if (shopId.HasValue && shopId.Value != Guid.Empty) q = q.Where(x => x.ShopId == shopId.Value);
            if (franchiseOwnerId.HasValue && franchiseOwnerId.Value != Guid.Empty) q = q.Where(x => x.FranchiseOwnerId == franchiseOwnerId.Value);

            return new SuccessDataResult<List<ShopFranchiseShare>>(q.OrderByDescending(x => x.EffectiveFrom).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<ShopFranchiseShare> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<ShopFranchiseShare>(null, "یافت نشد.")
                             : new SuccessDataResult<ShopFranchiseShare>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<ShopFranchiseShareDetailDto>> GetDetails(QueryOptions opt, Guid? shopId = null, Guid? franchiseOwnerId = null, DateOnly? on = null)
        {
            opt ??= new QueryOptions();
            var q = _dal.QueryNoTracking();

            if (shopId.HasValue && shopId.Value != Guid.Empty) q = q.Where(x => x.ShopId == shopId.Value);
            if (franchiseOwnerId.HasValue && franchiseOwnerId.Value != Guid.Empty) q = q.Where(x => x.FranchiseOwnerId == franchiseOwnerId.Value);
            if (on.HasValue) q = q.Where(x => x.EffectiveFrom <= on.Value && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= on.Value));

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.Notes ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.EffectiveFrom);

            var total = q.Count();
            var page = q.ApplyPaging(opt.Page, opt.PageSize).ToList();
            var items = page.Select(s => Project(s, on)).ToList();

            return new SuccessDataResult<PagedResult<ShopFranchiseShareDetailDto>>(
                new PagedResult<ShopFranchiseShareDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<ShopFranchiseShareDetailDto> GetDetailById(Guid id)
        {
            var s = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return s == null ? new ErrorDataResult<ShopFranchiseShareDetailDto>(null, "یافت نشد.")
                             : new SuccessDataResult<ShopFranchiseShareDetailDto>(Project(s));
        }

        [CacheAspect(5)]
        public IDataResult<List<ShopFranchiseShareDetailDto>> GetActiveOn(Guid shopId, DateOnly onDate)
        {
            var list = _dal.QueryNoTracking()
                .Where(x => x.ShopId == shopId && x.EffectiveFrom <= onDate && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= onDate))
                .OrderBy(x => x.FranchiseOwnerId)
                .ToList()
                .Select(x => Project(x, onDate))
                .ToList();

            return new SuccessDataResult<List<ShopFranchiseShareDetailDto>>(list);
        }

        [CacheAspect(5)]
        public IDataResult<ShopFranchiseShareSummaryDto> GetSummary(Guid shopId, DateOnly onDate)
        {
            var shop = _shopDal.QueryNoTracking().FirstOrDefault(s => s.Id == shopId);
            var total = TotalPercentOn(shopId, onDate);
            return new SuccessDataResult<ShopFranchiseShareSummaryDto>(new ShopFranchiseShareSummaryDto
            {
                ShopId = shopId,
                ShopName = shop?.Name ?? "",
                OnDate = onDate,
                TotalPercent = total
            });
        }

        // ---------- Writes ----------
        [SecuredOperation("shopfranchiseshare.write,admin")]
        [ValidationAspect(typeof(ShopFranchiseShareValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IShopFranchiseShareService.Get")]
        public IResult Add(ShopFranchiseShare entity)
        {
            // 1) Aynı Shop+Owner için tarih çakışması olamaz
            if (HasOverlapForOwner(entity))
                return new ErrorResult("برای این مالک امتیاز در همین بازه زمانی قبلاً سهمی ثبت شده است.");

            // 2) OnDate toplam % kontrolü (başlangıç günü üzerinden kontrol)
            var checkDate = entity.EffectiveFrom;
            var totalOn = TotalPercentOn(entity.ShopId, checkDate) + entity.SharePercent;
            if (totalOn > 100m)
                return new ErrorResult("جمع درصد سهم‌ها در این تاریخ از ۱۰۰٪ بیشتر می‌شود.");

            _dal.Add(entity);
            return new SuccessResult("سهم امتیاز ثبت شد.");
        }

        [SecuredOperation("shopfranchiseshare.write,admin")]
        [ValidationAspect(typeof(ShopFranchiseShareValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IShopFranchiseShareService.Get")]
        public IResult Update(ShopFranchiseShare entity)
        {
            var current = _dal.Get(x => x.Id == entity.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            // 1) Çakışma kontrolü (aynı Shop+Owner)
            if (HasOverlapForOwner(entity, entity.Id))
                return new ErrorResult("برای این مالک امتیاز در همین بازه زمانی قبلاً سهمی ثبت شده است.");

            // 2) OnDate toplam % kontrolü (başlangıç)
            var totalOn = TotalPercentOn(entity.ShopId, entity.EffectiveFrom, entity.Id) + entity.SharePercent;
            if (totalOn > 100m)
                return new ErrorResult("جمع درصد سهم‌ها در این تاریخ از ۱۰۰٪ بیشتر می‌شود.");

            current.ShopId = entity.ShopId;
            current.FranchiseOwnerId = entity.FranchiseOwnerId;
            current.SharePercent = entity.SharePercent;
            current.EffectiveFrom = entity.EffectiveFrom;
            current.EffectiveTo = entity.EffectiveTo;
            current.Notes = entity.Notes;

            _dal.Update(current);
            return new SuccessResult("سهم بروزرسانی شد.");
        }

        [SecuredOperation("shopfranchiseshare.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IShopFranchiseShareService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
