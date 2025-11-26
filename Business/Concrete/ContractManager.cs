using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Business.ValidationRules.FluentValidation.Accrual;
using Business.ValidationRules.FluentValidation.Contract;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Validation;
using Core.Extensions;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.Accrual;
using Entities.Dtos.contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class ContractManager : IContractService
    {
        private readonly IContractDal _contractDal;
        private readonly IShopDal _shopDal;
        private readonly ITenantDal _tenantDal;
        private readonly IMarketDal _marketDal;
        private readonly IFloorDal _floorDal;
        private readonly IAccrualDal _accrualDal;
        private readonly ITimeProvider _timeProvider;
        private IMeterReadingDal _meterReadingDal;
        private readonly IShopFranchiseShareDal _shopFranchiseShareDal;
        private readonly IFranchiseOwnerDal _franchiseOwnerDal;

        public ContractManager(
            IContractDal contractDal,
            IShopDal shopDal,
            ITenantDal tenantDal,
            IMarketDal marketDal,
            IFloorDal floorDal,
            IAccrualDal accrualDal,
            ITimeProvider timeProvider,
            IShopFranchiseShareDal shopFranchiseShareDal,
            IFranchiseOwnerDal franchiseOwnerDal,
            IMeterReadingDal meterReadingDal
            )
        {
            _contractDal = contractDal;
            _shopDal = shopDal;
            _tenantDal = tenantDal;
            _marketDal = marketDal;
            _floorDal = floorDal;
            _accrualDal = accrualDal;
            _shopFranchiseShareDal = shopFranchiseShareDal;
            _franchiseOwnerDal = franchiseOwnerDal;
            _timeProvider = timeProvider;
        }

        [CacheAspect(10)]
        public IDataResult<List<Contract>> GetAll(bool includeArchived = false)
        {
            var q = _contractDal.QueryNoTracking();
            if (!includeArchived) q = q.Where(x => x.Status == ContractStatus.Active);
            return new SuccessDataResult<List<Contract>>(q.OrderByDescending(x => x.CreatedAtUtc).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<Contract> GetById(Guid id)
        {
            var e = _contractDal.QueryNoTracking()
                        .Include(x => x.Accruals)
                        .Include(x => x.Collections)
                        .FirstOrDefault(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<Contract>(null, "یافت نشد.")
                : new SuccessDataResult<Contract>(e);
        }

        [CacheAspect(10)]
        public IDataResult<ContractDetailDto> GetDetail(Guid id)
        {
            var e = _contractDal.QueryNoTracking()
                        .Include(x => x.Accruals)
                        .Include(x => x.Collections)
                        .FirstOrDefault(x => x.Id == id);
            if (e == null) return new ErrorDataResult<ContractDetailDto>(null, "یافت نشد.");

            var dto = new ContractDetailDto
            {
                Id = e.Id,
                ShopId = e.ShopId,
                TenantId = e.TenantId,
                SignedOn = e.SignedOn,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                DueDayOfMonth = e.DueDayOfMonth,
                OwnerPortionAmount = e.OwnerPortionAmount,
                FranchisePortionAmount = e.FranchisePortionAmount,
                MarketPortionAmount = e.MarketPortionAmount,
                TotalFixedRent = e.TotalFixedRent,
                ElectricityNote = e.ElectricityNote,
                BusinessActivity = e.BusinessActivity,
                Status = e.Status,
                Version = e.Version,
                ArchiveReason = e.ArchiveReason,
                AccrualCount = e.Accruals?.Count ?? 0,
                CollectionCount = e.Collections?.Count ?? 0
            };
            return new SuccessDataResult<ContractDetailDto>(dto);
        }

        [ValidationAspect(typeof(ContractCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IContractService.Get")]
        public IResult Add(ContractCreateDto dto, int userId)
        {
            var now = _timeProvider.UtcNow();

            var entity = new Contract
            {
                Id = dto.ContractId,
                ShopId = dto.ShopId,
                TenantId = dto.TenantId,
                SignedOn = dto.SignedOn,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DueDayOfMonth = dto.DueDayOfMonth,
                OwnerPortionAmount = dto.OwnerPortionAmount,
                FranchisePortionAmount = dto.FranchisePortionAmount,
                MarketPortionAmount = dto.MarketPortionAmount,
                ElectricityNote = dto.ElectricityNote,
                BusinessActivity = dto.BusinessActivity,
                Status = ContractStatus.Active,
                Version = dto.Version,
                CreatedByUserId = userId,
                CreatedAtUtc = now,
                Note = dto.Note,
                InitialElectricityIndex = dto.InitialElectricityIndex,
            };

            _contractDal.Add(entity);
            return new SuccessResult("ثبت شد.");
        }

        [ValidationAspect(typeof(ContractUpdateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IContractService.Get")]
        public IResult Update(ContractUpdateDto dto, int userId)
        {
            var e = _contractDal.Get(x => x.Id == dto.Id);
            if (e == null) return new ErrorResult("یافت نشد.");

            e.ShopId = dto.ShopId;
            e.TenantId = dto.TenantId;
            e.SignedOn = dto.SignedOn;
            e.StartDate = dto.StartDate;
            e.EndDate = dto.EndDate;
            e.DueDayOfMonth = dto.DueDayOfMonth;
            e.OwnerPortionAmount = dto.OwnerPortionAmount;
            e.FranchisePortionAmount = dto.FranchisePortionAmount;
            e.MarketPortionAmount = dto.MarketPortionAmount;
            e.ElectricityNote = dto.ElectricityNote;
            e.BusinessActivity = dto.BusinessActivity;
            e.Status = dto.Status;
            e.Version = dto.Version;
            e.ArchiveReason = dto.ArchiveReason;
            e.UpdatedByUserId = userId;
            e.UpdatedAtUtc = DateTime.UtcNow;

            // İş kuralı: arşivli sözleşme aktif ediliyorsa arşiv alanlarını temizle
            if (e.Status == ContractStatus.Active)
            {
                e.ArchivedAtUtc = null;
                e.ArchivedByUserId = null;
                e.ArchiveReason = null;
            }

            _contractDal.Update(e);
            return new SuccessResult("بروز شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IContractService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _contractDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            try
            {
                _contractDal.Delete(e);
                return new SuccessResult("حذف شد.");
            }
            catch (DbUpdateException)
            {
                return new ErrorResult("به علت وابستگی داده‌ها حذف ممکن نیست.");
            }
        }

        [TransactionScopeAspect, CacheRemoveAspect("IContractService.Get")]
        public IResult Archive(Guid id, int userId, string? reason)
        {
            var e = _contractDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.Status = ContractStatus.Archived;
            e.ArchivedByUserId = userId;
            e.ArchivedAtUtc = DateTime.UtcNow;
            e.ArchiveReason = reason;
            _contractDal.Update(e);
            return new SuccessResult("قرارداد بایگانی شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IContractService.Get")]
        public IResult Unarchive(Guid id, int userId)
        {
            var e = _contractDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.Status = ContractStatus.Active;
            e.ArchivedByUserId = null;
            e.ArchivedAtUtc = null;
            e.ArchiveReason = null;
            e.UpdatedByUserId = userId;
            e.UpdatedAtUtc = DateTime.UtcNow;
            _contractDal.Update(e);
            return new SuccessResult("قرارداد فعال شد.");
        }

        public IDataResult<List<ContractListItemDto>> GetListItems()
        {
            // ----------------------------
            // BASE QUERIES - Hepsini memory'ye al
            // ----------------------------
            var contracts = _contractDal.QueryNoTracking().ToList();
            var shops = _shopDal.QueryNoTracking().ToList();
            var tenants = _tenantDal.QueryNoTracking().ToList();
            var markets = _marketDal.QueryNoTracking().ToList();
            var floors = _floorDal.QueryNoTracking().ToList();
            var accruals = _accrualDal.QueryNoTracking().ToList();
            var shares = _shopFranchiseShareDal.QueryNoTracking().ToList();
            var owners = _franchiseOwnerDal.QueryNoTracking().ToList();

            //var evalDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var evalDate = _timeProvider.Today();          // DateOnly (Kabul, Gregorian)
            var nowUtc = _timeProvider.UtcNow();
            var (curY, curM, _) = _timeProvider.TodayPersian();

            // ----------------------------
            // AKTİF FRANCHISE SAHİPLERİ (memory içi)
            // ----------------------------
            var activeShares = shares
                .Where(s =>
                    s.EffectiveFrom <= evalDate &&
                    (s.EffectiveTo == null || evalDate <= s.EffectiveTo))
                .ToList();

            // Dükkan başına aktif imtiyaz sahibi sayısı
            var ownersCountPerShop = activeShares
                .GroupBy(s => s.ShopId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Dükkan başına en yüksek yüzde
            var topPercentPerShop = activeShares
                .GroupBy(s => s.ShopId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Max(x => x.SharePercent)  // ShopFranchiseShare.SharePercent decimal
                );

            // En yüksek paya sahip imtiyaz sahibinin adı
            var topOwnerPerShop = activeShares
                .GroupBy(s => s.ShopId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        if (!topPercentPerShop.TryGetValue(g.Key, out var topPct))
                            return (string?)null;

                        var name = g
                            .Where(x => x.SharePercent == topPct)
                            .Join(
                                owners,
                                s => s.FranchiseOwnerId,
                                o => o.Id,
                                (s, o) => o.DisplayName)
                            .OrderBy(n => n)          // eşitse alfabetik ilk
                            .FirstOrDefault();

                        return name;
                    });

            // ----------------------------
            // ELEKTRİK (bu ay) - memory içi
            // ----------------------------
            var electricityThisPeriod = accruals
                .Where(a =>
                    a.Kind == AccrualKind.Electricity &&
                    a.Year == curY &&
                    a.Month == curM)
                .GroupBy(a => a.ContractId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(z => z.Amount)   // Amount decimal, null olamaz
                );

            // BU DÖNEM (curY/curM) TAHAKKUK TOPLAMI (Amount)
            var currentPeriodAccrualTotals = accruals
                .Where(a => a.Year == curY && a.Month == curM)
                .GroupBy(a => a.ContractId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(a => a.Amount)
                );

            // BU DÖNEM ÖDENMEMİŞ BORÇ (Outstanding)
            var currentPeriodOutstandingTotals = accruals
                .Where(a =>
                    a.Year == curY &&
                    a.Month == curM &&
                    a.Status != AccrualStatus.Settled)
                .GroupBy(a => a.ContractId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(a => a.OutstandingAmount)
                );

            // ESKİ DÖNEMLERDEN (curY/curM ÖNCESİ) DEVREDEN BORÇ
            var oldOutstandingTotals = accruals
                .Where(a =>
                    a.Status != AccrualStatus.Settled &&
                    (
                        a.Year < curY ||
                        (a.Year == curY && a.Month < curM)
                    ))
                .GroupBy(a => a.ContractId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(a => a.OutstandingAmount)
                );


            // ----------------------------
            // ANA SORGU - memory join
            // ----------------------------
            var list =
                (from c in contracts
                 join s in shops on c.ShopId equals s.Id
                 join t in tenants on c.TenantId equals t.Id
                 join m in markets on s.MarketId equals m.Id
                 join fl in floors on s.FloorId equals fl.Id

                 select new ContractListItemDto
                 {
                     ContractId = c.Id,
                     ShopId = c.ShopId,
                     TenantId = c.TenantId,
                     MarketId = s.MarketId,
                     FloorId = s.FloorId,

                     MarketName = m.MarketName ?? string.Empty,
                     FloorName = fl.FloorName ?? string.Empty,
                     ShopName = s.Name ?? string.Empty,
                     TenantName = t.DisplayName ?? string.Empty,

                     // FRANCHISE
                     FranchiseOwnersCount = ownersCountPerShop.TryGetValue(s.Id, out var cnt)
                         ? cnt
                         : 0,

                     TopFranchiseOwnerName = topOwnerPerShop.TryGetValue(s.Id, out var topName)
                         ? topName
                         : null,

                     FranchiseSummary = ownersCountPerShop.TryGetValue(s.Id, out var ownerCount) && ownerCount > 0
                         ? ownerCount == 1
                             ? (topOwnerPerShop.TryGetValue(s.Id, out var singleName) ? singleName : "")
                             : ((topOwnerPerShop.TryGetValue(s.Id, out var firstName) ? firstName : "") +
                                " + " + (ownerCount - 1) + " شخص دیگر...")
                         : string.Empty,

                     FranchiseName = topOwnerPerShop.TryGetValue(s.Id, out var fn) ? fn : "",

                     // TARİHLER
                     ContractStartDate = c.StartDate,
                     ContractEndDate = c.EndDate,

                     // SABİT TUTARLAR
                     OwnerPortionAmount = c.OwnerPortionAmount,
                     MarketPortionAmount = c.MarketPortionAmount,
                     FranchisePortionAmount = c.FranchisePortionAmount,
                     TotalFixedRent =
                         c.OwnerPortionAmount +
                         c.MarketPortionAmount +
                         c.FranchisePortionAmount,

                     BusinessActivity = c.BusinessActivity ?? "",

                     // ELEKTRİK
                     ElectricityThisPeriodTotal = electricityThisPeriod.TryGetValue(c.Id, out var elecTotal)
                         ? elecTotal
                         : (decimal?)null,

                     // BORÇ SONRADAN HESAPLANACAK
                     // BU DÖNEM TOPLAM TAHAKKUK (Owner+Market+Franchise+Electricity)
                     CurrentPeriodAccrualTotal = currentPeriodAccrualTotals.TryGetValue(c.Id, out var accrualTotal)
        ? accrualTotal
        : 0m,

                     // BU DÖNEM ÖDENMEMİŞ KISIM
                     CurrentPeriodOutstanding = currentPeriodOutstandingTotals.TryGetValue(c.Id, out var curOut)
        ? curOut
        : 0m,

                     // ESKİ DÖNEMLERDEN DEVREDEN BORÇ
                     OldOutstanding = oldOutstandingTotals.TryGetValue(c.Id, out var oldOut)
                        ? oldOut
                        : 0m,

                     // TOPLAM ÖDENMEMİŞ BORÇ
                     TotalOutstanding =
                        (oldOutstandingTotals.TryGetValue(c.Id, out var o1) ? o1 : 0m) +
                        (currentPeriodOutstandingTotals.TryGetValue(c.Id, out var c1) ? c1 : 0m),

                     // DİĞER
                     DueDayOfMonth = c.DueDayOfMonth,
                     Status = c.Status
                 })
                .OrderBy(x => x.MarketName)
                .ThenBy(x => x.FloorName)
                .ThenBy(x => x.ShopName)
                .ToList();

            // ----------------------------
            // ESKİ BORÇ (client side)
            // ----------------------------
            foreach (var item in list)
            {
                var cutoff = new DateTime(
                    item.ContractStartDate.Year,
                    item.ContractStartDate.Month,
                    item.ContractStartDate.Day);

                item.OldDebtTotal = accruals
                    .Where(a =>
                        a.ContractId == item.ContractId &&
                        a.Status != AccrualStatus.Settled &&
                        a.CreatedAtUtc < cutoff)
                    .Sum(a => a.OutstandingAmount);  // OutstandingAmount decimal
            }

            return new SuccessDataResult<List<ContractListItemDto>>(list);
        }

    }
}
