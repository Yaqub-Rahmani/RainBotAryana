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
using Microsoft.EntityFrameworkCore;

namespace Business.Concrete
{
    public class FranchiseOwnerManager : IFranchiseOwnerService
    {
        private readonly IFranchiseOwnerDal _ownerDal;
        private readonly IShopFranchiseShareDal _shareDal;           // varsayım: mevcut
        private readonly IFranchiseBalanceEntryDal _balanceDal;      // mevcut (önceki tabloda kurduk)

        public FranchiseOwnerManager(IFranchiseOwnerDal ownerDal, IShopFranchiseShareDal shareDal, IFranchiseBalanceEntryDal balanceDal)
        {
            _ownerDal = ownerDal;
            _shareDal = shareDal;
            _balanceDal = balanceDal;
        }

        // -------- Okumalar --------

        [CacheAspect(10)]
        public IDataResult<List<FranchiseOwner>> GetAll(bool? active = null)
        {
            var q = _ownerDal.QueryNoTracking();
            if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);
            var list = q.OrderByDescending(x => x.IsActive).ThenBy(x => x.DisplayName).ToList();
            return new SuccessDataResult<List<FranchiseOwner>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<FranchiseOwner> GetById(Guid id)
        {
            var e = _ownerDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<FranchiseOwner>(null, "یافت نشد.") : new SuccessDataResult<FranchiseOwner>(e);
        }


        [CacheAspect(10)]
        public IDataResult<PagedResult<FranchiseOwnerDetailDto>> GetDetails(QueryOptions opt, bool? active = null)
        {
            opt ??= new QueryOptions();

            // 1) Temel sorgu (owner)
            var owners = _ownerDal.QueryNoTracking();
            if (active.HasValue) owners = owners.Where(x => x.IsActive == active.Value);

            owners = owners.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                var like = $"%{key}%";
                owners = owners.Where(x =>
                    EF.Functions.Like(x.DisplayName, like) ||
                    (x.Phone != null && EF.Functions.Like(x.Phone, like)) ||
                    (x.Email != null && EF.Functions.Like(x.Email, like)) ||
                    (x.Notes != null && EF.Functions.Like(x.Notes, like))
                );
            }

            owners = owners.ApplySort(opt.Sort);
            // Varsayılan deterministik sıralama
            owners = owners.OrderByDescending(x => x.IsActive).ThenBy(x => x.DisplayName);

            // 2) Total (paging öncesi)
            var total = owners.Count();

            // 3) Sayfayı çek (yalnızca gerekli alanlar)
            var pageOwners = owners
                .Skip((opt.Page - 1) * opt.PageSize)
                .Take(opt.PageSize)
                .Select(o => new {
                    o.Id,
                    o.DisplayName,
                    o.Phone,
                    o.Email,
                    o.Notes,
                    o.IsActive
                })
                .ToList();

            var ownerIds = pageOwners.Select(x => x.Id).ToList();
            if (ownerIds.Count == 0)
            {
                return new SuccessDataResult<PagedResult<FranchiseOwnerDetailDto>>(new PagedResult<FranchiseOwnerDetailDto>
                {
                    Items = new List<FranchiseOwnerDetailDto>(),
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
            }

            // 4) Pay (ShopFranchiseShare) özetlerini tek seferde getir
            // Active sayımı için "bugün" (isterseniz metoda parametre eklenebilir)
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var shareAgg = _shareDal.QueryNoTracking()
                .Where(s => ownerIds.Contains(s.FranchiseOwnerId))
                .GroupBy(s => s.FranchiseOwnerId)
                .Select(g => new {
                    OwnerId = g.Key,
                    TotalCount = g.Count(),
                    // Bugün aktif: from <= today && (to is null || to >= today)
                    ActiveToday = g.Count(s => s.EffectiveFrom <= today && (!s.EffectiveTo.HasValue || s.EffectiveTo.Value >= today))
                })
                .ToList();

            var shareMapTotal = shareAgg.ToDictionary(x => x.OwnerId, x => x.TotalCount);
            var shareMapActive = shareAgg.ToDictionary(x => x.OwnerId, x => x.ActiveToday);

            // 5) Bakiye özetlerini (In/Out) tek seferde getir
            var balAgg = _balanceDal.QueryNoTracking()
                .Where(b => ownerIds.Contains(b.FranchiseOwnerId))
                .GroupBy(b => b.FranchiseOwnerId)
                .Select(g => new {
                    OwnerId = g.Key,
                    TotalIn = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                    TotalOut = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                })
                .ToList();

            var balMap = balAgg.ToDictionary(x => x.OwnerId, x => (x.TotalIn, x.TotalOut));

            // 6) DTO’ları birleştir
            var items = pageOwners.Select(o =>
            {
                var shopShareCount = shareMapTotal.TryGetValue(o.Id, out var sc) ? sc : 0;
                var activeShopShareCnt = shareMapActive.TryGetValue(o.Id, out var ac) ? ac : 0;
                var totals = balMap.TryGetValue(o.Id, out var t) ? t : (0m, 0m);

                return new FranchiseOwnerDetailDto
                {
                    Id = o.Id,
                    DisplayName = o.DisplayName,
                    Phone = o.Phone,
                    Email = o.Email,
                    Notes = o.Notes,
                    IsActive = o.IsActive,
                    ShopShareCount = shopShareCount,
                    ActiveShopShareCount = activeShopShareCnt,
                    TotalIn = totals.Item1,
                    TotalOut = totals.Item2
                };
            }).ToList();

            return new SuccessDataResult<PagedResult<FranchiseOwnerDetailDto>>(new PagedResult<FranchiseOwnerDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            });
        }

        [CacheAspect(10)]
        public IDataResult<FranchiseOwnerDetailDto> GetDetailById(Guid id)
        {
            // 1) Sahibi temel alanlarıyla çek
            var owner = _ownerDal.QueryNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new
                {
                    o.Id,
                    o.DisplayName,
                    o.Phone,
                    o.Email,
                    o.Notes,
                    o.IsActive
                })
                .FirstOrDefault();

            if (owner == null)
                return new ErrorDataResult<FranchiseOwnerDetailDto>(null, "یافت نشد.");

            // 2) Bugün aktif pay sayımı için tarih
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // 3) Pay (ShopFranchiseShare) özetleri
            var shares = _shareDal.QueryNoTracking().Where(s => s.FranchiseOwnerId == id);
            var shopShareCount = shares.Count();
            var activeShopShareCount = shares.Count(s =>
                s.EffectiveFrom <= today && (!s.EffectiveTo.HasValue || s.EffectiveTo.Value >= today));

            // 4) Bakiye (FranchiseBalanceEntry) özetleri – tek group ile
            var bal = _balanceDal.QueryNoTracking()
                .Where(b => b.FranchiseOwnerId == id)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    In = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                    Out = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                })
                .FirstOrDefault();

            var dto = new FranchiseOwnerDetailDto
            {
                Id = owner.Id,
                DisplayName = owner.DisplayName,
                Phone = owner.Phone,
                Email = owner.Email,
                Notes = owner.Notes,
                IsActive = owner.IsActive,
                ShopShareCount = shopShareCount,
                ActiveShopShareCount = activeShopShareCount,
                TotalIn = bal?.In ?? 0m,
                TotalOut = bal?.Out ?? 0m
            };

            return new SuccessDataResult<FranchiseOwnerDetailDto>(dto);
        }

        // -------- Yazmalar --------

        [SecuredOperation("franchiseowner.write,admin")]
        [ValidationAspect(typeof(FranchiseOwnerValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseOwnerService.Get")]
        public IResult Add(FranchiseOwner owner)
        {
            var dup = _ownerDal.Get(x => x.DisplayName.ToLower() == owner.DisplayName.ToLower());
            if (dup != null) return new ErrorResult("مالک/امتیاز با همین نام وجود دارد.");

            _ownerDal.Add(owner);
            return new SuccessResult("صاحب امتیاز افزوده شد.");
        }

        [SecuredOperation("franchiseowner.write,admin")]
        [ValidationAspect(typeof(FranchiseOwnerValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseOwnerService.Get")]
        public IResult Update(FranchiseOwner owner)
        {
            var current = _ownerDal.Get(x => x.Id == owner.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            var dup = _ownerDal.Get(x => x.Id != owner.Id && x.DisplayName.ToLower() == owner.DisplayName.ToLower());
            if (dup != null) return new ErrorResult("مالک/امتیاز با همین نام وجود دارد.");

            current.DisplayName = owner.DisplayName;
            current.Phone = owner.Phone;
            current.Email = owner.Email;
            current.Notes = owner.Notes;
            current.IsActive = owner.IsActive;

            _ownerDal.Update(current);
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("franchiseowner.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseOwnerService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _ownerDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            _ownerDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        [SecuredOperation("franchiseowner.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseOwnerService.Get")]
        public IResult Activate(Guid id)
        {
            var e = _ownerDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.IsActive = true;
            _ownerDal.Update(e);
            return new SuccessResult("فعال شد.");
        }

        [SecuredOperation("franchiseowner.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseOwnerService.Get")]
        public IResult Deactivate(Guid id)
        {
            var e = _ownerDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.IsActive = false;
            _ownerDal.Update(e);
            return new SuccessResult("غیرفعال شد.");
        }
    }
}
