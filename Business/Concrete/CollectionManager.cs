using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Business.ValidationRules.FluentValidation.Collection;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Validation;
using Core.Extensions;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.Collection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CollectionManager : ICollectionService
    {
        private readonly ICollectionDal _dal;
        private readonly ICollectionAllocationDal _allocDal; // varsayılan
        private readonly ITimeProvider _timeProvider;

        public CollectionManager(ICollectionDal dal, ICollectionAllocationDal allocDal, ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            _dal = dal;
            _allocDal = allocDal;
        }

        [CacheAspect(10)]
        public IDataResult<List<Collection>> GetAll(Guid? tenantId = null, Guid? shopId = null, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            var q = _dal.QueryNoTracking();

            if (tenantId.HasValue) q = q.Where(x => x.TenantId == tenantId.Value);
            if (shopId.HasValue) q = q.Where(x => x.ShopId == shopId.Value);
            if (fromUtc.HasValue) q = q.Where(x => x.PaidAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) q = q.Where(x => x.PaidAtUtc < toUtc.Value);

            return new SuccessDataResult<List<Collection>>(q.OrderByDescending(x => x.PaidAtUtc).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<Collection> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking()
                        .Include(x => x.Allocations)
                        .FirstOrDefault(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<Collection>(null, "یافت نشد.")
                : new SuccessDataResult<Collection>(e);
        }

        [CacheAspect(10)]
        public IDataResult<CollectionDetailDto> GetDetail(Guid id)
        {
            var e = _dal.QueryNoTracking()
                        .Include(x => x.Allocations)
                        .FirstOrDefault(x => x.Id == id);
            if (e == null) return new ErrorDataResult<CollectionDetailDto>(null, "یافت نشد.");

            var allocated = e.Allocations?.Sum(a => a.Amount) ?? 0m;
            var unallocated = Math.Round(Math.Max(0m, e.AmountReceived - allocated), 2, MidpointRounding.AwayFromZero);

            return new SuccessDataResult<CollectionDetailDto>(new CollectionDetailDto
            {
                Id = e.Id,
                TenantId = e.TenantId,
                ShopId = e.ShopId,
                ContractId = e.ContractId,
                PaidAtUtc = e.PaidAtUtc,
                CreatedByUserId = e.CreatedByUserId,
                UpdatedByUserId = e.UpdatedByUserId,
                UpdatedAtUtc = e.UpdatedAtUtc,
                AmountReceived = e.AmountReceived,
                Method = e.Method,
                ReferenceNo = e.ReferenceNo,
                TargetYear = e.TargetYear,
                TargetMonth = e.TargetMonth,
                Notes = e.Notes,
                AllocationCount = e.Allocations?.Count ?? 0,
                AllocatedTotal = allocated,
                UnallocatedAmount = unallocated
            });
        }

        [ValidationAspect(typeof(CollectionCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("ICollectionService.Get")]
        public IResult Add(CollectionCreateDto dto, int userId)
        {
            var nowUtc = _timeProvider.UtcNow();
            var todayGregorian = _timeProvider.Today();          // DateOnly (Kabul, Gregorian)
            var (py, pm, _) = _timeProvider.TodayPersian();      // Örn: (1404, 8, 15)

            var entity = new Collection
            {
                Id = Guid.NewGuid(),
                TenantId = dto.TenantId,
                ShopId = dto.ShopId,
                ContractId = dto.ContractId,
                PaidAtUtc = nowUtc,
                CreatedByUserId = userId,
                AmountReceived = Math.Round(dto.AmountReceived, 2, MidpointRounding.AwayFromZero),
                Method = dto.Method,
                ReferenceNo = dto.ReferenceNo,
                TargetYear = py,
                TargetMonth = pm,
                Notes = dto.Notes
            };

            _dal.Add(entity);

            var amt = entity.AmountReceived.ToString(CultureInfo.InvariantCulture);
            return new SuccessResult($"دریافت با مبلغ {amt} ثبت شد.");
        }


        [ValidationAspect(typeof(CollectionUpdateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("ICollectionService.Get")]
        public IResult Update(CollectionUpdateDto dto, int userId)
        {
            var e = _dal.Query().Include(x => x.Allocations).FirstOrDefault(x => x.Id == dto.Id);
            if (e == null) return new ErrorResult("یافت نشد.");

            // Dağıtım toplamını aşma
            var allocated = e.Allocations?.Sum(a => a.Amount) ?? 0m;
            var newAmount = Math.Round(dto.AmountReceived, 2, MidpointRounding.AwayFromZero);
            if (newAmount < allocated)
            {
                var minStr = allocated.ToString(CultureInfo.InvariantCulture);
                return new ErrorResult($"مبلغ دریافتی نمی‌تواند از مجموع تخصیص‌ها ({minStr}) کمتر باشد.");
            }

            e.TenantId = dto.TenantId;
            e.ShopId = dto.ShopId;
            e.ContractId = dto.ContractId;
            e.PaidAtUtc = dto.PaidAtUtc;
            e.AmountReceived = newAmount;
            e.Method = dto.Method;
            e.ReferenceNo = dto.ReferenceNo;
            e.TargetYear = dto.TargetYear;
            e.TargetMonth = dto.TargetMonth;
            e.Notes = dto.Notes;
            e.UpdatedByUserId = userId;
            e.UpdatedAtUtc = DateTime.UtcNow;

            _dal.Update(e);
            return new SuccessResult("بروز شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("ICollectionService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Query().Include(x => x.Allocations).FirstOrDefault(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            if ((e.Allocations?.Any() ?? false))
                return new ErrorResult("به علت وجود تخصیص‌ها حذف ممکن نیست.");

            try
            {
                _dal.Delete(e);
                return new SuccessResult("حذف شد.");
            }
            catch (DbUpdateException)
            {
                return new ErrorResult("حذف به علت وابستگی داده‌ها ممکن نیست.");
            }
        }
    }
}
