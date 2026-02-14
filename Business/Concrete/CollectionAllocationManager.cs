using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Business.ValidationRules.FluentValidation.CollectionAllocation;
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
using Entities.Dtos.CollectionAllocation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CollectionAllocationManager : ICollectionAllocationService
    {
        private readonly ICollectionAllocationDal _allocationDal;
        private readonly ICollectionDal _collectionDal;
        private readonly IDollarRateDal _dollarRateDal;
        private readonly ITimeProvider _timeProvider;

        public CollectionAllocationManager(
            ICollectionAllocationDal allocationDal,
            ICollectionDal collectionDal,
            IDollarRateDal dollarRateDal,
            ITimeProvider timeProvider)
        {
            _allocationDal = allocationDal;
            _collectionDal = collectionDal;
            _dollarRateDal = dollarRateDal;
            _timeProvider = timeProvider;
        }

        [CacheAspect(10)]
        public IDataResult<CollectionAllocationDto> GetById(Guid id)
        {
            var alloc = _allocationDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (alloc == null)
                return new ErrorDataResult<CollectionAllocationDto>(null,"سطر توزیع یافت نشد.");

            var collection = _collectionDal.QueryNoTracking()
                .FirstOrDefault(c => c.Id == alloc.CollectionId);

            if (collection == null)
                return new ErrorDataResult<CollectionAllocationDto>(null,"پرداخت مرتبط یافت نشد.");

            var rate = GetRateForDate(collection.PaidAtUtc);
            if (rate == null)
                return new ErrorDataResult<CollectionAllocationDto>(null,"نرخ دالر برای آن تاریخ یافت نشد.");

            var dto = MapToDto(alloc, rate.Value);

            return new SuccessDataResult<CollectionAllocationDto>(dto);
        }

        [CacheAspect(10)]
        public IDataResult<List<CollectionAllocationDto>> GetByCollection(Guid collectionId)
        {
            var collection = _collectionDal.QueryNoTracking()
                .FirstOrDefault(c => c.Id == collectionId);

            if (collection == null)
                return new ErrorDataResult<List<CollectionAllocationDto>>(null,"پرداخت یافت نشد.");

            var rate = GetRateForDate(collection.PaidAtUtc);
            if (rate == null)
                return new ErrorDataResult<List<CollectionAllocationDto>>(null,"نرخ دالر برای آن تاریخ یافت نشد.");

            var allocs = _allocationDal.QueryNoTracking()
                .Where(x => x.CollectionId == collectionId)
                .OrderBy(x => x.CreatedAtUtc)
                .ThenBy(x => x.Id)
                .ToList();

            var list = allocs
                .Select(a => MapToDto(a, rate.Value))
                .ToList();

            return new SuccessDataResult<List<CollectionAllocationDto>>(list);
        }

        [ValidationAspect(typeof(CollectionAllocationCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("ICollectionAllocationService.Get")]
        [SecuredOperation("admin,collectionallocation.add")]
        public IResult Add(CollectionAllocationCreateDto dto, int userId)
        {
            var collection = _collectionDal.QueryNoTracking()
                .FirstOrDefault(c => c.Id == dto.CollectionId);

            if (collection == null)
                return new ErrorResult("پرداخت مرتبط یافت نشد.");

            var entity = new CollectionAllocation
            {
                Id = Guid.NewGuid(),
                CollectionId = dto.CollectionId,
                Kind = dto.Kind,
                Amount = dto.Amount,
                ContractId = dto.ContractId ?? collection.ContractId,
                ShopId = dto.ShopId ?? collection.ShopId,
                TenantId = dto.TenantId ?? collection.TenantId,
                MeterReadingId = dto.MeterReadingId,
                PeriodYear = dto.PeriodYear,
                PeriodMonth = dto.PeriodMonth,
                Notes = dto.Notes,
                CreatedByUserId = userId,
                CreatedAtUtc = _timeProvider.UtcNow()
            };

            _allocationDal.Add(entity);

            return new SuccessResult("سطر توزیع ثبت شد.");
        }

        private decimal? GetRateForDate(DateTime dateUtc)
        {
            var rate = _dollarRateDal.QueryNoTracking()
                .Where(r => r.EffectiveFromUtc <= dateUtc)
                .OrderByDescending(r => r.EffectiveFromUtc)
                .ThenByDescending(r => r.Id)
                .FirstOrDefault();

            return rate?.AfnPerUsd;
        }

        private static CollectionAllocationDto MapToDto(CollectionAllocation a, decimal afnPerUsd)
        {
            if (afnPerUsd <= 0) afnPerUsd = 1; // güvenlik

            var usd = Math.Round(a.Amount / afnPerUsd, 4);

            return new CollectionAllocationDto
            {
                Id = a.Id,
                CollectionId = a.CollectionId,
                Kind = a.Kind,
                Amount = a.Amount,
                AfnPerUsdAtThatTime = afnPerUsd,
                UsdEquivalent = usd,
                ContractId = a.ContractId,
                ShopId = a.ShopId,
                TenantId = a.TenantId,
                MeterReadingId = a.MeterReadingId,
                PeriodYear = a.PeriodYear,
                PeriodMonth = a.PeriodMonth,
                Notes = a.Notes,
                CreatedByUserId = a.CreatedByUserId,
                CreatedAtUtc = a.CreatedAtUtc
            };
        }
    }
}
