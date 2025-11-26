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
        //private readonly IShopDal _dal;

        //private readonly IShopFranchiseShareDal _shareDal;
        //private readonly IContractDal _contractDal;

        //public ShopManager(IShopDal dal, IShopFranchiseShareDal shareDal, IContractDal contractDal)
        //{
        //    _dal = dal;
        //    _shareDal = shareDal;
        //    _contractDal = contractDal;
        //}

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
        public IDataResult<List<Shop>> GetAll(bool? onlyActive = null, Guid? marketId = null, Guid? floorId = null)
        {
            var q = _shopDal.QueryNoTracking();
            if (onlyActive == true) q = q.Where(x => x.IsActive);
            if (marketId.HasValue) q = q.Where(x => x.MarketId == marketId.Value);
            if (floorId.HasValue) q = q.Where(x => x.FloorId == floorId.Value);

            return new SuccessDataResult<List<Shop>>(q.Include(x => x.Market).Include(x => x.Floor).OrderBy(x => x.Name).ToList());
        }

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


        [CacheAspect(10)]
        public IDataResult<PagedResult<ShopDetailDto>> GetDetails(QueryOptions opt, Guid? marketId = null, Guid? floorId = null, bool? onlyActive = null)
        {
            opt ??= new QueryOptions();

            // 1) Temel sorgu (Include YOK)
            var q = _shopDal.QueryNoTracking();

            if (onlyActive == true) q = q.Where(x => x.IsActive);
            if (marketId.HasValue) q = q.Where(x => x.MarketId == marketId.Value);
            if (floorId.HasValue) q = q.Where(x => x.FloorId == floorId.Value);

            q = q.ApplyFilters(opt.Filters);

            // 2) Arama (LIKE + null check)
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                var like = $"%{key}%";
                q = q.Where(x =>
                    (x.Name != null && EF.Functions.Like(x.Name, like)) ||
                    (x.UnitNumber != null && EF.Functions.Like(x.UnitNumber, like)) ||
                    EF.Functions.Like(x.Market.MarketName, like)
                );
            }

            // 3) Projeksiyon: DTO’yu doğrudan SQL’de kur
            var qDto = q.Select(s => new ShopDetailDto
            {
                Id = s.Id,
                Name = s.Name,
                UnitNumber = s.UnitNumber,
                IsActive = s.IsActive,
                MarketId = s.MarketId,
                MarketName = s.Market.MarketName,  // Include gerekmez
                FloorId = s.FloorId,
                FloorName = s.Floor.FloorName,     // Include gerekmez
                FranchiseShareCount = s.ShopFranchiseShares.Count(), // SQL COUNT(*)
                ContractCount = s.Contracts.Count()
            });

            // 4) Sıralama (varsayılan ekle)
            qDto = qDto.ApplySort(opt.Sort);
            qDto = qDto.OrderBy(x => x.Name ?? x.UnitNumber); // her durumda deterministik

            // 5) Sayfalama + total
            var total = qDto.Count();
            var items = qDto
                .Skip((opt.Page - 1) * opt.PageSize)
                .Take(opt.PageSize)
                .ToList();

            return new SuccessDataResult<PagedResult<ShopDetailDto>>(new PagedResult<ShopDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            });
        }

        [CacheAspect(10)]
        public IDataResult<ShopDetailDto> GetDetailById(Guid id)
        {
            var e = _shopDal.QueryNoTracking()
                .Include(x => x.Market).Include(x => x.Floor)
                .Include(x => x.ShopFranchiseShares).Include(x => x.Contracts)
                .FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<ShopDetailDto>(null, "یافت نشد.") : new SuccessDataResult<ShopDetailDto>(Project(e));
        }

        [SecuredOperation("shop.write,admin")]
        [ValidationAspect(typeof(ShopValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IShopService.Get")]
        public IResult Add(Shop entity)
        {
            var check = EnsureUniqueNamePerMarketAndFloor(entity);
            if (!check.Success) return check;

            _shopDal.Add(entity);
            return new SuccessResult("ثبت شد.");
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

        private IResult EnsureUniqueNamePerMarketAndFloor(Shop entity)
        {
            var normalized = (entity.Name ?? "").Trim().ToLower();

            var exists = _shopDal.QueryNoTracking().Any(x =>
                x.MarketId == entity.MarketId &&
                x.FloorId == entity.FloorId &&
                x.Id != entity.Id &&                    // kendisi hariç (Update için)
                x.Name != null &&
                x.Name.Trim().ToLower() == normalized
            );

            return exists
                ? new ErrorResult("در همان مارکیت و همان منزل، دکانی به همین نام قبلاً موجود است.")
                : new SuccessResult();
        }

        public async Task<IDataResult<List<FranchiseOwnerShopsDto>>> GetFranchiseOwnerShops(Guid ownerId)
        {
            if (ownerId == Guid.Empty)
                return new ErrorDataResult<List<FranchiseOwnerShopsDto>>(null, "ownerId is required.");

            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                // 1) BUGÜN itibarıyla BENİM paylarım → tek satır/ShopId + toplam %
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

                // 2) Dükkan + (opsiyonel) Kat/Market + BUGÜN GEÇERLİ & ACTIVE en güncel kontrat (+ Tenant)
                var projected = await (
                    from osh in ownerSharesAtDate
                    join sh in _shopDal.QueryNoTracking() on osh.ShopId equals sh.Id

                    join f in _floorDal.QueryNoTracking() on sh.FloorId equals f.Id into fgrp
                    from f in fgrp.DefaultIfEmpty()

                    join m in _marketDal.QueryNoTracking() on sh.MarketId equals m.Id into mgrp
                    from m in mgrp.DefaultIfEmpty()

                        // LEFT APPLY: en güncel, bugün geçerli ve ACTIVE kontrat
                    from c in _contractDal.QueryNoTracking()
                        .Where(x => x.ShopId == osh.ShopId
                                    && x.Status == ContractStatus.Active
                                    && x.StartDate <= today
                                    && (x.EndDate == null || x.EndDate >= today))
                        .OrderByDescending(x => x.SignedOn)
                        .Take(1)
                        .DefaultIfEmpty()

                    join t in _tenantDal.QueryNoTracking() on c.TenantId equals t.Id into tgrp
                    from t in tgrp.DefaultIfEmpty()

                    select new
                    {
                        OwnerId = ownerId,
                        osh.ShopId,
                        ShopName = sh.Name,
                        FloorId = (Guid?)(f != null ? f.Id : (Guid?)null),
                        FloorName = f != null ? f.FloorName : null,
                        MarketId = (Guid?)(m != null ? m.Id : (Guid?)null),
                        MarketName = m != null ? m.MarketName : null,

                        ContractId = (Guid?)(c != null ? c.Id : (Guid?)null),
                        SignedOn = (DateOnly?)(c != null ? c.SignedOn : (DateOnly?)null),
                        StartDate = (DateOnly?)(c != null ? c.StartDate : (DateOnly?)null),
                        EndDate = (DateOnly?)(c != null ? c.EndDate : (DateOnly?)null),
                        DueDay = (int?)(c != null ? c.DueDayOfMonth : (int?)null),
                        Portion = (decimal?)(c != null ? c.FranchisePortionAmount : (decimal?)null),
                        StatusInt = c != null ? (int?)c.Status : null,

                        TenantId = (Guid?)(t != null ? t.Id : (Guid?)null),
                        TenantName = t != null ? (t.DisplayName + " - " + t.FatherName) : null,

                        MySharePercent = osh.MySharePercent
                    }
                ).ToListAsync();

                // 3) DTO'ya çevir (tarih formatlama + clamp/round güvenliği)
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

                    // Benim tutar = Toplam * (Benim Pay % / 100)
                    MyPortionAmount = (x.Portion.HasValue ? (x.Portion.Value * Clamp01(x.MySharePercent)) / 100m : 0m),

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

        //public async Task<IDataResult<List<ShopPickerDto>>> GetAvailableShopsAsync()
        //{
        //    var today = _time.Today();

        //    // “Aktif sözleşme” tanımı:
        //    // - Statü Active OLMALI
        //    // - Tarih aralığı today içinde OLMALI (StartDate <= today <= EndDate)
        //    // Eğer siz Status’ü süresi biter bitmez Archived’a alıyorsanız,
        //    // tarih kontrolünü kaldırıp sadece Status == Active de kullanabilirsiniz.
        //    var busyShopIdsQuery =
        //        _contractDal.QueryNoTracking()
        //                  .Where(c => c.Status == ContractStatus.Active
        //                              && c.StartDate <= today
        //                              && c.EndDate >= today)
        //                  .Select(c => c.ShopId)
        //                  .Distinct();

        //    var query =
        //        _shopDal.QueryNoTracking()
        //             .Where(s => s.IsActive && !busyShopIdsQuery.Contains(s.Id))
        //             .OrderBy(s => s.UnitNumber)
        //             .ThenBy(s => s.Name)
        //             .Select(s => new ShopPickerDto
        //             {
        //                 Id = s.Id,
        //                 UnitNumber = s.UnitNumber,
        //                 Name = s.Name,
        //                 MarketId = s.MarketId,
        //                 FloorId = s.FloorId
        //             });

        //    var list = await query.ToListAsync();
        //    return new SuccessDataResult<List<ShopPickerDto>>(list);
        //}

        public async Task<IDataResult<List<ShopPickerDto>>> GetAvailableShopsAsync()
        {
            var today = _time.Today();

            // 1) Bu dükkâna bugün itibarıyla aktif sözleşme var mı?
            var busyShopIdsQuery =
                _contractDal.QueryNoTracking()
                    .Where(c => c.Status == ContractStatus.Active
                                && c.StartDate <= today
                                && c.EndDate >= today)
                    .Select(c => c.ShopId)
                    .Distinct();

            // 2) Aktif imtiyaz sahibine sahip dükkanlar (var/yok kontrolü)
            var activeShareQuery =
                _shopDal.QueryNoTracking() // Shop -> ShopFranchiseShares (navigation)
                    .Select(s => new
                    {
                        s.Id,
                        HasActive = s.ShopFranchiseShares
                            .Any(sh =>
                                sh.EffectiveFrom <= today &&
                                (sh.EffectiveTo == null || sh.EffectiveTo >= today))
                    });

            var shares = await activeShareQuery.ToDictionaryAsync(x => x.Id, x => x.HasActive);

            // 3) Müsait (sözleşmesiz) dükkânlar
            var shopsQuery =
                _shopDal.QueryNoTracking()
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
                         HasActiveFranchiseOwner = shares.ContainsKey(s.Id) && shares[s.Id]
                     });

            var list = await shopsQuery.ToListAsync();

            return new SuccessDataResult<List<ShopPickerDto>>(list);
        }

    }
}
