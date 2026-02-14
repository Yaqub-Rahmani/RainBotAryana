using Business.Abstract;
using Core.Aspects.Autofac.Caching;
using Core.Extensions;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.Dashboard;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    public class DashboardManager : IDashboardService
    {
        private readonly IMoneyAccountDal _moneyAccountDal;
        private readonly IMoneyEntryDal _moneyEntryDal;
        private readonly IReserveAccountDal _reserveAccountDal;
        private readonly IReserveEntryDal _reserveEntryDal;
        private readonly IFamilyMemberDal _familyMemberDal;
        private readonly IFamilyBalanceEntryDal _familyBalanceDal;
        private readonly IFranchiseOwnerDal _franchiseOwnerDal;
        private readonly IFranchiseBalanceEntryDal _franchiseBalanceDal;
        private readonly ITaxPaymentEntryDal _taxDal;
        private readonly IZakatPaymentEntryDal _zakatDal;
        private readonly ICharityPaymentEntryDal _charityDal;
        private readonly ICollectionDal _collectionDal;
        private readonly IAccrualDal _accrualDal;
        private readonly ITenantDal _tenantDal;
        private readonly IDollarRateDal _dollarRateDal;
        private readonly ITimeProvider _timeProvider;

        public DashboardManager(
            IMoneyAccountDal moneyAccountDal,
            IMoneyEntryDal moneyEntryDal,
            IReserveAccountDal reserveAccountDal,
            IReserveEntryDal reserveEntryDal,
            IFamilyMemberDal familyMemberDal,
            IFamilyBalanceEntryDal familyBalanceDal,
            IFranchiseOwnerDal franchiseOwnerDal,
            IFranchiseBalanceEntryDal franchiseBalanceDal,
            ITaxPaymentEntryDal taxDal,
            IZakatPaymentEntryDal zakatDal,
            ICharityPaymentEntryDal charityDal,
            ICollectionDal collectionDal,
            IAccrualDal accrualDal,
            ITenantDal tenantDal,
            IDollarRateDal dollarRateDal,
            ITimeProvider timeProvider)
        {
            _moneyAccountDal = moneyAccountDal;
            _moneyEntryDal = moneyEntryDal;
            _reserveAccountDal = reserveAccountDal;
            _reserveEntryDal = reserveEntryDal;
            _familyMemberDal = familyMemberDal;
            _familyBalanceDal = familyBalanceDal;
            _franchiseOwnerDal = franchiseOwnerDal;
            _franchiseBalanceDal = franchiseBalanceDal;
            _taxDal = taxDal;
            _zakatDal = zakatDal;
            _charityDal = charityDal;
            _collectionDal = collectionDal;
            _accrualDal = accrualDal;
            _tenantDal = tenantDal;
            _dollarRateDal = dollarRateDal;
            _timeProvider = timeProvider;
        }

        // =========================
        // EXISTING (dokunma)
        // =========================
        [CacheAspect(1)]
        public IDataResult<FinanceDashboardDto> GetFinanceDashboard(DateTime? fromUtc = null, DateTime? toUtc = null, int take = 20)
        {
            var nowUtc = _timeProvider.UtcNow();
            var listFrom = fromUtc ?? nowUtc.AddDays(-60);
            var listTo = toUtc ?? nowUtc;
            take = take <= 0 ? 20 : Math.Min(take, 200);

            var dto = new FinanceDashboardDto
            {
                GeneratedAtUtc = nowUtc
            };

            // ---- Dollar rate (DB) ----
            var latestRate = _dollarRateDal.QueryNoTracking()
                .OrderByDescending(x => x.EffectiveFromUtc)
                .ThenByDescending(x => x.Id)
                .Select(x => new { x.EffectiveFromUtc, x.AfnPerUsd })
                .FirstOrDefault();

            if (latestRate != null)
            {
                dto.LatestAfnPerUsd = latestRate.AfnPerUsd;
                dto.LatestAfnPerUsdEffectiveFromUtc = latestRate.EffectiveFromUtc;
            }

            // ---- Money accounts (cash/bank + totals) ----
            var moneyAccounts = _moneyAccountDal.QueryNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new { x.Id, x.Name, x.AccountType, x.Currency })
                .ToList();

            var moneyEntryAgg = _moneyEntryDal.QueryNoTracking()
                .GroupBy(x => x.MoneyAccountId)
                .Select(g => new
                {
                    MoneyAccountId = g.Key,
                    TotalIn = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                    TotalOut = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                })
                .ToList();

            var entryAggMap = moneyEntryAgg.ToDictionary(x => x.MoneyAccountId, x => x);

            foreach (var acc in moneyAccounts)
            {
                entryAggMap.TryGetValue(acc.Id, out var agg);
                var totalIn = agg?.TotalIn ?? 0m;
                var totalOut = agg?.TotalOut ?? 0m;

                dto.CashBank.Accounts.Add(new MoneyAccountBalanceItemDto
                {
                    MoneyAccountId = acc.Id,
                    AccountName = acc.Name,
                    AccountType = acc.AccountType.ToString(),
                    Currency = acc.Currency.ToString(),
                    TotalIn = totalIn,
                    TotalOut = totalOut
                });

                var bal = totalIn - totalOut;

                if (acc.AccountType == MoneyAccountType.CashUsd) dto.CashBank.CashUsd = bal;
                if (acc.AccountType == MoneyAccountType.CashLocal) dto.CashBank.CashAfn = bal;
                if (acc.AccountType == MoneyAccountType.BankUsd) dto.CashBank.BankUsd = bal;
                if (acc.AccountType == MoneyAccountType.BankLocal) dto.CashBank.BankAfn = bal;
            }

            dto.Totals.TotalUsd = dto.CashBank.CashUsd + dto.CashBank.BankUsd;
            dto.Totals.TotalAfn = dto.CashBank.CashAfn + dto.CashBank.BankAfn;

            if (dto.LatestAfnPerUsd.HasValue && dto.LatestAfnPerUsd.Value > 0)
            {
                dto.Totals.TotalUsdInAfn = Math.Round(dto.Totals.TotalUsd * dto.LatestAfnPerUsd.Value, 2);
                dto.Totals.TotalAfnInUsd = Math.Round(dto.Totals.TotalAfn / dto.LatestAfnPerUsd.Value, 4);
            }

            // ---- Family pool ----
            var familyAgg = _familyBalanceDal.QueryNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.FamilyMemberId)
                .Select(g => new
                {
                    FamilyMemberId = g.Key,
                    TotalIn = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                    TotalOut = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                })
                .ToList();

            var familyNames = _familyMemberDal.QueryNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new { x.Id, x.DisplayName })
                .ToList();

            var familyNameMap = familyNames.ToDictionary(x => x.Id, x => x.DisplayName ?? "");
            dto.Breakdowns.FamilyMembers = familyAgg
                .Select(x => new FamilyBalanceSummaryDto
                {
                    FamilyMemberId = x.FamilyMemberId,
                    FamilyMemberName = familyNameMap.TryGetValue(x.FamilyMemberId, out var n) ? n : "",
                    TotalIn = x.TotalIn,
                    TotalOut = x.TotalOut
                })
                .OrderByDescending(x => x.Balance)
                .ToList();

            dto.Buckets.FamilyPoolAfn = dto.Breakdowns.FamilyMembers.Sum(x => x.Balance);

            // ---- Franchise pool ----
            var frAgg = _franchiseBalanceDal.QueryNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.FranchiseOwnerId)
                .Select(g => new
                {
                    FranchiseOwnerId = g.Key,
                    TotalIn = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                    TotalOut = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                })
                .ToList();

            var frNames = _franchiseOwnerDal.QueryNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new { x.Id, x.DisplayName })
                .ToList();

            var frNameMap = frNames.ToDictionary(x => x.Id, x => x.DisplayName ?? "");
            dto.Breakdowns.FranchiseOwners = frAgg
                .Select(x => new FranchiseBalanceSummaryDto
                {
                    FranchiseOwnerId = x.FranchiseOwnerId,
                    FranchiseOwnerName = frNameMap.TryGetValue(x.FranchiseOwnerId, out var n) ? n : "",
                    TotalIn = x.TotalIn,
                    TotalOut = x.TotalOut
                })
                .OrderByDescending(x => x.Balance)
                .ToList();

            dto.Buckets.FranchisePoolAfn = dto.Breakdowns.FranchiseOwners.Sum(x => x.Balance);

            // ---- Reserve accounts (Market/Electricity/Reserves) ----
            var reserveAccounts = _reserveAccountDal.QueryNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new { x.Id, x.Type, x.DisplayName })
                .ToList();

            var reserveAgg = _reserveEntryDal.QueryNoTracking()
                .GroupBy(x => x.ReserveAccountId)
                .Select(g => new
                {
                    ReserveAccountId = g.Key,
                    TotalIn = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                    TotalOut = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                })
                .ToList();

            var reserveAggMap = reserveAgg.ToDictionary(x => x.ReserveAccountId, x => x);
            dto.Breakdowns.ReserveAccounts = reserveAccounts
                .Select(a =>
                {
                    reserveAggMap.TryGetValue(a.Id, out var agg);
                    return new ReserveAccountBalanceDto
                    {
                        ReserveAccountId = a.Id,
                        AccountTitle = a.DisplayName ?? a.Type.ToString(),
                        AccountType = a.Type.ToString(),
                        TotalIn = agg?.TotalIn ?? 0m,
                        TotalOut = agg?.TotalOut ?? 0m
                    };
                })
                .OrderBy(x => x.AccountType)
                .ToList();

            // buckets (AFN)
            decimal ReserveBal(ReserveAccountType t)
            {
                var row = dto.Breakdowns.ReserveAccounts.FirstOrDefault(x => x.AccountType == t.ToString());
                return row?.Balance ?? 0m;
            }

            dto.Buckets.MarketMainAfn = ReserveBal(ReserveAccountType.MarketMain);
            dto.Buckets.ElectricityMainAfn = ReserveBal(ReserveAccountType.ElectricityMain);
            dto.Buckets.ReserveTotalAfn =
                ReserveBal(ReserveAccountType.MarketReserve) +
                ReserveBal(ReserveAccountType.ElectricityReserve) +
                ReserveBal(ReserveAccountType.OwnerReserve);

            // ---- Tax/Zakat/Charity pools (AFN) ----
            //dto.Buckets.TaxPoolAfn = _taxDal.QueryNoTracking()
            //    .Select(x => x.Direction == BalanceDirection.In ? x.Amount : -x.Amount)
            //    .DefaultIfEmpty(0m)
            //    .Sum();

            dto.Buckets.TaxPoolAfn = _taxDal.QueryNoTracking()
                .Select(x => x.Direction == BalanceDirection.In
                    ? (decimal?)x.Amount
                    : -(decimal?)x.Amount)
                .Sum() ?? 0m;


            //dto.Buckets.ZakatPoolAfn = _zakatDal.QueryNoTracking()
            //    .Select(x => x.Direction == BalanceDirection.In ? (decimal)x.Amount : -(decimal)x.Amount)
            //    .DefaultIfEmpty(0m)
            //    .Sum();

            //dto.Buckets.CharityPoolAfn = _charityDal.QueryNoTracking()
            //    .Select(x => x.Direction == BalanceDirection.In ? (decimal)x.Amount : -(decimal)x.Amount)
            //    .DefaultIfEmpty(0m)
            //    .Sum();

            dto.Buckets.ZakatPoolAfn = _zakatDal.QueryNoTracking()
                .Select(x => x.Direction == BalanceDirection.In
                    ? (decimal?)x.Amount
                    : -(decimal?)x.Amount)
                .Sum() ?? 0m;

            dto.Buckets.CharityPoolAfn = _charityDal.QueryNoTracking()
                .Select(x => x.Direction == BalanceDirection.In
                    ? (decimal?)x.Amount
                    : -(decimal?)x.Amount)
                .Sum() ?? 0m;


            if (dto.LatestAfnPerUsd.HasValue && dto.LatestAfnPerUsd.Value > 0)
            {
                var r = dto.LatestAfnPerUsd.Value;
                dto.Buckets.FamilyPoolUsd = Math.Round(dto.Buckets.FamilyPoolAfn / r, 4);
                dto.Buckets.FranchisePoolUsd = Math.Round(dto.Buckets.FranchisePoolAfn / r, 4);
                dto.Buckets.MarketMainUsd = Math.Round(dto.Buckets.MarketMainAfn / r, 4);
                dto.Buckets.ElectricityMainUsd = Math.Round(dto.Buckets.ElectricityMainAfn / r, 4);
                dto.Buckets.ReserveTotalUsd = Math.Round(dto.Buckets.ReserveTotalAfn / r, 4);
                dto.Buckets.TaxPoolUsd = Math.Round(dto.Buckets.TaxPoolAfn / r, 4);
                dto.Buckets.ZakatPoolUsd = Math.Round(dto.Buckets.ZakatPoolAfn / r, 4);
                dto.Buckets.CharityPoolUsd = Math.Round(dto.Buckets.CharityPoolAfn / r, 4);
            }

            // ---- Tabs: Collections (approved/pending) ----
            var collectionsQ = _collectionDal.QueryNoTracking()
                .Where(x => x.PaidAtUtc >= listFrom && x.PaidAtUtc <= listTo)
                .Include(x => x.Tenant)
                .Include(x => x.Shop).ThenInclude(s => s.Market)
                .Include(x => x.Shop).ThenInclude(s => s.Floor)
                .OrderByDescending(x => x.PaidAtUtc);

            var approved = collectionsQ.Where(x => x.IsApproved).Take(take).ToList();
            var pending = collectionsQ.Where(x => !x.IsApproved).Take(take).ToList();

            dto.Tabs.ApprovedCollections = approved.Select(MapCollection).ToList();
            dto.Tabs.PendingCollections = pending.Select(MapCollection).ToList();

            // ---- Tabs: Period debts (Accrual) ----
            // Son 12 ayı göster (takvim değil, kayıt bazlı en son 12 ay)
            var periodAgg = _accrualDal.QueryNoTracking()
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .GroupBy(x => new { x.TenantId, x.Year, x.Month })
                .Select(g => new
                {
                    g.Key.TenantId,
                    g.Key.Year,
                    g.Key.Month,
                    TotalAccrued = g.Sum(x => x.Amount),
                    TotalOutstanding = g.Sum(x => x.OutstandingAmount)
                })
                .Take(200)
                .ToList();

            var tenantIds = periodAgg.Select(x => x.TenantId).Distinct().ToList();
            var tenantNameMap = _tenantDal.QueryNoTracking()
                .Where(t => tenantIds.Contains(t.Id))
                .Select(t => new { t.Id, t.DisplayName })
                .ToDictionary(x => x.Id, x => x.DisplayName ?? "");

            dto.Tabs.PeriodDebts = periodAgg.Select(x => new PeriodDebtDashboardItemDto
            {
                TenantId = x.TenantId,
                TenantName = tenantNameMap.TryGetValue(x.TenantId, out var n) ? n : "",
                Year = x.Year,
                Month = x.Month,
                TotalAccrued = x.TotalAccrued,
                TotalOutstanding = x.TotalOutstanding
            }).ToList();

            return new SuccessDataResult<FinanceDashboardDto>(dto);
        }



        private static CollectionDashboardItemDto MapCollection(Collection c)
        {
            return new CollectionDashboardItemDto
            {
                Id = c.Id,
                PaidAtUtc = c.PaidAtUtc,
                TenantName = c.Tenant?.DisplayName ?? "",
                MarketName = c.Shop?.Market?.MarketName ?? "",
                FloorName = c.Shop?.Floor?.FloorName ?? "",
                ShopName = c.Shop?.Name ?? c.Shop?.Name ?? "",
                AmountReceived = c.AmountReceived,
                Method = c.Method.ToString(),
                ReferenceNo = c.ReferenceNo,
                IsApproved = c.IsApproved,
                ApprovedAtUtc = c.ApprovedAtUtc
            };
        }



        // =========================
        // NEW: Charts
        // =========================
        public IDataResult<DashboardChartsDto> GetCharts(DateTime? fromUtc = null, DateTime? toUtc = null, int months = 3)
        {
            months = months <= 0 ? 3 : Math.Min(months, 24);

            var now = _timeProvider.UtcNow();
            var start = fromUtc?.ToUniversalTime() ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(months - 1));
            start = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var end = toUtc?.ToUniversalTime() ?? now;
            var endExclusive = new DateTime(end.Year, end.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);

            // Collections grouped by month
            var colMap = _collectionDal.QueryNoTracking()
                .Where(x => x.PaidAtUtc >= start && x.PaidAtUtc < endExclusive)
                .GroupBy(x => new { x.PaidAtUtc.Year, x.PaidAtUtc.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Sum = g.Sum(z => z.AmountReceived) })
                .ToList()
                .ToDictionary(k => $"{k.Year:D4}-{k.Month:D2}", v => v.Sum);

            // Accruals grouped by CreatedAtUtc month
            var accrMap = _accrualDal.QueryNoTracking()
                .Where(x => x.CreatedAtUtc >= start && x.CreatedAtUtc < endExclusive)
                .GroupBy(x => new { x.CreatedAtUtc.Year, x.CreatedAtUtc.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Sum = g.Sum(z => z.Amount), Out = g.Sum(z => z.OutstandingAmount) })
                .ToList()
                .ToDictionary(k => $"{k.Year:D4}-{k.Month:D2}", v => new { v.Sum, v.Out });

            var items = new List<ChartMonthItemDto>();
            var cursor = start;
            var guard = 0;

            while (cursor < endExclusive && guard++ < 60)
            {
                var key = $"{cursor.Year:D4}-{cursor.Month:D2}";
                colMap.TryGetValue(key, out var collections);
                accrMap.TryGetValue(key, out var acc);

                items.Add(new ChartMonthItemDto
                {
                    Key = key,
                    Label = key,
                    CollectionsAfn = collections,
                    AccruedAfn = acc?.Sum ?? 0m,
                    OutstandingAfn = acc?.Out ?? 0m
                });

                cursor = cursor.AddMonths(1);
            }

            return new SuccessDataResult<DashboardChartsDto>(new DashboardChartsDto
            {
                Months = items.Count,
                Items = items.ToArray()
            });
        }

        // =========================
        // Helpers
        // =========================
        private static QueryOptions Normalize(QueryOptions opt)
        {
            opt ??= new QueryOptions();
            if (opt.Page <= 0) opt.Page = 1;
            if (opt.PageSize <= 0) opt.PageSize = 25;
            if (opt.PageSize > 500) opt.PageSize = 500;
            opt.Filters ??= new List<string>();
            opt.Includes ??= new List<string>();
            return opt;
        }

        private static PagedResult<T> ToPaged<T>(IQueryable<T> q, QueryOptions opt)
        {
            opt = Normalize(opt);
            var total = q.Count();
            var items = q.ApplyPaging(opt.Page, opt.PageSize).ToList();
            return new PagedResult<T>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            };
        }

        // =========================
        // NEW: Money Accounts (server-side)
        // =========================

        public IDataResult<PagedResult<MoneyAccountBalanceTableRowDto>> GetMoneyAccountsDetails(QueryOptions opt)
        {
            opt = Normalize(opt);

            var entries = _moneyEntryDal.QueryNoTracking();

            var q =
                from a in _moneyAccountDal.QueryNoTracking()
                join m in entries on a.Id equals m.MoneyAccountId into gj
                let totalIn = gj.Where(z => z.Direction == BalanceDirection.In)
                                 .Sum(z => (decimal?)z.Amount) ?? 0m
                let totalOut = gj.Where(z => z.Direction == BalanceDirection.Out)
                                 .Sum(z => (decimal?)z.Amount) ?? 0m
                select new MoneyAccountBalanceTableRowDto
                {
                    Id = a.Id,
                    AccountName = a.Name,
                    AccountType = a.AccountType.ToString(),
                    Currency = a.Currency.ToString(),
                    TotalIn = totalIn,
                    TotalOut = totalOut,
                    Balance = totalIn - totalOut,
                    IsActive = a.IsActive
                };

            q = q.ApplyFilters(opt.Filters);
            q = q.ApplySearch(opt.Search, new[]
            {
                nameof(MoneyAccountBalanceTableRowDto.AccountName),
                nameof(MoneyAccountBalanceTableRowDto.AccountType),
                nameof(MoneyAccountBalanceTableRowDto.Currency)
            });
            q = q.ApplySort(opt.Sort);

            return new SuccessDataResult<PagedResult<MoneyAccountBalanceTableRowDto>>(ToPaged(q, opt));
        }

        //public IDataResult<PagedResult<MoneyAccountBalanceTableRowDto>> GetMoneyAccountsDetails(QueryOptions opt)
        //{
        //    opt = Normalize(opt);

        //    var entryAgg = _moneyEntryDal.QueryNoTracking()
        //        .GroupBy(x => x.MoneyAccountId)
        //        .Select(g => new
        //        {
        //            MoneyAccountId = g.Key,
        //            TotalIn = g.Where(z => z.Direction == BalanceDirection.In).Sum(z => (decimal?)z.Amount) ?? 0m,
        //            TotalOut = g.Where(z => z.Direction == BalanceDirection.Out).Sum(z => (decimal?)z.Amount) ?? 0m
        //        });

        //    var q = from a in _moneyAccountDal.QueryNoTracking()
        //            join e in entryAgg on a.Id equals e.MoneyAccountId into ej
        //            from e in ej.DefaultIfEmpty()
        //            select new MoneyAccountBalanceTableRowDto
        //            {
        //                Id = a.Id,
        //                AccountName = a.Name,
        //                AccountType = a.AccountType.ToString(),
        //                Currency = a.Currency.ToString(),
        //                TotalIn = e == null ? 0m : e.TotalIn,
        //                TotalOut = e == null ? 0m : e.TotalOut,
        //                Balance = (e == null ? 0m : (e.TotalIn - e.TotalOut)),
        //                IsActive = a.IsActive
        //            };

        //    q = q.ApplyFilters(opt.Filters);
        //    q = q.ApplySearch(opt.Search, new[] { "AccountName", "AccountType", "Currency" });
        //    q = q.ApplySort(opt.Sort);

        //    return new SuccessDataResult<PagedResult<MoneyAccountBalanceTableRowDto>>(ToPaged(q, opt));
        //}

        // =========================
        // NEW: Reserve Accounts (server-side)
        // =========================
        //public IDataResult<PagedResult<ReserveAccountBalanceTableRowDto>> GetReserveAccountsDetails(QueryOptions opt)
        //{
        //    opt = Normalize(opt);

        //    var entryAgg = _reserveEntryDal.QueryNoTracking()
        //        .GroupBy(x => x.ReserveAccountId)
        //        .Select(g => new
        //        {
        //            ReserveAccountId = g.Key,
        //            TotalIn = g.Where(z => z.Direction == BalanceDirection.In).Sum(z => (decimal?)z.Amount) ?? 0m,
        //            TotalOut = g.Where(z => z.Direction == BalanceDirection.Out).Sum(z => (decimal?)z.Amount) ?? 0m
        //        });

        //    var q = from a in _reserveAccountDal.QueryNoTracking()
        //            join e in entryAgg on a.Id equals e.ReserveAccountId into ej
        //            from e in ej.DefaultIfEmpty()
        //            select new ReserveAccountBalanceTableRowDto
        //            {
        //                Id = a.Id,
        //                AccountType = a.Type.ToString(),
        //                DisplayName = a.DisplayName ?? "",
        //                TotalIn = e == null ? 0m : e.TotalIn,
        //                TotalOut = e == null ? 0m : e.TotalOut,
        //                Balance = (e == null ? 0m : (e.TotalIn - e.TotalOut)),
        //                IsActive = a.IsActive
        //            };

        //    q = q.ApplyFilters(opt.Filters);
        //    q = q.ApplySearch(opt.Search, new[] { "AccountType", "DisplayName" });
        //    q = q.ApplySort(opt.Sort);

        //    return new SuccessDataResult<PagedResult<ReserveAccountBalanceTableRowDto>>(ToPaged(q, opt));
        //}

        // =========================
        // NEW: Family Pool breakdown (server-side)
        // =========================
        public IDataResult<PagedResult<ReserveAccountBalanceTableRowDto>> GetReserveAccountsDetails(QueryOptions opt)
        {
            opt = Normalize(opt);

            // ---------- Reserve accounts (existing 5) ----------
            var entryAgg = _reserveEntryDal.QueryNoTracking()
                .GroupBy(x => x.ReserveAccountId)
                .Select(g => new
                {
                    ReserveAccountId = g.Key,
                    TotalIn = g.Where(z => z.Direction == BalanceDirection.In).Sum(z => (decimal?)z.Amount) ?? 0m,
                    TotalOut = g.Where(z => z.Direction == BalanceDirection.Out).Sum(z => (decimal?)z.Amount) ?? 0m
                });

            var baseQuery = from a in _reserveAccountDal.QueryNoTracking()
                            join e in entryAgg on a.Id equals e.ReserveAccountId into ej
                            from e in ej.DefaultIfEmpty()
                            select new ReserveAccountBalanceTableRowDto
                            {
                                Id = a.Id,
                                AccountType = a.Type.ToString(),
                                DisplayName = a.DisplayName ?? "",
                                TotalIn = e == null ? 0m : e.TotalIn,
                                TotalOut = e == null ? 0m : e.TotalOut,
                                Balance = (e == null ? 0m : (e.TotalIn - e.TotalOut)),
                                IsActive = a.IsActive
                            };

            var baseItems = baseQuery.ToList();

            // ---------- Add 3 pools into "حساب‌های ذخیره" list (5 -> 8) ----------
            var taxPoolId = Guid.Parse("C1F5E0A3-7A0F-4C5D-8D9D-7F85A2E7F5B1");
            var zakatPoolId = Guid.Parse("7B2E0D4F-1E6E-4A57-A6F7-0F1F8F6A9D2C");
            var charityPoolId = Guid.Parse("2A9F8B61-3D9E-4F7A-9C1B-5E0D8A7F4C11");

            var taxAgg = _taxDal.QueryNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    In = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                    Out = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                })
                .FirstOrDefault();

            var zakatAgg = _zakatDal.QueryNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    In = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                    Out = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                })
                .FirstOrDefault();

            var charityAgg = _charityDal.QueryNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    In = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                    Out = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                })
                .FirstOrDefault();

            var extra = new List<ReserveAccountBalanceTableRowDto>
                {
                    new ReserveAccountBalanceTableRowDto
                    {
                        Id = taxPoolId,
                        AccountType = "TaxPool",
                        DisplayName = "حساب مالیات",
                        TotalIn = taxAgg == null ? 0m : taxAgg.In,
                        TotalOut = taxAgg == null ? 0m : taxAgg.Out,
                        Balance = (taxAgg == null ? 0m : (taxAgg.In - taxAgg.Out)),
                        IsActive = true
                    },
                    new ReserveAccountBalanceTableRowDto
                    {
                        Id = zakatPoolId,
                        AccountType = "ZakatPool",
                        DisplayName = "حساب زکات",
                        TotalIn = zakatAgg == null ? 0m : zakatAgg.In,
                        TotalOut = zakatAgg == null ? 0m : zakatAgg.Out,
                        Balance = (zakatAgg == null ? 0m : (zakatAgg.In - zakatAgg.Out)),
                        IsActive = true
                    },
                    new ReserveAccountBalanceTableRowDto
                    {
                        Id = charityPoolId,
                        AccountType = "CharityPool",
                        DisplayName = "حساب صدقه",
                        TotalIn = charityAgg == null ? 0m : charityAgg.In,
                        TotalOut = charityAgg == null ? 0m : charityAgg.Out,
                        Balance = (charityAgg == null ? 0m : (charityAgg.In - charityAgg.Out)),
                        IsActive = true
                    },
                };

            var q = baseItems.Concat(extra).AsQueryable();

            q = q.ApplyFilters(opt.Filters);
            q = q.ApplySearch(opt.Search, new[] { "AccountType", "DisplayName" });
            q = q.ApplySort(opt.Sort);

            return new SuccessDataResult<PagedResult<ReserveAccountBalanceTableRowDto>>(ToPaged(q, opt));
        }



        public IDataResult<PagedResult<PoolPartyBalanceRowDto>> GetFamilyPoolDetails(QueryOptions opt)
        {
            opt = Normalize(opt);

            var agg = _familyBalanceDal.QueryNoTracking()
                .GroupBy(x => x.FamilyMemberId)
                .Select(g => new
                {
                    PartyId = g.Key,
                    InAfn = g.Where(z => z.Direction == BalanceDirection.In).Sum(z => (decimal?)z.Amount) ?? 0m,
                    OutAfn = g.Where(z => z.Direction == BalanceDirection.Out).Sum(z => (decimal?)z.Amount) ?? 0m
                });

            var q = from a in agg
                    join p in _familyMemberDal.QueryNoTracking() on a.PartyId equals p.Id
                    select new PoolPartyBalanceRowDto
                    {
                        Id = a.PartyId,
                        PartyId = a.PartyId,
                        PartyName = p.DisplayName,
                        TotalInAfn = a.InAfn,
                        TotalOutAfn = a.OutAfn,
                        BalanceAfn = a.InAfn - a.OutAfn,
                        DebtAfn = a.OutAfn > a.InAfn ? (a.OutAfn - a.InAfn) : 0m
                    };

            q = q.ApplyFilters(opt.Filters);
            q = q.ApplySearch(opt.Search, new[] { "PartyName" });
            q = q.ApplySort(opt.Sort);

            return new SuccessDataResult<PagedResult<PoolPartyBalanceRowDto>>(ToPaged(q, opt));
        }

        // =========================
        // NEW: Franchise Pool breakdown (server-side)
        // =========================
        public IDataResult<PagedResult<PoolPartyBalanceRowDto>> GetFranchisePoolDetails(QueryOptions opt)
        {
            opt = Normalize(opt);

            var agg = _franchiseBalanceDal.QueryNoTracking()
                .GroupBy(x => x.FranchiseOwnerId)
                .Select(g => new
                {
                    PartyId = g.Key,
                    InAfn = g.Where(z => z.Direction == BalanceDirection.In).Sum(z => (decimal?)z.Amount) ?? 0m,
                    OutAfn = g.Where(z => z.Direction == BalanceDirection.Out).Sum(z => (decimal?)z.Amount) ?? 0m
                });

            var q = from a in agg
                    join p in _franchiseOwnerDal.QueryNoTracking() on a.PartyId equals p.Id
                    select new PoolPartyBalanceRowDto
                    {
                        Id = a.PartyId,
                        PartyId = a.PartyId,
                        PartyName = p.DisplayName,
                        TotalInAfn = a.InAfn,
                        TotalOutAfn = a.OutAfn,
                        BalanceAfn = a.InAfn - a.OutAfn,
                        DebtAfn = a.OutAfn > a.InAfn ? (a.OutAfn - a.InAfn) : 0m
                    };

            q = q.ApplyFilters(opt.Filters);
            q = q.ApplySearch(opt.Search, new[] { "PartyName" });
            q = q.ApplySort(opt.Sort);

            return new SuccessDataResult<PagedResult<PoolPartyBalanceRowDto>>(ToPaged(q, opt));
        }

        // =========================
        // NEW: Collections (paid / pending approve) server-side
        // =========================
        public IDataResult<PagedResult<CollectionDashboardRowDto>> GetCollectionsDetails(QueryOptions opt, bool? isApproved = null)
        {
            opt = Normalize(opt);

            // Default sort: pending (not approved) first, then newest
            if (string.IsNullOrWhiteSpace(opt.Sort))
            {
                opt.Sort = isApproved.HasValue ? "-PaidAtUtc" : "+IsApproved,-PaidAtUtc";
            }

            var q = _collectionDal.QueryNoTracking();

            if (isApproved.HasValue)
                q = q.Where(x => x.IsApproved == isApproved.Value);

            var proj = q
                .Select(x => new CollectionDashboardRowDto
                {
                    Id = x.Id,
                    PaidAtUtc = x.PaidAtUtc,

                    TenantId = x.TenantId,
                    TenantName = x.Tenant.DisplayName,

                    ShopId = x.ShopId,
                    ShopName = x.Shop.Name,
                    ShopUnitNumber = x.Shop.UnitNumber,
                    MarketName = x.Shop.Market.MarketName,
                    FloorName = x.Shop.Floor.FloorName,

                    AmountReceived = x.AmountReceived,
                    Method = x.Method.ToString(),
                    ReferenceNo = x.ReferenceNo,

                    IsApproved = x.IsApproved,
                    ApprovedAtUtc = x.ApprovedAtUtc,

                    TargetYear = x.TargetYear,
                    TargetMonth = x.TargetMonth,
                    Notes = x.Notes
                });

            proj = proj.ApplyFilters(opt.Filters);
            proj = proj.ApplySearch(opt.Search, new[] { "TenantName", "ShopName", "ShopUnitNumber", "MarketName", "FloorName", "ReferenceNo", "Method" });
            proj = proj.ApplySort(opt.Sort);

            return new SuccessDataResult<PagedResult<CollectionDashboardRowDto>>(ToPaged(proj, opt));
        }

        // =========================
        // NEW: Period Debts (Accruals) server-side
        // =========================
        public IDataResult<PagedResult<PeriodDebtRowDto>> GetPeriodDebtsDetails(QueryOptions opt)
        {
            opt = Normalize(opt);

            var q = _accrualDal.QueryNoTracking()
                .Where(x => x.OutstandingAmount > 0m)
                .GroupBy(x => new { x.TenantId, x.Year, x.Month, x.Tenant.DisplayName })
                .Select(g => new PeriodDebtRowDto
                {
                    Id = (g.Key.TenantId.ToString("N") + "-" + g.Key.Year.ToString() + "-" + g.Key.Month.ToString()),
                    TenantId = g.Key.TenantId,
                    TenantName = g.Key.DisplayName,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAccrued = g.Sum(z => z.Amount),
                    TotalOutstanding = g.Sum(z => z.OutstandingAmount)
                });

            q = q.ApplyFilters(opt.Filters);
            q = q.ApplySearch(opt.Search, new[] { "TenantName" });
            q = q.ApplySort(opt.Sort);

            return new SuccessDataResult<PagedResult<PeriodDebtRowDto>>(ToPaged(q, opt));
        }


        public IDataResult<PagedResult<DashboardEntryDto>> GetEntriesDetails(
            QueryOptions opt,
            DashboardEntrySource source,
            Guid? sourceId,
            DateTime? fromUtc = null,
            DateTime? toUtc = null)
                {
                    opt ??= new QueryOptions();
                    if (string.IsNullOrWhiteSpace(opt.Sort)) opt.Sort = "-OccurredAtUtc";

                    IQueryable<DashboardEntryDto> q;

                    switch (source)
                    {
                        case DashboardEntrySource.ReserveAccount:
                            if (!sourceId.HasValue)
                                return new ErrorDataResult<PagedResult<DashboardEntryDto>>(null, "sourceId لازم است");

                            var r = _reserveEntryDal.QueryNoTracking()
                                .Where(x => x.ReserveAccountId == sourceId.Value);

                            if (fromUtc.HasValue) r = r.Where(x => x.OccurredAtUtc >= fromUtc.Value);
                            if (toUtc.HasValue) r = r.Where(x => x.OccurredAtUtc <= toUtc.Value);

                            q = r.Select(x => new DashboardEntryDto
                            {
                                Source = source.ToString(),
                                SourceId = sourceId,
                                Id = x.Id,
                                OccurredAtUtc = x.OccurredAtUtc,
                                Direction = x.Direction.ToString(),
                                Amount = x.Amount,
                                Notes = x.Description,
                                Method = x.PaymentMethod.ToString(),
                                Reference = x.PaymentReference ?? x.ExternalDocNo
                            });
                            break;

                        case DashboardEntrySource.TaxPool:
                            var t = _taxDal.QueryNoTracking();
                            if (fromUtc.HasValue) t = t.Where(x => x.OccurredAtUtc >= fromUtc.Value);
                            if (toUtc.HasValue) t = t.Where(x => x.OccurredAtUtc <= toUtc.Value);

                            q = t.Select(x => new DashboardEntryDto
                            {
                                Source = source.ToString(),
                                SourceId = null,
                                Id = x.Id,
                                OccurredAtUtc = x.OccurredAtUtc,
                                Direction = x.Direction.ToString(),
                                Amount = x.Amount,
                                Notes = x.Notes,
                                Method = x.PaymentMethod.ToString(),
                                Reference = x.PaymentReference
                            });
                            break;

                        case DashboardEntrySource.ZakatPool:
                            var z = _zakatDal.QueryNoTracking();
                            if (fromUtc.HasValue) z = z.Where(x => x.OccurredAtUtc >= fromUtc.Value);
                            if (toUtc.HasValue) z = z.Where(x => x.OccurredAtUtc <= toUtc.Value);

                            q = z.Select(x => new DashboardEntryDto
                            {
                                Source = source.ToString(),
                                SourceId = null,
                                Id = x.Id,
                                OccurredAtUtc = x.OccurredAtUtc,
                                Direction = x.Direction.ToString(),
                                Amount = x.Amount,
                                Notes = x.Notes,
                                Method = x.PaymentMethod.ToString(),
                                Reference = x.PaymentReference
                            });
                            break;

                        case DashboardEntrySource.CharityPool:
                            var c = _charityDal.QueryNoTracking();
                            if (fromUtc.HasValue) c = c.Where(x => x.OccurredAtUtc >= fromUtc.Value);
                            if (toUtc.HasValue) c = c.Where(x => x.OccurredAtUtc <= toUtc.Value);

                            q = c.Select(x => new DashboardEntryDto
                            {
                                Source = source.ToString(),
                                SourceId = null,
                                Id = x.Id,
                                OccurredAtUtc = x.OccurredAtUtc,
                                Direction = x.Direction.ToString(),
                                Amount = x.Amount,
                                Notes = x.Notes,
                                Method = x.PaymentMethod.ToString(),
                                Reference = x.PaymentReference
                            });
                            break;

                case DashboardEntrySource.MoneyAccount:
                    if (!sourceId.HasValue)
                        return new ErrorDataResult<PagedResult<DashboardEntryDto>>(null,"sourceId لازم است");

                    var m = _moneyEntryDal.QueryNoTracking()
                        .Where(x => x.MoneyAccountId == sourceId.Value);

                    if (fromUtc.HasValue) m = m.Where(x => x.OccurredAtUtc >= fromUtc.Value);
                    if (toUtc.HasValue) m = m.Where(x => x.OccurredAtUtc <= toUtc.Value);

                    q = from e in m
                        join a in _moneyAccountDal.QueryNoTracking() on e.MoneyAccountId equals a.Id
                        select new DashboardEntryDto
                        {
                            Source = source.ToString(),
                            SourceId = sourceId,
                            Id = e.Id,
                            OccurredAtUtc = e.OccurredAtUtc,
                            Direction = e.Direction.ToString(),
                            Amount = e.Amount,
                            Title = a.Name,
                            Currency = e.Currency.ToString(),
                            AfnPerUsdAtThatTime = e.AfnPerUsdAtThatTime,
                            UsdEquivalent = e.UsdEquivalent
                        };
                    break;

                case DashboardEntrySource.FamilyMember:
                    if (!sourceId.HasValue)
                        return new ErrorDataResult<PagedResult<DashboardEntryDto>>(null,"sourceId لازم است");

                    var f = _familyBalanceDal.QueryNoTracking()
                        .Where(x => x.FamilyMemberId == sourceId.Value);

                    if (fromUtc.HasValue) f = f.Where(x => x.OccurredAtUtc >= fromUtc.Value);
                    if (toUtc.HasValue) f = f.Where(x => x.OccurredAtUtc <= toUtc.Value);

                    q = from e in f
                        join p in _familyMemberDal.QueryNoTracking() on e.FamilyMemberId equals p.Id
                        select new DashboardEntryDto
                        {
                            Source = source.ToString(),
                            SourceId = sourceId,
                            Id = e.Id,
                            OccurredAtUtc = e.OccurredAtUtc,
                            Direction = e.Direction.ToString(),
                            Amount = e.Amount,
                            Title = p.DisplayName,
                            Notes = e.Notes,
                            Method = e.PaymentMethod.ToString(),
                            Reference = e.PaymentReference
                        };
                    break;

                case DashboardEntrySource.FranchiseOwner:
                    if (!sourceId.HasValue)
                        return new ErrorDataResult<PagedResult<DashboardEntryDto>>(null,"sourceId لازم است");

                    var fo = _franchiseBalanceDal.QueryNoTracking()
                        .Where(x => x.FranchiseOwnerId == sourceId.Value);

                    if (fromUtc.HasValue) fo = fo.Where(x => x.OccurredAtUtc >= fromUtc.Value);
                    if (toUtc.HasValue) fo = fo.Where(x => x.OccurredAtUtc <= toUtc.Value);

                    q = from e in fo
                        join p in _franchiseOwnerDal.QueryNoTracking() on e.FranchiseOwnerId equals p.Id
                        select new DashboardEntryDto
                        {
                            Source = source.ToString(),
                            SourceId = sourceId,
                            Id = e.Id,
                            OccurredAtUtc = e.OccurredAtUtc,
                            Direction = e.Direction.ToString(),
                            Amount = e.Amount,
                            Title = p.DisplayName,
                            Notes = e.Notes,
                            Method = e.PaymentMethod.ToString(),
                            Reference = e.PaymentReference
                        };
                    break;

                default:
                                    return new ErrorDataResult<PagedResult<DashboardEntryDto>>(null,"source پشتیبانی نمی‌شود");
                            }

                            q = q.ApplySearch(opt.Search, new[] { "Notes", "Method", "Reference", "Direction" });
                            q = q.ApplySort(opt.Sort);

                            return new SuccessDataResult<PagedResult<DashboardEntryDto>>(ToPaged(q, opt));
                        }

        

        //public IDataResult<PagedResult<DashboardEntryDto>> GetEntriesDetails(
        //QueryOptions opt,
        //DashboardEntrySource source,
        //Guid? sourceId,
        //DateTime? fromUtc,
        //DateTime? toUtc,
        //int? periodYear,
        //int? periodMonth)
        //{
        //    opt = Normalize(opt);

        //    if (string.IsNullOrWhiteSpace(opt.Sort))
        //        opt.Sort = "-OccurredAtUtc";

        //    IQueryable<DashboardEntryDto> q;

        //    switch (source)
        //    {
        //        case DashboardEntrySource.MoneyAccount:
        //            {
        //                if (!sourceId.HasValue) return new ErrorDataResult<PagedResult<DashboardEntryDto>>(null, "sourceId لازم است");

        //                var baseQ = _moneyEntryDal.QueryNoTracking().Where(x => x.MoneyAccountId == sourceId.Value);
        //                if (fromUtc.HasValue) baseQ = baseQ.Where(x => x.OccurredAtUtc >= fromUtc.Value);
        //                if (toUtc.HasValue) baseQ = baseQ.Where(x => x.OccurredAtUtc <= toUtc.Value);

        //                q =
        //                    from e in baseQ
        //                    join acc in _moneyAccountDal.QueryNoTracking() on e.MoneyAccountId equals acc.Id
        //                    join c in _collectionDal.QueryNoTracking() on e.CollectionId equals c.Id into cj
        //                    from c in cj.DefaultIfEmpty()
        //                    select new DashboardEntryDto
        //                    {
        //                        Source = source.ToString(),
        //                        SourceId = sourceId,

        //                        Id = e.Id,
        //                        OccurredAtUtc = e.OccurredAtUtc,
        //                        Direction = e.Direction.ToString(),
        //                        Amount = e.Amount,

        //                        Title = acc.Name,
        //                        Currency = e.Currency.ToString(),
        //                        AfnPerUsdAtThatTime = e.AfnPerUsdAtThatTime,
        //                        UsdEquivalent = e.UsdEquivalent,

        //                        CollectionId = e.CollectionId,

        //                        TenantId = c != null ? c.TenantId : null,
        //                        TenantName = c != null ? c.Tenant.DisplayName : null,
        //                        MarketName = c != null ? c.Shop.Market.MarketName : null,
        //                        FloorName = c != null ? c.Shop.Floor.FloorName : null,
        //                        ShopUnitNumber = c != null ? c.Shop.UnitNumber : null,

        //                        Method = c != null ? c.Method.ToString() : null,
        //                        Reference = c != null ? c.ReferenceNo : null,
        //                        Notes = c != null ? c.Notes : null
        //                    };
        //                break;
        //            }

        //        case DashboardEntrySource.FamilyMember:
        //            {
        //                if (!sourceId.HasValue) return new ErrorDataResult<PagedResult<DashboardEntryDto>>(null, "sourceId لازم است");

        //                var baseQ = _familyBalanceDal.QueryNoTracking()
        //                    .Where(x => x.IsActive && x.FamilyMemberId == sourceId.Value);

        //                if (fromUtc.HasValue) baseQ = baseQ.Where(x => x.OccurredAtUtc >= fromUtc.Value);
        //                if (toUtc.HasValue) baseQ = baseQ.Where(x => x.OccurredAtUtc <= toUtc.Value);

        //                q =
        //                    from e in baseQ
        //                    join p in _familyMemberDal.QueryNoTracking() on e.FamilyMemberId equals p.Id
        //                    join c in _collectionDal.QueryNoTracking() on e.CollectionId equals c.Id into cj
        //                    from c in cj.DefaultIfEmpty()
        //                    select new DashboardEntryDto
        //                    {
        //                        Source = source.ToString(),
        //                        SourceId = sourceId,

        //                        Id = e.Id,
        //                        OccurredAtUtc = e.OccurredAtUtc,
        //                        Direction = e.Direction.ToString(),
        //                        Amount = e.Amount,

        //                        Title = p.DisplayName,
        //                        Notes = e.Notes,
        //                        Method = e.PaymentMethod.ToString(),
        //                        Reference = e.PaymentReference,
        //                        PeriodYear = e.PeriodYear,
        //                        PeriodMonth = e.PeriodMonth,

        //                        CollectionId = e.CollectionId,

        //                        TenantId = c != null ? c.TenantId : null,
        //                        TenantName = c != null ? c.Tenant.DisplayName : null,
        //                        MarketName = c != null ? c.Shop.Market.MarketName : null,
        //                        FloorName = c != null ? c.Shop.Floor.FloorName : null,
        //                        ShopUnitNumber = c != null ? c.Shop.UnitNumber : null
        //                    };
        //                break;
        //            }

        //        case DashboardEntrySource.FranchiseOwner:
        //            {
        //                if (!sourceId.HasValue) return new ErrorDataResult<PagedResult<DashboardEntryDto>>(null, "sourceId لازم است");

        //                var baseQ = _franchiseBalanceDal.QueryNoTracking()
        //                    .Where(x => x.IsActive && x.FranchiseOwnerId == sourceId.Value);

        //                if (fromUtc.HasValue) baseQ = baseQ.Where(x => x.OccurredAtUtc >= fromUtc.Value);
        //                if (toUtc.HasValue) baseQ = baseQ.Where(x => x.OccurredAtUtc <= toUtc.Value);

        //                q =
        //                    from e in baseQ
        //                    join o in _franchiseOwnerDal.QueryNoTracking() on e.FranchiseOwnerId equals o.Id
        //                    join c in _collectionDal.QueryNoTracking() on e.CollectionId equals c.Id into cj
        //                    from c in cj.DefaultIfEmpty()
        //                    select new DashboardEntryDto
        //                    {
        //                        Source = source.ToString(),
        //                        SourceId = sourceId,

        //                        Id = e.Id,
        //                        OccurredAtUtc = e.OccurredAtUtc,
        //                        Direction = e.Direction.ToString(),
        //                        Amount = e.Amount,

        //                        Title = o.DisplayName,
        //                        Notes = e.Notes,
        //                        Method = e.PaymentMethod.ToString(),
        //                        Reference = e.PaymentReference,
        //                        PeriodYear = e.PeriodYear,
        //                        PeriodMonth = e.PeriodMonth,

        //                        CollectionId = e.CollectionId,

        //                        TenantId = c != null ? c.TenantId : null,
        //                        TenantName = c != null ? c.Tenant.DisplayName : null,

        //                        MarketName = e.Shop.Market.MarketName,
        //                        FloorName = e.Shop.Floor.FloorName,
        //                        ShopUnitNumber = e.Shop.UnitNumber
        //                    };
        //                break;
        //            }

        //        // ✅ Ödenen para -> hangi borçları kapattı (AccrualSettlement listesi)
        //        case DashboardEntrySource.Collection:
        //            {
        //                if (!sourceId.HasValue) return new ErrorDataResult<PagedResult<DashboardEntryDto>>(null, "sourceId لازم است");

        //                var coll = _collectionDal.QueryNoTracking().Where(c => c.Id == sourceId.Value);

        //                q =
        //                    from c in coll
        //                    from a in c.Allocations
        //                    from s in a.AccrualSettlements
        //                    select new DashboardEntryDto
        //                    {
        //                        Source = source.ToString(),
        //                        SourceId = sourceId,

        //                        Id = s.Id,
        //                        OccurredAtUtc = s.SettledAtUtc,
        //                        Direction = "Out",
        //                        Amount = s.Amount, // ✅ kapatılan miktar

        //                        Title = "بستن بدهی",
        //                        Notes = s.Notes,
        //                        Method = c.Method.ToString(),
        //                        Reference = c.ReferenceNo,

        //                        CollectionId = c.Id,

        //                        TenantId = c.TenantId,
        //                        TenantName = c.Tenant.DisplayName,
        //                        MarketName = c.Shop.Market.MarketName,
        //                        FloorName = c.Shop.Floor.FloorName,
        //                        ShopUnitNumber = c.Shop.UnitNumber,

        //                        AccrualId = s.AccrualId,
        //                        Kind = s.Accrual.Kind.ToString(),
        //                        PeriodYear = s.Accrual.Year,
        //                        PeriodMonth = s.Accrual.Month,
        //                        AccrualAmount = s.Accrual.Amount,
        //                        OutstandingAmount = s.Accrual.OutstandingAmount
        //                    };

        //                if (fromUtc.HasValue) q = q.Where(x => x.OccurredAtUtc >= fromUtc.Value);
        //                if (toUtc.HasValue) q = q.Where(x => x.OccurredAtUtc <= toUtc.Value);

        //                break;
        //            }

        //        // ✅ Borç tablosu -> borç nedeni (Kind) + detay satırları (Accrual)
        //        case DashboardEntrySource.AccrualDebt:
        //            {
        //                if (!sourceId.HasValue) return new ErrorDataResult<PagedResult<DashboardEntryDto>>(null, "sourceId لازم است");

        //                var a = _accrualDal.QueryNoTracking()
        //                    .Where(x => x.TenantId == sourceId.Value && x.OutstandingAmount > 0m);

        //                if (periodYear.HasValue) a = a.Where(x => x.Year == periodYear.Value);
        //                if (periodMonth.HasValue) a = a.Where(x => x.Month == periodMonth.Value);

        //                q = a.Select(x => new DashboardEntryDto
        //                {
        //                    Source = source.ToString(),
        //                    SourceId = sourceId,

        //                    Id = x.Id,
        //                    OccurredAtUtc = x.CreatedAtUtc,
        //                    Direction = "Debt",
        //                    Amount = x.Amount,

        //                    Notes = x.Notes,

        //                    TenantId = x.TenantId,
        //                    TenantName = x.Tenant.DisplayName,

        //                    MarketName = x.Shop.Market.MarketName,
        //                    FloorName = x.Shop.Floor.FloorName,
        //                    ShopUnitNumber = x.Shop.UnitNumber,

        //                    Kind = x.Kind.ToString(),
        //                    PeriodYear = x.Year,
        //                    PeriodMonth = x.Month,

        //                    AccrualId = x.Id,
        //                    AccrualAmount = x.Amount,
        //                    OutstandingAmount = x.OutstandingAmount
        //                });

        //                break;
        //            }

        //        default:
        //            return new ErrorDataResult<PagedResult<DashboardEntryDto>>(null, "source desteklenmiyor");
        //    }

        //    // filters/search/sort
        //    q = q.ApplyFilters(opt.Filters);
        //    q = q.ApplySearch(opt.Search, new[]
        //    {
        //            nameof(DashboardEntryDto.Title),
        //            nameof(DashboardEntryDto.TenantName),
        //            nameof(DashboardEntryDto.MarketName),
        //            nameof(DashboardEntryDto.FloorName),
        //            nameof(DashboardEntryDto.ShopUnitNumber),
        //            nameof(DashboardEntryDto.Kind),
        //            nameof(DashboardEntryDto.Method),
        //            nameof(DashboardEntryDto.Reference),
        //            nameof(DashboardEntryDto.Notes)
        //        });
        //    q = q.ApplySort(opt.Sort);

        //    return new SuccessDataResult<PagedResult<DashboardEntryDto>>(ToPaged(q, opt));
        //}



    }
}
