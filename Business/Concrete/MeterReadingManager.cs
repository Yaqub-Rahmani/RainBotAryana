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
    public class MeterReadingManager : IMeterReadingService
    {
        private readonly IMeterReadingDal _readingDal;
        private readonly IShopDal _shopDal;
        private readonly IAccrualDal _accrualDal;
        private readonly IContractDal _contractDal;

        public MeterReadingManager(
            IMeterReadingDal readingDal,
            IShopDal shopDal,
            IAccrualDal accrualDal,
            IContractDal contractDal)
        {
            _readingDal = readingDal;
            _shopDal = shopDal;
            _accrualDal = accrualDal;
            _contractDal = contractDal;
        }

        // ---------- Helpers ----------
        private static DateOnly FirstOfMonth(int year, int month) => new DateOnly(year, month, 1);
        private static DateOnly LastOfMonth(int year, int month) => FirstOfMonth(year, month).AddMonths(1).AddDays(-1);

        private Contract? FindCoveringContract(Guid shopId, int year, int month)
        {
            var start = FirstOfMonth(year, month);
            var end = LastOfMonth(year, month);
            return _contractDal.QueryNoTracking()
                .Where(c => c.ShopId == shopId && c.Status == ContractStatus.Active
                            && c.StartDate <= end && c.EndDate >= start)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefault();
        }

        private Accrual? FindExistingAccrual(Guid shopId, Guid tenantId, Guid contractId, int year, int month)
        {
            return _accrualDal.Query().FirstOrDefault(a =>
                a.ShopId == shopId &&
                a.TenantId == tenantId &&
                a.ContractId == contractId &&
                a.Year == year && a.Month == month &&
                a.Kind == AccrualKind.Electricity);
        }

        private void EnsureUniquePeriod(MeterReading r, Guid? excludeId = null)
        {
            var q = _readingDal.QueryNoTracking().Where(x => x.ShopId == r.ShopId && x.Year == r.Year && x.Month == r.Month);
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
            if (q.Any())
                throw new InvalidOperationException("برای این فروشگاه در این دوره قبلاً قرائت ثبت شده است.");
        }

        private MeterReadingDetailDto ProjectDetail(MeterReading r)
        {
            var shop = _shopDal.QueryNoTracking().FirstOrDefault(s => s.Id == r.ShopId);
            var accr = _accrualDal.QueryNoTracking()
                        .FirstOrDefault(a => a.MeterReadingId == r.Id && a.Kind == AccrualKind.Electricity);

            return new MeterReadingDetailDto
            {
                Id = r.Id,
                ShopId = r.ShopId,
                ShopName = shop?.Name ?? shop?.Name ?? "", // Shop.Name varsayımı; yoksa düzenle
                Year = r.Year,
                Month = r.Month,
                ReadingDate = r.ReadingDate,
                PreviousIndex = r.PreviousIndex,
                CurrentIndex = r.CurrentIndex,
                ConsumptionKwh = Math.Max(0, r.CurrentIndex - r.PreviousIndex),
                UnitPrice = r.UnitPrice,
                ReserveRate = r.ReserveRate,
                NetAmount = Math.Round(Math.Max(0, r.CurrentIndex - r.PreviousIndex) * r.UnitPrice, 2, MidpointRounding.AwayFromZero),
                ReserveAmount = Math.Round(Math.Round(Math.Max(0, r.CurrentIndex - r.PreviousIndex) * r.UnitPrice, 2, MidpointRounding.AwayFromZero) * r.ReserveRate, 2, MidpointRounding.AwayFromZero),
                TotalBilled = Math.Round(Math.Max(0, r.CurrentIndex - r.PreviousIndex) * r.UnitPrice, 2, MidpointRounding.AwayFromZero)
                              + Math.Round(Math.Round(Math.Max(0, r.CurrentIndex - r.PreviousIndex) * r.UnitPrice, 2, MidpointRounding.AwayFromZero) * r.ReserveRate, 2, MidpointRounding.AwayFromZero),
                IsFinalized = r.IsFinalized,
                Notes = r.Notes,
                AccrualId = accr?.Id,
                AccrualAmount = accr?.Amount,
                AccrualOutstanding = accr?.OutstandingAmount,
                AccrualStatus = accr?.Status.ToString()
            };
        }

        // Elektrik tahakkuku oluştur/güncelle
        private void UpsertElectricityAccrual(MeterReading r)
        {
            var contract = FindCoveringContract(r.ShopId, r.Year, r.Month);
            if (contract == null)
                throw new InvalidOperationException("این دوره برای فروشگاه قرارداد فعالی یافت نشد.");

            var amount = Math.Round((Math.Max(0, r.CurrentIndex - r.PreviousIndex) * r.UnitPrice), 2, MidpointRounding.AwayFromZero);
            var reserve = Math.Round(amount * r.ReserveRate, 2, MidpointRounding.AwayFromZero);
            var total = amount + reserve;

            var existing = _accrualDal.Query().FirstOrDefault(a => a.MeterReadingId == r.Id);
            if (existing == null)
            {
                var acc = new Accrual
                {
                    Id = Guid.NewGuid(),
                    ShopId = r.ShopId,
                    TenantId = contract.TenantId,
                    ContractId = contract.Id,
                    MeterReadingId = r.Id,
                    Kind = AccrualKind.Electricity,
                    Year = r.Year,
                    Month = r.Month,
                    CreatedAtUtc = DateTime.UtcNow,
                    Status = AccrualStatus.Open,
                    Amount = total
                };
                acc.InitializeOutstanding();
                _accrualDal.Add(acc);
            }
            else
            {
                // Eğer kısmen/tam kapalıysa, outstanding’i delta kadar ayarla
                var delta = total - existing.Amount;
                existing.Amount = total;
                existing.OutstandingAmount = Math.Max(0, Math.Round(existing.OutstandingAmount + delta, 2, MidpointRounding.AwayFromZero));
                existing.Status = existing.OutstandingAmount == 0 ? AccrualStatus.Settled :
                                  (existing.OutstandingAmount == existing.Amount ? AccrualStatus.Open : AccrualStatus.PartiallySettled);
                _accrualDal.Update(existing);
            }
        }

        private void TryRemoveAccrualIfNoSettlements(MeterReading r)
        {
            var existing = _accrualDal.Query().FirstOrDefault(a => a.MeterReadingId == r.Id);
            if (existing == null) return;

            var hasSettlement = existing.OutstandingAmount != existing.Amount;
            if (hasSettlement)
                throw new InvalidOperationException("برای این قرائت تسویه‌ای انجام شده است؛ حذف/بازگشت ممکن نیست.");

            _accrualDal.Delete(existing);
        }

        // ---------- Okumalar ----------
        [CacheAspect(10)]
        public IDataResult<List<MeterReading>> GetAll(Guid? shopId = null, int? year = null, int? month = null, bool? finalized = null)
        {
            var q = _readingDal.QueryNoTracking();
            if (shopId.HasValue && shopId.Value != Guid.Empty) q = q.Where(x => x.ShopId == shopId.Value);
            if (year.HasValue) q = q.Where(x => x.Year == year.Value);
            if (month.HasValue) q = q.Where(x => x.Month == month.Value);
            if (finalized.HasValue) q = q.Where(x => x.IsFinalized == finalized.Value);

            var list = q.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ThenByDescending(x => x.ReadingDate).ToList();
            return new SuccessDataResult<List<MeterReading>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<MeterReading> GetById(Guid id)
        {
            var e = _readingDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<MeterReading>(null, "یافت نشد.") : new SuccessDataResult<MeterReading>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<MeterReadingDetailDto>> GetDetails(QueryOptions opt, Guid? shopId = null, int? year = null, int? month = null, bool? finalized = null)
        {
            opt ??= new QueryOptions();
            var q = _readingDal.QueryNoTracking();

            if (shopId.HasValue && shopId.Value != Guid.Empty) q = q.Where(x => x.ShopId == shopId.Value);
            if (year.HasValue) q = q.Where(x => x.Year == year.Value);
            if (month.HasValue) q = q.Where(x => x.Month == month.Value);
            if (finalized.HasValue) q = q.Where(x => x.IsFinalized == finalized.Value);

            q = q.ApplyFilters(opt.Filters);
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.Notes ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month);

            var items = q.Select(r => ProjectDetail(r));

            var total = items.Count();
            var pageItems = items.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<MeterReadingDetailDto>>(
                new PagedResult<MeterReadingDetailDto>
                {
                    Items = pageItems,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<MeterReadingDetailDto> GetDetailById(Guid id)
        {
            var r = _readingDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (r == null) return new ErrorDataResult<MeterReadingDetailDto>(null, "یافت نشد.");
            return new SuccessDataResult<MeterReadingDetailDto>(ProjectDetail(r));
        }

        // ---------- Yazmalar ----------
        [SecuredOperation("meterreading.write,admin")]
        [ValidationAspect(typeof(MeterReadingValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMeterReadingService.Get")]
        public IResult Add(MeterReading reading)
        {
            if (_shopDal.Get(x => x.Id == reading.ShopId) == null)
                return new ErrorResult("فروشگاه یافت نشد.");

            EnsureUniquePeriod(reading);
            _readingDal.Add(reading);
            return new SuccessResult("قرائت ثبت شد.");
        }

        [SecuredOperation("meterreading.write,admin")]
        [ValidationAspect(typeof(MeterReadingValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMeterReadingService.Get")]
        public IResult Update(MeterReading reading)
        {
            var current = _readingDal.Get(x => x.Id == reading.Id);
            if (current == null) return new ErrorResult("یافت نشد.");
            if (current.IsFinalized) return new ErrorResult("این قرائت نهایی شده است و قابل ویرایش نیست.");

            EnsureUniquePeriod(reading, reading.Id);

            current.ShopId = reading.ShopId;
            current.Year = reading.Year;
            current.Month = reading.Month;
            current.ReadingDate = reading.ReadingDate;
            current.PreviousIndex = reading.PreviousIndex;
            current.CurrentIndex = reading.CurrentIndex;
            current.UnitPrice = reading.UnitPrice;
            current.ReserveRate = reading.ReserveRate;
            current.Notes = reading.Notes;

            _readingDal.Update(current);
            return new SuccessResult("قرائت بروزرسانی شد.");
        }

        [SecuredOperation("meterreading.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMeterReadingService.Get")]
        public IResult Delete(Guid id)
        {
            var r = _readingDal.Get(x => x.Id == id);
            if (r == null) return new ErrorResult("یافت نشد.");
            if (r.IsFinalized) return new ErrorResult("قرائت نهایی شده قابل حذف نیست.");

            // Eğer bu reading’e bağlı Accrual varsa ve settlement yoksa kaldır
            TryRemoveAccrualIfNoSettlements(r);

            _readingDal.Delete(r);
            return new SuccessResult("حذف شد.");
        }

        // ---------- İşlemsel ----------
        [SecuredOperation("meterreading.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMeterReadingService.Get")]
        public IResult Finalize(Guid id)
        {
            var r = _readingDal.Get(x => x.Id == id);
            if (r == null) return new ErrorResult("یافت نشد.");
            if (r.IsFinalized) return new SuccessResult("پیش‌تر نهایی شده است.");

            // Accrual üret/güncelle
            UpsertElectricityAccrual(r);

            r.IsFinalized = true;
            _readingDal.Update(r);
            return new SuccessResult("قرائت نهایی شد و بدهی برق ثبت گردید.");
        }

        [SecuredOperation("meterreading.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMeterReadingService.Get")]
        public IResult Unfinalize(Guid id)
        {
            var r = _readingDal.Get(x => x.Id == id);
            if (r == null) return new ErrorResult("یافت نشد.");
            if (!r.IsFinalized) return new SuccessResult("قرائت پیش‌تر باز بوده است.");

            // Accrual varsa ve settlement yoksa kaldır; varsa settlement -> engelle
            TryRemoveAccrualIfNoSettlements(r);

            r.IsFinalized = false;
            _readingDal.Update(r);
            return new SuccessResult("وضعیت قرائت به حالت بازگشت.");
        }

        [SecuredOperation("meterreading.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMeterReadingService.Get")]
        public IResult SyncAccrual(Guid id)
        {
            var r = _readingDal.Get(x => x.Id == id);
            if (r == null) return new ErrorResult("یافت نشد.");

            UpsertElectricityAccrual(r);
            return new SuccessResult("همگام‌سازی بدهی برق انجام شد.");
        }
    }
}
