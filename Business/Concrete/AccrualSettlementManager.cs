using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Business.ValidationRules.FluentValidation.AccrualSettlement;
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
using Entities.Dtos.AccrualSettlement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class AccrualSettlementManager : IAccrualSettlementService
    {
        private readonly IAccrualSettlementDal _settlementDal;
        private readonly ICollectionAllocationDal _allocationDal;
        private readonly ICollectionDal _collectionDal;
        private readonly IDollarRateDal _dollarRateDal;
        private readonly ITimeProvider _timeProvider;

        public AccrualSettlementManager(
            IAccrualSettlementDal settlementDal,
            ICollectionAllocationDal allocationDal,
            ICollectionDal collectionDal,
            IDollarRateDal dollarRateDal,
            ITimeProvider timeProvider)
        {
            _settlementDal = settlementDal;
            _allocationDal = allocationDal;
            _collectionDal = collectionDal;
            _dollarRateDal = dollarRateDal;
            _timeProvider = timeProvider;
        }

        [CacheAspect(10)]
        public IDataResult<AccrualSettlementDto> GetById(Guid id)
        {
            var s = _settlementDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (s == null)
                return new ErrorDataResult<AccrualSettlementDto>(null, "تسویه یافت نشد.");

            var rate = GetRateForSettlement(s);
            if (rate == null)
                return new ErrorDataResult<AccrualSettlementDto>(null, "نرخ دالر برای آن تاریخ یافت نشد.");

            var dto = MapToDto(s, rate.Value);
            return new SuccessDataResult<AccrualSettlementDto>(dto);
        }

        [CacheAspect(10)]
        public IDataResult<List<AccrualSettlementDto>> GetByAccrual(Guid accrualId)
        {
            var list = _settlementDal.QueryNoTracking()
                .Where(x => x.AccrualId == accrualId)
                .OrderBy(x => x.SettledAtUtc)
                .ThenBy(x => x.Id)
                .ToList();

            var result = new List<AccrualSettlementDto>();

            foreach (var s in list)
            {
                var rate = GetRateForSettlement(s);
                if (rate == null) continue;
                result.Add(MapToDto(s, rate.Value));
            }

            return new SuccessDataResult<List<AccrualSettlementDto>>(result);
        }

        [CacheAspect(10)]
        public IDataResult<List<AccrualSettlementDto>> GetByAllocation(Guid collectionAllocationId)
        {
            var list = _settlementDal.QueryNoTracking()
                .Where(x => x.CollectionAllocationId == collectionAllocationId)
                .OrderBy(x => x.SettledAtUtc)
                .ThenBy(x => x.Id)
                .ToList();

            var result = new List<AccrualSettlementDto>();

            foreach (var s in list)
            {
                var rate = GetRateForSettlement(s);
                if (rate == null) continue;
                result.Add(MapToDto(s, rate.Value));
            }

            return new SuccessDataResult<List<AccrualSettlementDto>>(result);
        }

        [ValidationAspect(typeof(AccrualSettlementCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IAccrualSettlementService.Get")]
        [SecuredOperation("admin,accrual.settlement")]
        public IResult Add(AccrualSettlementCreateDto dto, int userId)
        {
            var alloc = _allocationDal.QueryNoTracking()
                .FirstOrDefault(x => x.Id == dto.CollectionAllocationId);

            if (alloc == null)
                return new ErrorResult("سطر توزیع مرتبط یافت نشد.");

            if (dto.Amount <= 0)
                return new ErrorResult("مبلغ باید بزرگتر از صفر باشد.");

            // Burada istersen Accrual'ın kalan borcunu kontrol edip
            // dto.Amount > Remaining ise hata dönebilirsin.

            var nowUtc = _timeProvider.UtcNow();

            var entity = new AccrualSettlement
            {
                Id = Guid.NewGuid(),
                CollectionAllocationId = dto.CollectionAllocationId,
                AccrualId = dto.AccrualId,
                Amount = dto.Amount,
                SettledByUserId = userId,
                SettledAtUtc = nowUtc,
                Notes = dto.Notes
            };

            _settlementDal.Add(entity);

            return new SuccessResult("تسویه ثبت شد.");
        }

        /// <summary>
        /// Kur tarihi olarak tahsilatın yapıldığı günü (Collection.PaidAtUtc) kullanıyoruz.
        /// </summary>
        private decimal? GetRateForSettlement(AccrualSettlement s)
        {
            var alloc = _allocationDal.QueryNoTracking()
                .FirstOrDefault(a => a.Id == s.CollectionAllocationId);

            if (alloc == null)
                return null;

            var collection = _collectionDal.QueryNoTracking()
                .FirstOrDefault(c => c.Id == alloc.CollectionId);

            if (collection == null)
                return null;

            var paidAt = collection.PaidAtUtc;

            var rate = _dollarRateDal.QueryNoTracking()
                .Where(r => r.EffectiveFromUtc <= paidAt)
                .OrderByDescending(r => r.EffectiveFromUtc)
                .ThenByDescending(r => r.Id)
                .FirstOrDefault();

            return rate?.AfnPerUsd;
        }

        private static AccrualSettlementDto MapToDto(AccrualSettlement s, decimal afnPerUsd)
        {
            if (afnPerUsd <= 0) afnPerUsd = 1;
            var usd = Math.Round(s.Amount / afnPerUsd, 4);

            return new AccrualSettlementDto
            {
                Id = s.Id,
                CollectionAllocationId = s.CollectionAllocationId,
                AccrualId = s.AccrualId,
                Amount = s.Amount,
                AfnPerUsdAtThatTime = afnPerUsd,
                UsdEquivalent = usd,
                SettledByUserId = s.SettledByUserId,
                SettledAtUtc = s.SettledAtUtc,
                Notes = s.Notes
            };
        }
    }
}
