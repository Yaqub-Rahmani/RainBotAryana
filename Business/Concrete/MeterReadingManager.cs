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
using Business.ValidationRules.FluentValidation.MeterReading;
using Entities.Dtos.MeterReadings;
using Microsoft.EntityFrameworkCore;
using Core.Utilities.KabulDateTime;
using Entities.Dtos.Accrual;

namespace Business.Concrete
{
    public class MeterReadingManager : IMeterReadingService
    {
        private readonly IMeterReadingDal _meterReadingDal;
        private readonly IContractDal _contractDal;
        private readonly ITimeProvider _timeProvider;
        private readonly IAccrualDal _accrualDal;
        private readonly IAccrualService _accrualService;
        private readonly IContractService _contractService;

        public MeterReadingManager(
            IMeterReadingDal meterReadingDal,
            IContractDal contractDal,
            ITimeProvider timeProvider,
            IAccrualDal accrualDal,
            IAccrualService accrualService,
            IContractService contractService)
        {
            _meterReadingDal = meterReadingDal;
            _contractDal = contractDal;
            _timeProvider = timeProvider;
            _accrualDal = accrualDal;
            _accrualService = accrualService;
            _contractService = contractService;
        }

        [CacheAspect(10)]
        public IDataResult<List<MeterReading>> GetAll(Guid? shopId = null, Guid? contractId = null, int? year = null, int? month = null, bool? finalized = null)
        {
            var q = _meterReadingDal.QueryNoTracking();
            if (shopId.HasValue) q = q.Where(x => x.ShopId == shopId.Value);
            if (contractId.HasValue) q = q.Where(x => x.ContractId == contractId.Value);
            if (year.HasValue) q = q.Where(x => x.Year == year.Value);
            if (month.HasValue) q = q.Where(x => x.Month == month.Value);
            if (finalized.HasValue) q = q.Where(x => x.IsFinalized == finalized.Value);
            return new SuccessDataResult<List<MeterReading>>(q.OrderByDescending(x => x.ReadingDate).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<MeterReading> GetById(Guid id)
        {
            var e = _meterReadingDal.Get(x => x.Id == id);
            return e == null ? new ErrorDataResult<MeterReading>(null, "یافت نشد.") : new SuccessDataResult<MeterReading>(e);
        }

        [CacheAspect(10)]
        public IDataResult<MeterReadingDetailDto> GetDetail(Guid id)
        {
            var e = _meterReadingDal.Get(x => x.Id == id);
            if (e == null) return new ErrorDataResult<MeterReadingDetailDto>(null, "یافت نشد.");
            var dto = new MeterReadingDetailDto
            {
                Id = e.Id,
                ShopId = e.ShopId,
                ContractId = e.ContractId,
                Year = e.Year,
                Month = e.Month,
                ReadingDate = e.ReadingDate,
                PreviousIndex = e.PreviousIndex,
                CurrentIndex = e.CurrentIndex,
                ConsumptionKwh = Math.Max(0, e.CurrentIndex - e.PreviousIndex),
                UnitPrice = e.UnitPrice,
                NetAmount = Math.Round(Math.Max(0, e.CurrentIndex - e.PreviousIndex) * e.UnitPrice, 2, MidpointRounding.AwayFromZero),
                ReserveRate = e.ReserveRate,
                ReserveAmount = Math.Round(Math.Round(Math.Max(0, e.CurrentIndex - e.PreviousIndex) * e.UnitPrice, 2, MidpointRounding.AwayFromZero) * e.ReserveRate, 2, MidpointRounding.AwayFromZero),
                TotalBilled = Math.Round(Math.Round(Math.Max(0, e.CurrentIndex - e.PreviousIndex) * e.UnitPrice, 2, MidpointRounding.AwayFromZero) * (1 + e.ReserveRate), 2, MidpointRounding.AwayFromZero),
                IsFinalized = e.IsFinalized,
                FinalizedByUserId = e.FinalizedByUserId,
                FinalizedAt = e.FinalizedAt,
                CreatedByUserId = e.CreatedByUserId,
                UpdatedByUserId = e.UpdatedByUserId,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                Notes = e.Notes,
                RowVersion = e.RowVersion
            };
            return new SuccessDataResult<MeterReadingDetailDto>(dto);
        }

        [ValidationAspect(typeof(MeterReadingCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IMeterReadingService.Get")]
        public IResult Add(MeterReadingCreateDto dto, int userId)
        {
            // Kabul zaman dilimine göre "şu an"
            var nowUtc = _timeProvider.UtcNow();
            var todayGregorian = _timeProvider.Today();          // DateOnly (Kabul, Gregorian)
            var (py, pm, _) = _timeProvider.TodayPersian();      // Örn: (1404, 8, 15)


            var e = new MeterReading
            {
                Id = dto.MeterReadingId,
                ShopId = dto.ShopId,
                ContractId = dto.ContractId,
                Year = py,                // örn. 1404
                Month = pm,               // örn. 8
                ReadingDate = todayGregorian,
                PreviousIndex = Math.Round(dto.PreviousIndex, 3, MidpointRounding.AwayFromZero),
                CurrentIndex = Math.Round(dto.CurrentIndex, 3, MidpointRounding.AwayFromZero),
                UnitPrice = Math.Round(dto.UnitPrice, 4, MidpointRounding.AwayFromZero),
                ReserveRate = dto.ReserveRate,
                Notes = dto.Notes,
                CreatedByUserId = userId,
                CreatedAt = nowUtc,
                IsFinalized = false,
            };
            _meterReadingDal.Add(e);

            if (dto.IsConfirmed)
            {
                CreateAccrualsForMeterReading(e, userId);
            }

            return new SuccessResult("ثبت شد.");
        }

        [ValidationAspect(typeof(MeterReadingUpdateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IMeterReadingService.Get")]
        public IResult Update(MeterReadingUpdateDto dto, int userId)
        {
            var e = _meterReadingDal.Get(x => x.Id == dto.Id);
            if (e == null) return new ErrorResult("یافت نشد.");
            if (e.IsFinalized) return new ErrorResult("قرائت نهایی شده است و قابل ویرایش نیست.");

            // Concurrency check
            if (!e.RowVersion.SequenceEqual(dto.RowVersion))
                return new ErrorResult("داده همزمان تغییر کرده است. لطفاً دوباره بارگذاری کنید.");

            e.ShopId = dto.ShopId;
            e.ContractId = dto.ContractId;
            e.PreviousIndex = Math.Round(dto.PreviousIndex, 3, MidpointRounding.AwayFromZero);
            e.CurrentIndex = Math.Round(dto.CurrentIndex, 3, MidpointRounding.AwayFromZero);
            e.UnitPrice = Math.Round(dto.UnitPrice, 4, MidpointRounding.AwayFromZero);
            e.ReserveRate = dto.ReserveRate;
            e.Notes = dto.Notes;
            e.UpdatedByUserId = userId;
            e.UpdatedAt = DateTimeOffset.UtcNow;

            try
            {
                _meterReadingDal.Update(e);
                return new SuccessResult("بروز شد.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ErrorResult("داده همزمان تغییر کرده است. لطفاً دوباره بارگذاری کنید.");
            }
        }

        [TransactionScopeAspect, CacheRemoveAspect("IMeterReadingService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _meterReadingDal.Query().Include(x => x.Allocations).Include(x => x.Accruals).FirstOrDefault(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            if (e.IsFinalized) return new ErrorResult("به علت نهایی شدن، حذف ممکن نیست.");
            if ((e.Allocations?.Any() ?? false) || (e.Accruals?.Any() ?? false))
                return new ErrorResult("به علت وابستگی داده‌ها حذف ممکن نیست.");
            _meterReadingDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IMeterReadingService.Get")]
        public IResult Finalize(Guid id, int userId)
        {
            var e = _meterReadingDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            if (e.IsFinalized) return new ErrorResult("قبلاً نهایی شده است.");

            e.IsFinalized = true;
            e.FinalizedByUserId = userId;
            e.FinalizedAt = DateTimeOffset.UtcNow;
            _meterReadingDal.Update(e);
            return new SuccessResult("نهایی شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IMeterReadingService.Get")]
        public IResult Unfinalize(Guid id, int userId)
        {
            var e = _meterReadingDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            // İş kuralı: settlement/accuarl varsa iptal etme
            if ((e.Accruals?.Any() ?? false) || (e.Allocations?.Any() ?? false))
                return new ErrorResult("به علت وابستگی‌ها امکان بازگشایی نیست.");

            e.IsFinalized = false;
            e.FinalizedByUserId = null;
            e.FinalizedAt = null;
            e.UpdatedByUserId = userId;
            e.UpdatedAt = DateTimeOffset.UtcNow;
            _meterReadingDal.Update(e);
            return new SuccessResult("بازگشایی شد.");
        }

        public IDataResult<MeterReadingNewContextDto> GetNewContext(Guid contractId)
        {
            var contract = _contractDal
                .QueryNoTracking()
                .SingleOrDefault(c => c.Id == contractId);

            if (contract == null)
            {
                return new ErrorDataResult<MeterReadingNewContextDto>(
                    null,
                    "Sözleşme bulunamadı.");
            }

            var lastReading = _meterReadingDal
                .QueryNoTracking()
                .Where(m => m.ContractId == contractId)
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .FirstOrDefault();

            MeterReadingNewContextDto dto;

            if (lastReading == null)
            {
                // İlk okuma senaryosu → sözleşme başlangıcını miladiden Şemsî’ye çevir
                var (startY, startM, _) = _timeProvider.ToPersian(contract.StartDate);

                dto = new MeterReadingNewContextDto
                {
                    IsFirstReadingForContract = true,

                    // Sözleşmenin başladığı Şemsî yıl/ay
                    LastYear = startY,
                    LastMonth = startM,

                    // İlk okuma öncesi "son endeks" sözleşmedeki başlangıç endeksi
                    LastCurrentIndex = contract.InitialElectricityIndex,

                    // İlk okumada önceki tüketim/tutar yok
                    LastUnitPrice = null,
                    LastNetAmount = null,
                    LastPreviouskWh = null
                };
            }
            else
            {
                // Daha önce okuma yapılmış → son okuma bilgileri
                dto = new MeterReadingNewContextDto
                {
                    IsFirstReadingForContract = false,

                    LastYear = lastReading.Year,
                    LastMonth = lastReading.Month,
                    LastCurrentIndex = lastReading.CurrentIndex,

                    LastUnitPrice = lastReading.UnitPrice,
                    LastNetAmount = lastReading.NetAmount,
                    LastPreviouskWh = lastReading.ConsumptionKwh
                };
            }

            return new SuccessDataResult<MeterReadingNewContextDto>(dto);
        }

        /// <summary>
        /// Onaylanmış sayaç kaydı için:
        /// - Market
        /// - Owner
        /// - (varsa) Franchise
        /// - Elektrik
        /// accrual'larını oluşturur.
        /// </summary>
        private IResult CreateAccrualsForMeterReading(MeterReading reading, int userId)
        {
            var contractResult = _contractService.GetById(reading.ContractId);
            if (!contractResult.Success || contractResult.Data == null)
                return new ErrorResult("Sözleşme bulunamadı.");

            var contract = contractResult.Data;
            var tenantId = contract.TenantId;

            var consumption = Math.Max(0m, reading.CurrentIndex - reading.PreviousIndex);
            var electricityAmount = Math.Round(consumption * reading.UnitPrice, 2, MidpointRounding.AwayFromZero);

            var accruals = new List<AccrualCreateDto>
    {
        new AccrualCreateDto
        {
            ShopId = reading.ShopId,
            TenantId = tenantId,
            ContractId = reading.ContractId,
            MeterReadingId = null,
            Kind = AccrualKind.Market,
            Year = reading.Year,
            Month = reading.Month,
            Amount = contract.MarketPortionAmount,
            Notes = $"دوره تائیدی {reading.Year}/{reading.Month} - سهم مارکیت"
        },
        new AccrualCreateDto
        {
            ShopId = reading.ShopId,
            TenantId = tenantId,
            ContractId = reading.ContractId,
            MeterReadingId = null,
            Kind = AccrualKind.Owner,
            Year = reading.Year,
            Month = reading.Month,
            Amount = contract.OwnerPortionAmount,
            Notes = $"دوره تائیدی {reading.Year}/{reading.Month} - صاحب مارکیت"
        },
        new AccrualCreateDto
        {
            ShopId = reading.ShopId,
            TenantId = tenantId,
            ContractId = reading.ContractId,
            MeterReadingId = reading.Id,
            Kind = AccrualKind.Electricity,
            Year = reading.Year,
            Month = reading.Month,
            Amount = electricityAmount,
            Notes = $"دوره تائیدی {reading.Year}/{reading.Month} - صرفیه برق"
        }
    };

            if (contract.FranchisePortionAmount > 0)
            {
                accruals.Add(new AccrualCreateDto
                {
                    ShopId = reading.ShopId,
                    TenantId = tenantId,
                    ContractId = reading.ContractId,
                    MeterReadingId = null,
                    Kind = AccrualKind.Franchise,
                    Year = reading.Year,
                    Month = reading.Month,
                    Amount = contract.FranchisePortionAmount,
                    Notes = $"Kabul dönemi {reading.Year}/{reading.Month} - Franchise"
                });
            }

            foreach (var acc in accruals)
            {
                var result = _accrualService.Add(acc, userId);
                if (!result.Success)
                    return result;
            }

            return new SuccessResult("Dönem borçları oluşturuldu.");
        }

    }
}
