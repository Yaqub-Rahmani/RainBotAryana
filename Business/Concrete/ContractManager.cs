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
        private readonly IMeterReadingDal _meterReadingDal;
        private readonly IShopFranchiseShareDal _shopFranchiseShareDal;
        private readonly IFranchiseOwnerDal _franchiseOwnerDal;
        private readonly ICollectionDal _collectionDal;
        private readonly ICollectionAllocationDal _collectionAllocationDal;

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
            IMeterReadingDal meterReadingDal,
            ICollectionDal collectionDal,
            ICollectionAllocationDal collectionAllocationDal
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
            _collectionDal = collectionDal;
            _collectionAllocationDal = collectionAllocationDal;
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

        // -----------------------------
        // DETAY METODU
        // -----------------------------
        //[CacheAspect(10)]
        public IDataResult<ContractDetailDto> GetDetail(Guid contractId)
        {
            // Bugünün tarihi (Kabul)
            var evalDate = _timeProvider.Today();
            var (curY, curM, _) = _timeProvider.TodayPersian();

            // Ana sözleşme
            var contract = _contractDal
                .QueryNoTracking()
                .FirstOrDefault(c => c.Id == contractId);

            if (contract == null)
                return new ErrorDataResult<ContractDetailDto>(null, "Sözleşme bulunamadı.");

            // Shop / Tenant / Market / Floor
            var shop = _shopDal.QueryNoTracking().First(s => s.Id == contract.ShopId);
            var tenant = _tenantDal.QueryNoTracking().First(t => t.Id == contract.TenantId);
            var market = _marketDal.QueryNoTracking().First(m => m.Id == shop.MarketId);
            var floor = _floorDal.QueryNoTracking().First(f => f.Id == shop.FloorId);

            // -----------------------------
            // FRANCHISE PAYLARI
            // -----------------------------
            var shares = _shopFranchiseShareDal
                .QueryNoTracking()
                .Where(s => s.ShopId == shop.Id)
                .ToList();

            var ownerIds = shares
                .Select(s => s.FranchiseOwnerId)
                .Distinct()
                .ToList();

            var owners = _franchiseOwnerDal
                .QueryNoTracking()
                .Where(o => ownerIds.Contains(o.Id))
                .ToList();

            var franchiseDetails = (
                    from s in shares
                    join o in owners on s.FranchiseOwnerId equals o.Id
                    select new FranchiseOwnerShareDto
                    {
                        FranchiseOwnerId = o.Id,
                        FranchiseOwnerName = o.DisplayName,
                        SharePercent = s.SharePercent,
                        EffectiveFrom = s.EffectiveFrom,
                        EffectiveTo = s.EffectiveTo,
                        IsActiveOnEvalDate =
                            s.EffectiveFrom <= evalDate &&
                            (s.EffectiveTo == null || evalDate <= s.EffectiveTo)
                    })
                .OrderByDescending(x => x.SharePercent)
                .ThenBy(x => x.FranchiseOwnerName)
                .ToList();

            var franchiseOwnersCount = franchiseDetails.Count;
            var topFranchiseOwnerName = franchiseDetails.FirstOrDefault()?.FranchiseOwnerName;

            string franchiseSummary;
            if (franchiseOwnersCount == 0)
            {
                franchiseSummary = string.Empty;
            }
            else if (franchiseOwnersCount == 1)
            {
                franchiseSummary = topFranchiseOwnerName ?? string.Empty;
            }
            else
            {
                franchiseSummary = $"{topFranchiseOwnerName} + {franchiseOwnersCount - 1} شخص دیگر...";
            }

            // -----------------------------
            // TAHAKKUKLAR (tüm dönemler)
            // -----------------------------
            var accruals = _accrualDal
                .QueryNoTracking()
                .Where(a => a.ContractId == contract.Id)
                .ToList();

            var currentPeriodAccrualTotal = accruals
                .Where(a => a.Year == curY && a.Month == curM)
                .Sum(a => a.Amount);

            var currentPeriodOutstanding = accruals
                .Where(a =>
                    a.Year == curY &&
                    a.Month == curM &&
                    a.Status != AccrualStatus.Settled)
                .Sum(a => a.OutstandingAmount);

            var oldOutstanding = accruals
                .Where(a =>
                    a.Status != AccrualStatus.Settled &&
                    (a.Year < curY || (a.Year == curY && a.Month < curM)))
                .Sum(a => a.OutstandingAmount);

            var totalOutstanding = accruals
                .Where(a => a.Status != AccrualStatus.Settled)
                .Sum(a => a.OutstandingAmount);

            // Eski borç – start date öncesi gibi düşünebilirsin; istersen CreatedAtUtc’a göre sıkılaştırırsın
            var oldDebtTotal = accruals
                .Where(a => a.Status != AccrualStatus.Settled)
                .Sum(a => a.OutstandingAmount);

            // Aylık tahakkuk geçmişi (örneğin son 24 ay)
            var accrualHistory = accruals
                .GroupBy(a => new { a.Year, a.Month })
                .OrderByDescending(g => g.Key.Year)
                .ThenByDescending(g => g.Key.Month)
                .Select(g => new AccrualHistoryItemDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    OwnerAmount = g.Where(a => a.Kind == AllocationKind.Owner).Sum(a => a.Amount),
                    FranchiseAmount = g.Where(a => a.Kind == AllocationKind.Franchise).Sum(a => a.Amount),
                    MarketAmount = g.Where(a => a.Kind == AllocationKind.Market).Sum(a => a.Amount),
                    ElectricityAmount = g.Where(a => a.Kind == AllocationKind.Electricity).Sum(a => a.Amount),
                    TotalAmount = g.Sum(a => a.Amount),
                    OutstandingAmount = g.Sum(a => a.OutstandingAmount)
                })
                .Take(24)
                .ToList();

            // -----------------------------
            // TAHSİLATLAR
            // -----------------------------
            var collections = _collectionDal
                .QueryNoTracking()
                .Where(c => c.ContractId == contract.Id)
                .OrderByDescending(c => c.PaidAtUtc)
                .Take(50)
                .ToList();

            var collectionIds = collections.Select(c => c.Id).ToList();

            var allocations = _collectionAllocationDal
                .QueryNoTracking()
                .Where(a => collectionIds.Contains(a.CollectionId))
                .ToList();

            var collectionHistory = collections
                .Select(c =>
                {
                    var colAllocs = allocations.Where(a => a.CollectionId == c.Id);

                    return new CollectionHistoryItemDto
                    {
                        CollectionId = c.Id,
                        //PaidAtUtc = c.PaidAtUtc,
                        PaidAtUtc = DateOnly.FromDateTime(c.PaidAtUtc),

                        AmountReceived = c.AmountReceived,
                        Method = c.Method,
                        ReferenceNo = c.ReferenceNo,
                        Notes = c.Notes,

                        OwnerAllocated = colAllocs
                            .Where(a => a.Kind == AllocationKind.Owner)
                            .Sum(a => a.Amount),

                        MarketAllocated = colAllocs
                            .Where(a => a.Kind == AllocationKind.Market)
                            .Sum(a => a.Amount),

                        FranchiseAllocated = colAllocs
                            .Where(a => a.Kind == AllocationKind.Franchise)
                            .Sum(a => a.Amount),

                        ElectricityAllocated = colAllocs
                            .Where(a => a.Kind == AllocationKind.Electricity)
                            .Sum(a => a.Amount),
                    };
                })
                .ToList();

            // -----------------------------
            // ANA DTO
            // -----------------------------
            var dto = new ContractDetailDto
            {
                ContractId = contract.Id,
                ShopId = contract.ShopId,
                TenantId = contract.TenantId,
                MarketId = shop.MarketId,
                FloorId = shop.FloorId,

                MarketName = market.MarketName,
                FloorName = floor.FloorName,
                ShopName = shop.Name ?? string.Empty,
                TenantName = tenant.DisplayName,

                SignedOn = contract.SignedOn,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                DueDayOfMonth = contract.DueDayOfMonth,

                OwnerPortionAmount = contract.OwnerPortionAmount,
                MarketPortionAmount = contract.MarketPortionAmount,
                FranchisePortionAmount = contract.FranchisePortionAmount,
                TotalFixedRent = contract.TotalFixedRent,

                BusinessActivity = contract.BusinessActivity ?? string.Empty,
                Status = contract.Status,
                ContractVersion = contract.Version,
                ContractNote = contract.Note,
                ContractArchiveReason = contract.ArchiveReason,

                ElectricityNote = contract.ElectricityNote,
                InitialElectricityIndex = contract.InitialElectricityIndex,

                FranchiseOwnersCount = franchiseOwnersCount,
                TopFranchiseOwnerName = topFranchiseOwnerName,
                FranchiseSummary = franchiseSummary,
                FranchiseOwners = franchiseDetails,

                CurrentPeriodAccrualTotal = currentPeriodAccrualTotal,
                CurrentPeriodOutstanding = currentPeriodOutstanding,
                OldOutstanding = oldOutstanding,
                TotalOutstanding = totalOutstanding,
                OldDebtTotal = oldDebtTotal,

                AccrualHistory = accrualHistory,
                CollectionHistory = collectionHistory
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
                SignedOn = DateOnly.FromDateTime(now),
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

public IDataResult<PagedResult<ContractListItemDto>> GetListItems(QueryOptions opt)
    {
        opt ??= new QueryOptions();

        var evalDate = _timeProvider.Today();          // DateOnly
        var (curY, curM, _) = _timeProvider.TodayPersian();

        var page = opt.Page <= 0 ? 1 : opt.Page;
        var pageSize = opt.PageSize <= 0 ? 25 : opt.PageSize;

        // ------------------------------------------------------------
        // 1) Deferred (computed) filter flags + DB filter list
        // ------------------------------------------------------------
        var allFilters = opt.Filters ?? new List<string>();

        bool? hasOldDebt = null;       // OldDebtTotal:eq:has|no
        bool? hasFranchise = null;     // FranchiseOwnersCount:eq:has|no

        foreach (var f in allFilters)
        {
            if (string.IsNullOrWhiteSpace(f)) continue;

            var parts = f.Split(':');
            if (parts.Length < 3) continue;

            var field = parts[0];
            var op = parts[1];
            var value = string.Join(":", parts.Skip(2));

            if (field.Equals("OldDebtTotal", StringComparison.OrdinalIgnoreCase) &&
                op.Equals("eq", StringComparison.OrdinalIgnoreCase))
            {
                if (value.Equals("has", StringComparison.OrdinalIgnoreCase)) hasOldDebt = true;
                else if (value.Equals("no", StringComparison.OrdinalIgnoreCase)) hasOldDebt = false;
            }

            if (field.Equals("FranchiseOwnersCount", StringComparison.OrdinalIgnoreCase) &&
                op.Equals("eq", StringComparison.OrdinalIgnoreCase))
            {
                if (value.Equals("has", StringComparison.OrdinalIgnoreCase)) hasFranchise = true;
                else if (value.Equals("no", StringComparison.OrdinalIgnoreCase)) hasFranchise = false;
            }
        }

        // DB’ye gidecek filtreler (computed olanları çıkar)
        var dbFilters = allFilters
            .Where(f =>
            {
                var p = f.Split(':');
                if (p.Length < 2) return true;

                var field = p[0];
                if (field.Equals("OldDebtTotal", StringComparison.OrdinalIgnoreCase)) return false;
                if (field.Equals("FranchiseOwnersCount", StringComparison.OrdinalIgnoreCase)) return false;
                return true;
            })
            .ToList();

        // Sort computed alan mı? (deferred sort)
        var sort = opt.Sort ?? "";
        var sortDeferred = false;
        if (!string.IsNullOrWhiteSpace(sort))
        {
            var sortField = sort.Split(':')[0];
            sortDeferred =
                sortField.Equals("OldDebtTotal", StringComparison.OrdinalIgnoreCase) ||
                sortField.Equals("FranchiseOwnersCount", StringComparison.OrdinalIgnoreCase) ||
                sortField.Equals("FranchiseSummary", StringComparison.OrdinalIgnoreCase);
        }

        // ------------------------------------------------------------
        // 2) BASE QUERY (DB filtrelenebilir alanlar) -> DTO projection
        // ------------------------------------------------------------
        IQueryable<ContractListItemDto> q =
            from c in _contractDal.QueryNoTracking()
            join s in _shopDal.QueryNoTracking() on c.ShopId equals s.Id
            join t in _tenantDal.QueryNoTracking() on c.TenantId equals t.Id
            join m in _marketDal.QueryNoTracking() on s.MarketId equals m.Id
            join fl in _floorDal.QueryNoTracking() on s.FloorId equals fl.Id
            select new ContractListItemDto
            {
                ContractId = c.Id,
                ShopId = c.ShopId,
                TenantId = c.TenantId,

                MarketId = s.MarketId,
                FloorId = s.FloorId,

                MarketName = m.MarketName ?? "",
                FloorName = fl.FloorName ?? "",
                ShopName = s.Name ?? "",
                TenantName = t.DisplayName ?? "",

                BusinessActivity = c.BusinessActivity ?? "",

                ContractStartDate = c.StartDate,
                ContractEndDate = c.EndDate,

                OwnerPortionAmount = c.OwnerPortionAmount,
                MarketPortionAmount = c.MarketPortionAmount,
                FranchisePortionAmount = c.FranchisePortionAmount,
                TotalFixedRent = c.OwnerPortionAmount + c.MarketPortionAmount + c.FranchisePortionAmount,

                DueDayOfMonth = c.DueDayOfMonth,
                Status = c.Status,

                // computed (init-only ise burada ver)
                FranchiseOwnersCount = 0,
                FranchiseSummary = "",
                FranchiseName = "",
                TopFranchiseOwnerName = null,

                ElectricityThisPeriodTotal = null,
                CurrentPeriodAccrualTotal = 0m,
                CurrentPeriodOutstanding = 0m,
                OldOutstanding = 0m,
                TotalOutstanding = 0m,
                OldDebtTotal = 0m
            };

        // DB filters / search / sort (computed sort değilse)
        if (dbFilters.Count > 0) q = q.ApplyFilters(dbFilters);

        if (!string.IsNullOrWhiteSpace(opt.Search))
 //           q = q.ApplySearch(opt.Search);

        if (!sortDeferred && !string.IsNullOrWhiteSpace(sort))
            q = q.ApplySort(sort);

        // DB’den aday liste (computed olmayan kriterlerle daraltılmış)
        var baseItems = q.ToList();

        if (baseItems.Count == 0)
        {
            var empty = BuildPagedResult(new List<ContractListItemDto>(), page, pageSize, 0);
            return new SuccessDataResult<PagedResult<ContractListItemDto>>(empty);
        }

        // ------------------------------------------------------------
        // 3) Load only needed related data
        // ------------------------------------------------------------
        var contractIds = baseItems.Select(x => x.ContractId).Distinct().ToList();
        var shopIds = baseItems.Select(x => x.ShopId).Distinct().ToList();

        var shares = _shopFranchiseShareDal.QueryNoTracking()
            .Where(s => shopIds.Contains(s.ShopId))
            .ToList();

        var owners = _franchiseOwnerDal.QueryNoTracking().ToList();

        var accruals = _accrualDal.QueryNoTracking()
            .Where(a => contractIds.Contains(a.ContractId))
            .ToList();

        // ------------------------------------------------------------
        // 4) Compute franchise (active)
        // ------------------------------------------------------------
        var activeShares = shares
            .Where(s => s.EffectiveFrom <= evalDate && (s.EffectiveTo == null || evalDate <= s.EffectiveTo))
            .ToList();

        var ownersCountPerShop = activeShares
            .GroupBy(s => s.ShopId)
            .ToDictionary(g => g.Key, g => g.Count());

        var topPercentPerShop = activeShares
            .GroupBy(s => s.ShopId)
            .ToDictionary(g => g.Key, g => g.Max(x => x.SharePercent));

        var topOwnerPerShop = activeShares
            .GroupBy(s => s.ShopId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    if (!topPercentPerShop.TryGetValue(g.Key, out var topPct)) return (string?)null;

                    return g.Where(x => x.SharePercent == topPct)
                        .Join(owners, s => s.FranchiseOwnerId, o => o.Id, (s, o) => o.DisplayName)
                        .OrderBy(n => n)
                        .FirstOrDefault();
                });

        // ------------------------------------------------------------
        // 5) Compute accrual aggregates
        // ------------------------------------------------------------
        var electricityThisPeriod = accruals
            .Where(a => a.Kind == AllocationKind.Electricity && a.Year == curY && a.Month == curM)
            .GroupBy(a => a.ContractId)
            .ToDictionary(g => g.Key, g => g.Sum(z => z.Amount));

        var currentPeriodAccrualTotals = accruals
            .Where(a => a.Year == curY && a.Month == curM)
            .GroupBy(a => a.ContractId)
            .ToDictionary(g => g.Key, g => g.Sum(z => z.Amount));

        var currentPeriodOutstandingTotals = accruals
            .Where(a => a.Year == curY && a.Month == curM && a.Status != AccrualStatus.Settled)
            .GroupBy(a => a.ContractId)
            .ToDictionary(g => g.Key, g => g.Sum(z => z.OutstandingAmount));

        var oldOutstandingTotals = accruals
            .Where(a => a.Status != AccrualStatus.Settled &&
                        (a.Year < curY || (a.Year == curY && a.Month < curM)))
            .GroupBy(a => a.ContractId)
            .ToDictionary(g => g.Key, g => g.Sum(z => z.OutstandingAmount));

        var oldDebtTotals = accruals
            .Where(a => a.Status != AccrualStatus.Settled)
            .GroupBy(a => a.ContractId)
            .ToDictionary(g => g.Key, g => g.Sum(z => z.OutstandingAmount));

        // ------------------------------------------------------------
        // 6) Build final items (DTO init-only => create new objects)
        // ------------------------------------------------------------
        var finalItems = baseItems.Select(it =>
        {
            var ownerCount = ownersCountPerShop.TryGetValue(it.ShopId, out var cnt) ? cnt : 0;
            var topName = topOwnerPerShop.TryGetValue(it.ShopId, out var tn) ? tn : null;

            var franchiseSummary =
                ownerCount <= 0 ? "" :
                ownerCount == 1 ? (topName ?? "") :
                $"{(topName ?? "")} + {ownerCount - 1} شخص دیگر...";

            var elec = electricityThisPeriod.TryGetValue(it.ContractId, out var e) ? e : (decimal?)null;
            var accrTotal = currentPeriodAccrualTotals.TryGetValue(it.ContractId, out var at) ? at : 0m;
            var curOut = currentPeriodOutstandingTotals.TryGetValue(it.ContractId, out var co) ? co : 0m;
            var oldOut = oldOutstandingTotals.TryGetValue(it.ContractId, out var oo) ? oo : 0m;
            var oldDebt = oldDebtTotals.TryGetValue(it.ContractId, out var od) ? od : 0m;

            return new ContractListItemDto
            {
                // base fields
                ContractId = it.ContractId,
                ShopId = it.ShopId,
                TenantId = it.TenantId,
                MarketId = it.MarketId,
                FloorId = it.FloorId,

                MarketName = it.MarketName,
                FloorName = it.FloorName,
                ShopName = it.ShopName,
                TenantName = it.TenantName,

                BusinessActivity = it.BusinessActivity,

                ContractStartDate = it.ContractStartDate,
                ContractEndDate = it.ContractEndDate,

                OwnerPortionAmount = it.OwnerPortionAmount,
                MarketPortionAmount = it.MarketPortionAmount,
                FranchisePortionAmount = it.FranchisePortionAmount,
                TotalFixedRent = it.TotalFixedRent,

                DueDayOfMonth = it.DueDayOfMonth,
                Status = it.Status,

                // computed
                FranchiseOwnersCount = ownerCount,
                TopFranchiseOwnerName = topName,
                FranchiseName = topName ?? "",
                FranchiseSummary = franchiseSummary,

                ElectricityThisPeriodTotal = elec,
                CurrentPeriodAccrualTotal = accrTotal,
                CurrentPeriodOutstanding = curOut,
                OldOutstanding = oldOut,
                TotalOutstanding = oldOut + curOut,
                OldDebtTotal = oldDebt
            };
        }).ToList();

        // ------------------------------------------------------------
        // 7) Apply deferred filters
        // ------------------------------------------------------------
        if (hasOldDebt.HasValue)
            finalItems = hasOldDebt.Value
                ? finalItems.Where(x => x.OldDebtTotal > 0m).ToList()
                : finalItems.Where(x => x.OldDebtTotal <= 0m).ToList();

        if (hasFranchise.HasValue)
            finalItems = hasFranchise.Value
                ? finalItems.Where(x => x.FranchiseOwnersCount > 0).ToList()
                : finalItems.Where(x => x.FranchiseOwnersCount <= 0).ToList();

        // ------------------------------------------------------------
        // 8) Deferred sort (if requested on computed field)
        // ------------------------------------------------------------
        if (sortDeferred && !string.IsNullOrWhiteSpace(sort))
        {
            var parts = sort.Split(':');
            var field = parts[0];
            var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            finalItems = field.ToLowerInvariant() switch
            {
                "olddebttotal" => desc
                    ? finalItems.OrderByDescending(x => x.OldDebtTotal).ToList()
                    : finalItems.OrderBy(x => x.OldDebtTotal).ToList(),

                "franchiseownerscount" => desc
                    ? finalItems.OrderByDescending(x => x.FranchiseOwnersCount).ToList()
                    : finalItems.OrderBy(x => x.FranchiseOwnersCount).ToList(),

                "franchisesummary" => desc
                    ? finalItems.OrderByDescending(x => x.FranchiseSummary).ToList()
                    : finalItems.OrderBy(x => x.FranchiseSummary).ToList(),

                _ => finalItems
            };
        }

        // ------------------------------------------------------------
        // 9) Paging (build PagedResult<T> without assuming ctor)
        // ------------------------------------------------------------
        var totalCount = finalItems.Count;
        var pageItems = finalItems
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = BuildPagedResult(pageItems, page, pageSize, totalCount);
        return new SuccessDataResult<PagedResult<ContractListItemDto>>(result);
    }

    private static PagedResult<T> BuildPagedResult<T>(IList<T> pageItems, int page, int pageSize, int totalCount)
    {
        // Senin PagedResult<T> ctor’larını bilmediğimiz için runtime’da uygun ctor’u yakalayıp kullanıyoruz.
        // 1) (IList<T>, int, int, int) / (IEnumerable<T>, int, int, int)
        // 2) (IList<T>, int, int) / (IEnumerable<T>, int, int)
        // 3) parameterless + property set (varsa)
        var t = typeof(PagedResult<T>);
        object? instance = null;

        object[] args4a = { pageItems, page, pageSize, totalCount };
        object[] args4b = { pageItems.AsEnumerable(), page, pageSize, totalCount };
        object[] args3a = { pageItems, page, pageSize };
        object[] args3b = { pageItems.AsEnumerable(), page, pageSize };

        var ctors = t.GetConstructors();

        instance =
            TryCreate(ctors, args4a) ??
            TryCreate(ctors, args4b) ??
            TryCreate(ctors, args3a) ??
            TryCreate(ctors, args3b);

        if (instance == null)
        {
            // parameterless ctor dene
            var ctor0 = ctors.FirstOrDefault(c => c.GetParameters().Length == 0);
            if (ctor0 != null)
                instance = ctor0.Invoke(Array.Empty<object>());
        }

        if (instance == null)
            throw new InvalidOperationException("PagedResult<T> için uygun constructor bulunamadı. PagedResult<T> sınıfını kontrol et.");

        // Property set (varsa)
        SetIfWritable(instance, "Items", pageItems);
        SetIfWritable(instance, "Page", page);
        SetIfWritable(instance, "PageSize", pageSize);
        SetIfWritable(instance, "TotalCount", totalCount);

        // Bazı implementasyonlarda isimler farklı olabilir:
        SetIfWritable(instance, "Data", pageItems);
        SetIfWritable(instance, "Records", pageItems);

        return (PagedResult<T>)instance;
    }

    private static object? TryCreate(System.Reflection.ConstructorInfo[] ctors, object[] args)
    {
        foreach (var c in ctors)
        {
            var ps = c.GetParameters();
            if (ps.Length != args.Length) continue;

            bool ok = true;
            for (int i = 0; i < ps.Length; i++)
            {
                if (args[i] == null) continue;
                if (!ps[i].ParameterType.IsAssignableFrom(args[i]!.GetType()))
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
                return c.Invoke(args);
        }
        return null;
    }

    private static void SetIfWritable(object instance, string propName, object value)
    {
        var p = instance.GetType().GetProperty(propName);
        if (p == null) return;
        if (!p.CanWrite) return;
        if (value != null && !p.PropertyType.IsAssignableFrom(value.GetType()))
        {
            // IEnumerable<T> bekliyorsa
            if (p.PropertyType.IsAssignableFrom(value.GetType())) { }
            else return;
        }
        p.SetValue(instance, value);
    }


    //public IDataResult<PagedResult<ContractListItemDto>> GetListItems(QueryOptions opt)
    //{
    //    opt ??= new QueryOptions();

    //    var evalDate = _timeProvider.Today();
    //    var (curY, curM, _) = _timeProvider.TodayPersian();

    //    // -------------------------------------------------
    //    // BASE QUERY (IQueryable)
    //    // -------------------------------------------------
    //    var baseQuery =
    //        from c in _contractDal.QueryNoTracking()
    //        join s in _shopDal.QueryNoTracking() on c.ShopId equals s.Id
    //        join t in _tenantDal.QueryNoTracking() on c.TenantId equals t.Id
    //        join m in _marketDal.QueryNoTracking() on s.MarketId equals m.Id
    //        join fl in _floorDal.QueryNoTracking() on s.FloorId equals fl.Id
    //        select new
    //        {
    //            Contract = c,
    //            Shop = s,
    //            Tenant = t,
    //            Market = m,
    //            Floor = fl
    //        };

    //    // -------------------------------------------------
    //    // FILTER / SEARCH / SORT
    //    // -------------------------------------------------
    //    baseQuery = baseQuery.ApplyFilters(opt.Filters);
    //    baseQuery = baseQuery.ApplySearch(opt.Search, null);
    //    baseQuery = baseQuery.ApplySort(opt.Sort);

    //    // -------------------------------------------------
    //    // COUNT (paging öncesi)
    //    // -------------------------------------------------
    //    var totalCount = baseQuery.Count();

    //    // -------------------------------------------------
    //    // PAGING
    //    // -------------------------------------------------
    //    var pageRows = baseQuery
    //        .ApplyPaging(opt.Page, opt.PageSize)
    //        .ToList();

    //    var contractIds = pageRows.Select(x => x.Contract.Id).ToList();
    //    var shopIds = pageRows.Select(x => x.Shop.Id).ToList();

    //    // -------------------------------------------------
    //    // MEMORY DATA (SADECE GEREKLİ KAYITLAR)
    //    // -------------------------------------------------

    //    var accruals = _accrualDal.QueryNoTracking()
    //        .Where(a => contractIds.Contains(a.ContractId))
    //        .ToList();

    //    var shares = _shopFranchiseShareDal.QueryNoTracking()
    //        .Where(s => shopIds.Contains(s.ShopId))
    //        .ToList();

    //    var owners = _franchiseOwnerDal.QueryNoTracking().ToList();

    //    // -------------------------------------------------
    //    // ACTIVE SHARES
    //    // -------------------------------------------------
    //    var activeShares = shares
    //        .Where(s =>
    //            s.EffectiveFrom <= evalDate &&
    //            (s.EffectiveTo == null || evalDate <= s.EffectiveTo))
    //        .ToList();

    //    var ownersCountPerShop = activeShares
    //        .GroupBy(s => s.ShopId)
    //        .ToDictionary(g => g.Key, g => g.Count());

    //    var topOwnerPerShop = activeShares
    //        .GroupBy(s => s.ShopId)
    //        .ToDictionary(
    //            g => g.Key,
    //            g =>
    //                g.Join(
    //                    owners,
    //                    s => s.FranchiseOwnerId,
    //                    o => o.Id,
    //                    (s, o) => new { s.SharePercent, o.DisplayName }
    //                )
    //                .OrderByDescending(x => x.SharePercent)
    //                .ThenBy(x => x.DisplayName)
    //                .Select(x => x.DisplayName)
    //                .FirstOrDefault()
    //        );

    //    // -------------------------------------------------
    //    // ACCRUAL CALCULATIONS
    //    // -------------------------------------------------
    //    var electricityThisPeriod = accruals
    //        .Where(a => a.Kind == AllocationKind.Electricity && a.Year == curY && a.Month == curM)
    //        .GroupBy(a => a.ContractId)
    //        .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

    //    var currentAccrualTotals = accruals
    //        .Where(a => a.Year == curY && a.Month == curM)
    //        .GroupBy(a => a.ContractId)
    //        .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

    //    var currentOutstanding = accruals
    //        .Where(a => a.Year == curY && a.Month == curM && a.Status != AccrualStatus.Settled)
    //        .GroupBy(a => a.ContractId)
    //        .ToDictionary(g => g.Key, g => g.Sum(x => x.OutstandingAmount));

    //    var oldOutstanding = accruals
    //        .Where(a =>
    //            a.Status != AccrualStatus.Settled &&
    //            (a.Year < curY || (a.Year == curY && a.Month < curM)))
    //        .GroupBy(a => a.ContractId)
    //        .ToDictionary(g => g.Key, g => g.Sum(x => x.OutstandingAmount));

    //    // -------------------------------------------------
    //    // FINAL DTO PROJECTION (MEMORY)
    //    // -------------------------------------------------
    //    var items = pageRows.Select(x =>
    //    {
    //        var c = x.Contract;
    //        var s = x.Shop;

    //        var ownerCount = ownersCountPerShop.TryGetValue(s.Id, out var cnt) ? cnt : 0;
    //        var topOwner = topOwnerPerShop.TryGetValue(s.Id, out var nm) ? nm : null;

    //        return new ContractListItemDto
    //        {
    //            ContractId = c.Id,
    //            ShopId = c.ShopId,
    //            TenantId = c.TenantId,
    //            MarketId = s.MarketId,
    //            FloorId = s.FloorId,

    //            MarketName = x.Market.MarketName ?? "",
    //            FloorName = x.Floor.FloorName ?? "",
    //            ShopName = s.Name ?? "",
    //            TenantName = x.Tenant.DisplayName ?? "",

    //            FranchiseOwnersCount = ownerCount,
    //            TopFranchiseOwnerName = topOwner,

    //            FranchiseSummary =
    //                ownerCount == 0 ? "" :
    //                ownerCount == 1 ? topOwner ?? "" :
    //                $"{topOwner} + {ownerCount - 1} شخص دیگر...",

    //            ContractStartDate = c.StartDate,
    //            ContractEndDate = c.EndDate,

    //            OwnerPortionAmount = c.OwnerPortionAmount,
    //            MarketPortionAmount = c.MarketPortionAmount,
    //            FranchisePortionAmount = c.FranchisePortionAmount,

    //            TotalFixedRent =
    //                c.OwnerPortionAmount +
    //                c.MarketPortionAmount +
    //                c.FranchisePortionAmount,

    //            BusinessActivity = c.BusinessActivity ?? "",

    //            ElectricityThisPeriodTotal =
    //                electricityThisPeriod.TryGetValue(c.Id, out var e) ? e : (decimal?)null,

    //            CurrentPeriodAccrualTotal =
    //                currentAccrualTotals.TryGetValue(c.Id, out var a) ? a : 0m,

    //            CurrentPeriodOutstanding =
    //                currentOutstanding.TryGetValue(c.Id, out var co) ? co : 0m,

    //            OldOutstanding =
    //                oldOutstanding.TryGetValue(c.Id, out var oo) ? oo : 0m,

    //            TotalOutstanding =
    //                (oldOutstanding.TryGetValue(c.Id, out var o1) ? o1 : 0m) +
    //                (currentOutstanding.TryGetValue(c.Id, out var o2) ? o2 : 0m),

    //            DueDayOfMonth = c.DueDayOfMonth,
    //            Status = c.Status
    //        };
    //    }).ToList();

    //    // -------------------------------------------------
    //    // RESULT
    //    // -------------------------------------------------
    //    return new SuccessDataResult<PagedResult<ContractListItemDto>>(
    //        new PagedResult<ContractListItemDto>
    //        {
    //            Items = items,
    //            Page = opt.Page,
    //            PageSize = opt.PageSize,
    //            TotalCount = totalCount
    //        }
    //    );
    //}


    //public IDataResult<List<ContractListItemDto>> GetListItems()
    //{
    //    // ----------------------------
    //    // BASE QUERIES - Hepsini memory'ye al
    //    // ----------------------------
    //    var contracts = _contractDal.QueryNoTracking().ToList();
    //    var shops = _shopDal.QueryNoTracking().ToList();
    //    var tenants = _tenantDal.QueryNoTracking().ToList();
    //    var markets = _marketDal.QueryNoTracking().ToList();
    //    var floors = _floorDal.QueryNoTracking().ToList();
    //    var accruals = _accrualDal.QueryNoTracking().ToList();
    //    var shares = _shopFranchiseShareDal.QueryNoTracking().ToList();
    //    var owners = _franchiseOwnerDal.QueryNoTracking().ToList();

    //    //var evalDate = DateOnly.FromDateTime(DateTime.UtcNow);
    //    var evalDate = _timeProvider.Today();          // DateOnly (Kabul, Gregorian)
    //    var nowUtc = _timeProvider.UtcNow();
    //    var (curY, curM, _) = _timeProvider.TodayPersian();

    //    // ----------------------------
    //    // AKTİF FRANCHISE SAHİPLERİ (memory içi)
    //    // ----------------------------
    //    var activeShares = shares
    //        .Where(s =>
    //            s.EffectiveFrom <= evalDate &&
    //            (s.EffectiveTo == null || evalDate <= s.EffectiveTo))
    //        .ToList();

    //    // Dükkan başına aktif imtiyaz sahibi sayısı
    //    var ownersCountPerShop = activeShares
    //        .GroupBy(s => s.ShopId)
    //        .ToDictionary(g => g.Key, g => g.Count());

    //    // Dükkan başına en yüksek yüzde
    //    var topPercentPerShop = activeShares
    //        .GroupBy(s => s.ShopId)
    //        .ToDictionary(
    //            g => g.Key,
    //            g => g.Max(x => x.SharePercent)  // ShopFranchiseShare.SharePercent decimal
    //        );

    //    // En yüksek paya sahip imtiyaz sahibinin adı
    //    var topOwnerPerShop = activeShares
    //        .GroupBy(s => s.ShopId)
    //        .ToDictionary(
    //            g => g.Key,
    //            g =>
    //            {
    //                if (!topPercentPerShop.TryGetValue(g.Key, out var topPct))
    //                    return (string?)null;

    //                var name = g
    //                    .Where(x => x.SharePercent == topPct)
    //                    .Join(
    //                        owners,
    //                        s => s.FranchiseOwnerId,
    //                        o => o.Id,
    //                        (s, o) => o.DisplayName)
    //                    .OrderBy(n => n)          // eşitse alfabetik ilk
    //                    .FirstOrDefault();

    //                return name;
    //            });

    //    // ----------------------------
    //    // ELEKTRİK (bu ay) - memory içi
    //    // ----------------------------
    //    var electricityThisPeriod = accruals
    //        .Where(a =>
    //            a.Kind == AllocationKind.Electricity &&
    //            a.Year == curY &&
    //            a.Month == curM)
    //        .GroupBy(a => a.ContractId)
    //        .ToDictionary(
    //            g => g.Key,
    //            g => g.Sum(z => z.Amount)   // Amount decimal, null olamaz
    //        );

    //    // BU DÖNEM (curY/curM) TAHAKKUK TOPLAMI (Amount)
    //    var currentPeriodAccrualTotals = accruals
    //        .Where(a => a.Year == curY && a.Month == curM)
    //        .GroupBy(a => a.ContractId)
    //        .ToDictionary(
    //            g => g.Key,
    //            g => g.Sum(a => a.Amount)
    //        );

    //    // BU DÖNEM ÖDENMEMİŞ BORÇ (Outstanding)
    //    var currentPeriodOutstandingTotals = accruals
    //        .Where(a =>
    //            a.Year == curY &&
    //            a.Month == curM &&
    //            a.Status != AccrualStatus.Settled)
    //        .GroupBy(a => a.ContractId)
    //        .ToDictionary(
    //            g => g.Key,
    //            g => g.Sum(a => a.OutstandingAmount)
    //        );

    //    // ESKİ DÖNEMLERDEN (curY/curM ÖNCESİ) DEVREDEN BORÇ
    //    var oldOutstandingTotals = accruals
    //        .Where(a =>
    //            a.Status != AccrualStatus.Settled &&
    //            (
    //                a.Year < curY ||
    //                (a.Year == curY && a.Month < curM)
    //            ))
    //        .GroupBy(a => a.ContractId)
    //        .ToDictionary(
    //            g => g.Key,
    //            g => g.Sum(a => a.OutstandingAmount)
    //        );


    //    // ----------------------------
    //    // ANA SORGU - memory join
    //    // ----------------------------
    //    var list =
    //        (from c in contracts
    //         join s in shops on c.ShopId equals s.Id
    //         join t in tenants on c.TenantId equals t.Id
    //         join m in markets on s.MarketId equals m.Id
    //         join fl in floors on s.FloorId equals fl.Id

    //         select new ContractListItemDto
    //         {
    //             ContractId = c.Id,
    //             ShopId = c.ShopId,
    //             TenantId = c.TenantId,
    //             MarketId = s.MarketId,
    //             FloorId = s.FloorId,

    //             MarketName = m.MarketName ?? string.Empty,
    //             FloorName = fl.FloorName ?? string.Empty,
    //             ShopName = s.Name ?? string.Empty,
    //             TenantName = t.DisplayName ?? string.Empty,

    //             // FRANCHISE
    //             FranchiseOwnersCount = ownersCountPerShop.TryGetValue(s.Id, out var cnt)
    //                 ? cnt
    //                 : 0,

    //             TopFranchiseOwnerName = topOwnerPerShop.TryGetValue(s.Id, out var topName)
    //                 ? topName
    //                 : null,

    //             FranchiseSummary = ownersCountPerShop.TryGetValue(s.Id, out var ownerCount) && ownerCount > 0
    //                 ? ownerCount == 1
    //                     ? (topOwnerPerShop.TryGetValue(s.Id, out var singleName) ? singleName : "")
    //                     : ((topOwnerPerShop.TryGetValue(s.Id, out var firstName) ? firstName : "") +
    //                        " + " + (ownerCount - 1) + " شخص دیگر...")
    //                 : string.Empty,

    //             FranchiseName = topOwnerPerShop.TryGetValue(s.Id, out var fn) ? fn : "",

    //             // TARİHLER
    //             ContractStartDate = c.StartDate,
    //             ContractEndDate = c.EndDate,

    //             // SABİT TUTARLAR
    //             OwnerPortionAmount = c.OwnerPortionAmount,
    //             MarketPortionAmount = c.MarketPortionAmount,
    //             FranchisePortionAmount = c.FranchisePortionAmount,
    //             TotalFixedRent =
    //                 c.OwnerPortionAmount +
    //                 c.MarketPortionAmount +
    //                 c.FranchisePortionAmount,

    //             BusinessActivity = c.BusinessActivity ?? "",

    //             // ELEKTRİK
    //             ElectricityThisPeriodTotal = electricityThisPeriod.TryGetValue(c.Id, out var elecTotal)
    //                 ? elecTotal
    //                 : (decimal?)null,

    //             // BORÇ SONRADAN HESAPLANACAK
    //             // BU DÖNEM TOPLAM TAHAKKUK (Owner+Market+Franchise+Electricity)
    //             CurrentPeriodAccrualTotal = currentPeriodAccrualTotals.TryGetValue(c.Id, out var accrualTotal)
    //                ? accrualTotal
    //                : 0m,

    //             // BU DÖNEM ÖDENMEMİŞ KISIM
    //             CurrentPeriodOutstanding = currentPeriodOutstandingTotals.TryGetValue(c.Id, out var curOut)
    //                ? curOut
    //                : 0m,

    //             // ESKİ DÖNEMLERDEN DEVREDEN BORÇ
    //             OldOutstanding = oldOutstandingTotals.TryGetValue(c.Id, out var oldOut)
    //                ? oldOut
    //                : 0m,

    //             // TOPLAM ÖDENMEMİŞ BORÇ
    //             TotalOutstanding =
    //                (oldOutstandingTotals.TryGetValue(c.Id, out var o1) ? o1 : 0m) +
    //                (currentPeriodOutstandingTotals.TryGetValue(c.Id, out var c1) ? c1 : 0m),

    //             // DİĞER
    //             DueDayOfMonth = c.DueDayOfMonth,
    //             Status = c.Status
    //         })
    //        .OrderBy(x => x.MarketName)
    //        .ThenBy(x => x.FloorName)
    //        .ThenBy(x => x.ShopName)
    //        .ToList();

    //    // ----------------------------
    //    // ESKİ BORÇ (client side)
    //    // ----------------------------
    //    foreach (var item in list)
    //    {
    //        var cutoff = new DateTime(
    //            item.ContractStartDate.Year,
    //            item.ContractStartDate.Month,
    //            item.ContractStartDate.Day);

    //        item.OldDebtTotal = accruals
    //            .Where(a =>
    //                a.ContractId == item.ContractId &&
    //                a.Status != AccrualStatus.Settled
    //                //&& a.CreatedAtUtc < cutoff
    //                )
    //            .Sum(a => a.OutstandingAmount);  // OutstandingAmount decimal
    //    }

    //    return new SuccessDataResult<List<ContractListItemDto>>(list);
    //}

}
}
