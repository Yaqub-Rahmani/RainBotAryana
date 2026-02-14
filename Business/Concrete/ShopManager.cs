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
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Extensions;
using DataAccess.Concrete.EntityFramework;
using Entities.Dtos.shop;
using Microsoft.Data.SqlClient;
using System.Diagnostics.Contracts;
using Core.Utilities.KabulDateTime;

namespace Business.Concrete
{
    public class ShopManager : IShopService
    {
        private readonly IShopFranchiseShareDal _shareDal;
        private readonly IContractDal _contractDal;
        private readonly IShopDal _shopDal;
        private readonly IFloorDal _floorDal;
        private readonly IMarketDal _marketDal;
        private readonly ITenantDal _tenantDal;
        private readonly ITimeProvider _time;

        public ShopManager(
            ITimeProvider time,
            IShopFranchiseShareDal shareDal,
            IContractDal contractDal,
            IShopDal shopDal,
            IFloorDal floorDal,
            IMarketDal marketDal,
            ITenantDal tenantDal)
        {
            _shareDal = shareDal;
            _contractDal = contractDal;
            _shopDal = shopDal;
            _floorDal = floorDal;
            _marketDal = marketDal;
            _tenantDal = tenantDal;
            _time = time;
        }

        private static ShopDetailDto Project(Shop s)
            => new ShopDetailDto
            {
                Id = s.Id,
                Name = s.Name,
                UnitNumber = s.UnitNumber,
                IsActive = s.IsActive,
                MarketId = s.MarketId,
                MarketName = s.Market?.MarketName ?? "",
                FloorId = s.FloorId,
                FloorName = s.Floor?.FloorName ?? "",
                FranchiseShareCount = s.ShopFranchiseShares?.Count ?? 0,
                ContractCount = s.Contracts?.Count ?? 0
            };

        [CacheAspect(10)]
        public IDataResult<Shop> GetById(Guid id)
        {
            var e = _shopDal.QueryNoTracking()
                .Include(x => x.Market)
                .Include(x => x.Floor)
                .Include(x => x.ShopFranchiseShares)
                .Include(x => x.Contracts)
                .FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<Shop>(null, "یافت نشد.") : new SuccessDataResult<Shop>(e);
        }

        [CacheAspect(5)] // istersen aç
        public async Task<IDataResult<List<ShopShareSummaryDto>>> GetShopsWithSharesAsync(Guid ownerId)
        {
            if (ownerId == Guid.Empty)
                return new ErrorDataResult<List<ShopShareSummaryDto>>(null, "ownerId is required.");

            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                // 1) DB'den tek geçişte ham değerleri çek
                var raw = await _shopDal
                    .QueryNoTracking()
                    .Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.UnitNumber,
                        s.IsActive,
                        s.MarketId,
                        s.FloorId,

                        Owners = s.ShopFranchiseShares
                            .Where(x =>
                                x.EffectiveFrom <= today &&
                                (x.EffectiveTo == null || x.EffectiveTo >= today)
                            )
                            .Select(x => x.FranchiseOwnerId)
                            .Distinct()
                            .Count(),

                                        Filled = (decimal?)s.ShopFranchiseShares
                            .Where(x =>
                                x.EffectiveFrom <= today &&
                                (x.EffectiveTo == null || x.EffectiveTo >= today)
                            )
                            .Sum(x => (decimal?)x.SharePercent) ?? 0m,

                                        Mine = (decimal?)s.ShopFranchiseShares
                            .Where(x =>
                                x.FranchiseOwnerId == ownerId &&
                                x.EffectiveFrom <= today &&
                                (x.EffectiveTo == null || x.EffectiveTo >= today)
                            )
                            .Sum(x => (decimal?)x.SharePercent) ?? 0m

                        //// Kaç farklı sahip?
                        //Owners = s.ShopFranchiseShares
                        //    .Select(x => x.FranchiseOwnerId)
                        //    .Distinct()
                        //    .Count(),

                        //// Toplam doluluk (boş koleksiyonda 0 döner)
                        //Filled = (decimal?)s.ShopFranchiseShares
                        //    .Sum(x => (decimal?)x.SharePercent) ?? 0m,

                        //// Benim payım
                        //Mine = (decimal?)s.ShopFranchiseShares
                        //    .Where(x => x.FranchiseOwnerId == ownerId)
                        //    .Sum(x => (decimal?)x.SharePercent) ?? 0m
                    })
                    .ToListAsync();

                // 2) Materialize sonrası clamp/round ve PercentOpen hesapla
                var list = raw.Select(x =>
                {
                    var filled = x.Filled;
                    if (filled < 0m) filled = 0m;
                    else if (filled > 100m) filled = 100m;

                    var mine = x.Mine;
                    if (mine < 0m) mine = 0m;
                    else if (mine > 100m) mine = 100m;

                    var open = 100m - filled;
                    if (open < 0m) open = 0m;          // teorik aşım durumuna karşı
                    else if (open > 100m) open = 100m; // ekstrem tutarsızlıklara karşı

                    return new ShopShareSummaryDto
                    {
                        ShopId = x.Id,
                        ShopName = x.Name,
                        MarketId = x.MarketId,
                        FloorId = x.FloorId,
                        UnitNumber = x.UnitNumber,
                        IsActive = x.IsActive,

                        OwnersCount = x.Owners,
                        PercentFilled = Math.Round(filled, 2),
                        PercentOpen = Math.Round(open, 2),
                        PercentMy = Math.Round(mine, 2)
                    };
                })
                    //.Where(dto => !(dto.PercentFilled == 100m && dto.PercentOpen == 0m))
                    .ToList();



                return new SuccessDataResult<List<ShopShareSummaryDto>>(list);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<ShopShareSummaryDto>>(null, ex.Message);
            }
        }

        [SecuredOperation("shop.write,admin")]
        [ValidationAspect(typeof(ShopValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IShopService.Get")]
        public IResult Add(ShopCreateDto dto, int userId)
        {
            var nowUtc = _time.UtcNow();

            var entity = new Shop
            {
                Id = Guid.NewGuid(),

                MarketId = dto.MarketId,
                FloorId = dto.FloorId,

                // Senin DTO’daki ShopCode -> entity’de UnitNumber
                UnitNumber = "",
                Name = dto.Name,
                AreaM2 = dto.AreaM2,
                IsActive = true,
                CreatedByUserId = userId,
                CreatedAtUtc = nowUtc
            };

            var check = EnsureUniqueNamePerMarketAndFloor(entity);
            if (!check.Success)
                return check;

            _shopDal.Add(entity);
            return new SuccessResult("ثبت شد.");
        }

        private IResult EnsureUniqueNamePerMarketAndFloor(Shop entity)
        {
            var normalized = (entity.Name ?? "").Trim().ToLower();

            var exists = _shopDal.QueryNoTracking().Any(x =>
                x.MarketId == entity.MarketId &&
                x.FloorId == entity.FloorId &&
                x.Id != entity.Id &&                // Update için kendisi hariç
                x.Name != null &&
                x.Name.Trim().ToLower() == normalized
            );

            return exists
                ? new ErrorResult("در همان مارکیت و همان منزل، دکانی به همین نام قبلاً موجود است.")
                : new SuccessResult();
        }

        [SecuredOperation("shop.write,admin")]
        [ValidationAspect(typeof(ShopValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IShopService.Get")]
        public IResult Update(Shop entity)
        {
            var e = _shopDal.Get(x => x.Id == entity.Id);
            if (e == null) return new ErrorResult("یافت نشد.");

            // Hedef değerlerle kontrol (isim/market/kat değişiyor olabilir)
            var check = EnsureUniqueNamePerMarketAndFloor(entity);
            if (!check.Success) return check;

            e.MarketId = entity.MarketId;
            e.FloorId = entity.FloorId;
            e.Name = entity.Name;
            e.UnitNumber = entity.UnitNumber;
            e.IsActive = entity.IsActive;

            _shopDal.Update(e);
            return new SuccessResult("بروز شد.");
        }
        
        [SecuredOperation("shop.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IShopService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _shopDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _shopDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }


        public async Task<IDataResult<List<FranchiseOwnerShopsDto>>> GetFranchiseOwnerShops(Guid ownerId)
        {
            if (ownerId == Guid.Empty)
                return new ErrorDataResult<List<FranchiseOwnerShopsDto>>(null, "ownerId is required.");

            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                var ownerSharesAtDate =
                    from sfs in _shareDal.QueryNoTracking()
                    where sfs.FranchiseOwnerId == ownerId
                       && sfs.EffectiveFrom <= today
                       && (sfs.EffectiveTo == null || sfs.EffectiveTo >= today)
                    group sfs by sfs.ShopId into g
                    select new
                    {
                        ShopId = g.Key,
                        MySharePercent = g.Sum(x => (decimal?)x.SharePercent) ?? 0m
                    };

                var projected = await (
                    from osh in ownerSharesAtDate
                    join sh in _shopDal.QueryNoTracking() on osh.ShopId equals sh.Id

                    join f in _floorDal.QueryNoTracking() on sh.FloorId equals f.Id into fgrp
                    from f in fgrp.DefaultIfEmpty()

                    join m in _marketDal.QueryNoTracking() on sh.MarketId equals m.Id into mgrp
                    from m in mgrp.DefaultIfEmpty()

                    from c in _contractDal.QueryNoTracking()
                        .Where(x => x.ShopId == osh.ShopId
                                    && x.Status == ContractStatus.Active
                                    && x.StartDate <= today
                                    && (x.EndDate == null || x.EndDate >= today))
                        .OrderByDescending(x => x.SignedOn)
                        .Take(1)
                        .DefaultIfEmpty()

                        // ✅ null-safe: c null ise tenantId null
                    let tenantId = c != null ? (Guid?)c.TenantId : null

                    join t in _tenantDal.QueryNoTracking() on tenantId equals (Guid?)t.Id into tgrp
                    from t in tgrp.DefaultIfEmpty()

                    select new
                    {
                        OwnerId = ownerId,
                        osh.ShopId,
                        ShopName = sh.Name,

                        FloorId = (Guid?)(f != null ? f.Id : null),
                        FloorName = f != null ? f.FloorName : null,

                        MarketId = (Guid?)(m != null ? m.Id : null),
                        MarketName = m != null ? m.MarketName : null,

                        ContractId = (Guid?)(c != null ? c.Id : null),
                        SignedOn = (DateOnly?)(c != null ? c.SignedOn : null),
                        StartDate = (DateOnly?)(c != null ? c.StartDate : null),
                        EndDate = (DateOnly?)(c != null ? c.EndDate : null),
                        DueDay = (int?)(c != null ? c.DueDayOfMonth : null),
                        Portion = (decimal?)(c != null ? c.FranchisePortionAmount : null),
                        StatusInt = c != null ? (int?)c.Status : null,

                        TenantId = (Guid?)(t != null ? t.Id : null),
                        TenantName = t != null ? (t.DisplayName + " - " + t.FatherName) : null,

                        MySharePercent = osh.MySharePercent
                    }
                ).ToListAsync();

                string F(DateOnly? d) => d.HasValue ? d.Value.ToString("yyyy-MM-dd") : "--";
                decimal Clamp01(decimal v) => v < 0m ? 0m : (v > 100m ? 100m : v);

                var list = projected.Select(x => new FranchiseOwnerShopsDto
                {
                    OwnerId = x.OwnerId,
                    ShopId = x.ShopId,

                    FloorId = x.FloorId ?? Guid.Empty,
                    MarketId = x.MarketId ?? Guid.Empty,

                    TenantId = x.TenantId ?? Guid.Empty,
                    ContractId = x.ContractId ?? Guid.Empty,

                    TenantName = x.TenantName,
                    ShopName = x.ShopName,
                    FloorName = x.FloorName,
                    MarketName = x.MarketName,

                    ContractStartDate = F(x.StartDate),
                    ContractEndDate = F(x.EndDate),
                    ContractSignedOn = F(x.SignedOn),

                    DueDayOfMonth = x.DueDay ?? 0,
                    TotalFranchisePortionAmount = x.Portion ?? 0m,
                    MyPortionAmount = x.Portion.HasValue ? (x.Portion.Value * Clamp01(x.MySharePercent)) / 100m : 0m,

                    Status = x.StatusInt.HasValue ? (ContractStatus)x.StatusInt.Value : ContractStatus.Archived,
                    SharePercent = Math.Round(Clamp01(x.MySharePercent), 2)
                }).ToList();

                return new SuccessDataResult<List<FranchiseOwnerShopsDto>>(list);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<FranchiseOwnerShopsDto>>(null, ex.Message);
            }
        }


        public async Task<IDataResult<List<ShopPickerDto>>> GetAvailableShopsAsync()
        {
            var today = _time.Today();

            var busyShopIdsQuery =
                _contractDal.QueryNoTracking()
                    .Where(c => c.Status == ContractStatus.Active
                                && c.StartDate <= today
                                && (c.EndDate == null || c.EndDate >= today))
                    .Select(c => c.ShopId)
                    .Distinct();

            var list = await _shopDal.QueryNoTracking()
                .Where(s => s.IsActive && !busyShopIdsQuery.Contains(s.Id))
                .OrderBy(s => s.UnitNumber)
                .ThenBy(s => s.Name)
                .Select(s => new ShopPickerDto
                {
                    Id = s.Id,
                    UnitNumber = s.UnitNumber,
                    Name = s.Name,
                    MarketId = s.MarketId,
                    FloorId = s.FloorId,
                    HasActiveFranchiseOwner =
                        s.ShopFranchiseShares.Any(sh =>
                            sh.EffectiveFrom <= today &&
                            (sh.EffectiveTo == null || sh.EffectiveTo >= today))
                })
                .ToListAsync();

            return new SuccessDataResult<List<ShopPickerDto>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<List<ShopDto>> GetShopListView()
        {
            var today = _time.Today();   // DateOnly

            // Not: IShopDal.QueryWithIncludes içinde:
            // Market, Floor, Contracts.Tenant, ShopFranchiseShares.FranchiseOwner include edilmiş olmalı.
            var shops = _shopDal.QueryWithIncludes().ToList();

            var list = shops.Select(shop =>
            {
                // Aktif sözleşme (bugün için)
                var activeContract = shop.Contracts
                    .FirstOrDefault(c =>
                        c.Status == ContractStatus.Active &&
                        c.StartDate <= today &&
                        c.EndDate >= today);

                var hasActiveContract = activeContract != null;
                var hasTenant = hasActiveContract; // senin kuralın

                string? tenantName = null;
                string? businessActivity = null;
                DateOnly? contractEndDate = null;
                int? contractRemainingDays = null;

                if (hasActiveContract)
                {
                    tenantName = activeContract!.Tenant?.DisplayName;
                    businessActivity = activeContract!.BusinessActivity;
                    contractEndDate = activeContract!.EndDate;

                    var endDateTime = contractEndDate.Value.ToDateTime(TimeOnly.MinValue);
                    var diffDays = (endDateTime - _time.UtcNow()).TotalDays;
                    contractRemainingDays = (int)Math.Floor(diffDays);
                }

                // İmtiyaz sahipleri (bugün için geçerli paylar)
                var franchiseSharesToday = (shop.ShopFranchiseShares ?? new List<ShopFranchiseShare>())
                    .Where(fs =>
                        fs.EffectiveFrom <= today &&
                        (fs.EffectiveTo == null || fs.EffectiveTo >= today))
                    .ToList();

                var hasFranchiseOwner = franchiseSharesToday.Any();

                string? franchiseOwnerSummary = hasFranchiseOwner
                    ? string.Join("، ",
                        franchiseSharesToday.Select(fs =>
                            $"{fs.FranchiseOwner.DisplayName} (%{fs.SharePercent:0.#})"))
                    : null;

                return new ShopDto
                {
                    Id = shop.Id,
                    Name = shop.Name,

                    MarketId = shop.MarketId,
                    MarketName = shop.Market.MarketName,

                    FloorId = shop.FloorId,
                    FloorName = shop.Floor.FloorName,

                    HasActiveContract = hasActiveContract,
                    HasTenant = hasTenant,
                    TenantName = tenantName,
                    BusinessActivity = businessActivity,

                    HasFranchiseOwner = hasFranchiseOwner,
                    FranchiseOwnerSummary = franchiseOwnerSummary,

                    // Shop entity'nde AreaM2 yok; eklediğinde burayı doldurursun.
                    AreaM2 = shop.AreaM2,

                    ContractEndDate = contractEndDate,
                    ContractRemainingDays = contractRemainingDays,

                    IsActive = shop.IsActive
                };
            })
            .ToList();

            return new SuccessDataResult<List<ShopDto>>(list);
        }



        public async Task<IDataResult<PagedResult<ShopShareSummaryDto>>> GetMyShopsPagedAsync(Guid ownerId, QueryOptions opt)
        {
            if (ownerId == Guid.Empty)
                return new ErrorDataResult<PagedResult<ShopShareSummaryDto>>(null, "ownerId is required.");

            opt ??= new QueryOptions();
            var page = opt.Page <= 0 ? 1 : opt.Page;
            var pageSize = opt.PageSize <= 0 ? 25 : opt.PageSize;

            var today = _time.Today();

            var q = BuildShopShareSummaryQuery(ownerId, today)
                .Where(x => (x.PercentMy ?? 0m) > 0m);

            if (opt.Filters?.Count > 0) q = q.ApplyFilters(opt.Filters);
            if (!string.IsNullOrWhiteSpace(opt.Search)) q = q.ApplySearch(opt.Search, null);
            if (!string.IsNullOrWhiteSpace(opt.Sort)) q = q.ApplySort(opt.Sort);

            var total = await q.CountAsync();
            var items = await q.ApplyPaging(page, pageSize).ToListAsync();

            return new SuccessDataResult<PagedResult<ShopShareSummaryDto>>(new PagedResult<ShopShareSummaryDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            });
        }

        public async Task<IDataResult<PagedResult<ShopShareSummaryDto>>> GetAvailableShopsPagedAsync(Guid ownerId, QueryOptions opt)
        {
            if (ownerId == Guid.Empty)
                return new ErrorDataResult<PagedResult<ShopShareSummaryDto>>(null, "ownerId is required.");

            opt ??= new QueryOptions();
            var page = opt.Page <= 0 ? 1 : opt.Page;
            var pageSize = opt.PageSize <= 0 ? 25 : opt.PageSize;

            var today = _time.Today();

            var q = BuildShopShareSummaryQuery(ownerId, today)
                .Where(x => (x.PercentMy ?? 0m) <= 0m && (x.PercentOpen ?? 0m) > 0m);

            if (opt.Filters?.Count > 0) q = q.ApplyFilters(opt.Filters);
            if (!string.IsNullOrWhiteSpace(opt.Search)) q = q.ApplySearch(opt.Search, null);
            if (!string.IsNullOrWhiteSpace(opt.Sort)) q = q.ApplySort(opt.Sort);

            var total = await q.CountAsync();
            var items = await q.ApplyPaging(page, pageSize).ToListAsync();

            return new SuccessDataResult<PagedResult<ShopShareSummaryDto>>(new PagedResult<ShopShareSummaryDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            });
        }

        private IQueryable<ShopShareSummaryDto> BuildShopShareSummaryQuery(Guid ownerId, DateOnly today)
        {
            return _shopDal
                .QueryNoTracking()
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.UnitNumber,
                    s.IsActive,
                    s.MarketId,
                    s.FloorId,

                    Owners = s.ShopFranchiseShares
                        .Where(x => x.EffectiveFrom <= today && (x.EffectiveTo == null || x.EffectiveTo >= today))
                        .Select(x => x.FranchiseOwnerId)
                        .Distinct()
                        .Count(),

                    Filled = (decimal?)s.ShopFranchiseShares
                        .Where(x => x.EffectiveFrom <= today && (x.EffectiveTo == null || x.EffectiveTo >= today))
                        .Sum(x => (decimal?)x.SharePercent) ?? 0m,

                    Mine = (decimal?)s.ShopFranchiseShares
                        .Where(x => x.FranchiseOwnerId == ownerId && x.EffectiveFrom <= today && (x.EffectiveTo == null || x.EffectiveTo >= today))
                        .Sum(x => (decimal?)x.SharePercent) ?? 0m
                })
                .Select(x => new ShopShareSummaryDto
                {
                    ShopId = x.Id,
                    ShopName = x.Name,
                    MarketId = x.MarketId,
                    FloorId = x.FloorId,
                    UnitNumber = x.UnitNumber,
                    IsActive = x.IsActive,

                    OwnersCount = x.Owners,
                    PercentFilled = x.Filled < 0m ? 0m : (x.Filled > 100m ? 100m : x.Filled),
                    PercentMy = x.Mine < 0m ? 0m : (x.Mine > 100m ? 100m : x.Mine),
                    PercentOpen = 100m - (x.Filled < 0m ? 0m : (x.Filled > 100m ? 100m : x.Filled))
                });
        }

    }
}
