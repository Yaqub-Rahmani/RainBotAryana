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
using Entities.Dtos.FranchiseOwner;

namespace Business.Concrete
{
    public class FranchiseOwnerManager : IFranchiseOwnerService
    {
        private readonly IFranchiseOwnerDal _ownerDal;
        private readonly IShopFranchiseShareDal _shareDal;              // Pay/ortaklıklar
        private readonly IFranchiseBalanceEntryDal _balanceEntryDal;    // Hareketler (IN/OUT ledger)

        private readonly IMediaAssetDal _mediaDal;                      // Avatar
        private readonly IPhoneDal _phoneDal;                           // Primary phone
        private readonly IAddressDal _addrDal;                          // Primary address (Province/District dahil)
        private readonly IFranchiseOwnerBalanceDal _ownerBalanceDal;    // Özet bakiye (Balance)
        private readonly IBiometricDal _biometricDal;


        public FranchiseOwnerManager(
            IFranchiseOwnerDal ownerDal,
            IShopFranchiseShareDal shareDal,
            IFranchiseBalanceEntryDal balanceEntryDal,
            IMediaAssetDal mediaDal,
            IPhoneDal phoneDal,
            IAddressDal addrDal,
            IFranchiseOwnerBalanceDal ownerBalanceDal
            ,
            IBiometricDal biometricDal

        )
        {
            _ownerDal = ownerDal;
            _shareDal = shareDal;
            _balanceEntryDal = balanceEntryDal;

            _mediaDal = mediaDal;
            _phoneDal = phoneDal;
            _addrDal = addrDal;
            _ownerBalanceDal = ownerBalanceDal;
            _biometricDal = biometricDal;
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
            var balAgg = _balanceEntryDal.QueryNoTracking()
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
            var bal = _balanceEntryDal.QueryNoTracking()
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
        [CacheRemoveAspect(".*FranchiseOwner.*Get.*")]
        public async Task<IResult> UpsertFingerprintAsync(Guid ownerId, FranchiseOwnerFingerprintUpsertDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.TemplateBase64))
                return new ErrorResult("TemplateBase64 zorunlu.");

            var exists = await _ownerDal.QueryNoTracking().AnyAsync(x => x.Id == ownerId);
            if (!exists) return new ErrorResult("صاحب امتیاز یافت نشد.");

            var templateBytes = DecodeBase64(dto.TemplateBase64);

            var actives = await _biometricDal.Query()
                .Where(x => x.OwnerType == MediaOwnerType.FranchiseOwner
                         && x.OwnerId == ownerId
                         && x.Type == BiometricType.Fingerprint
                         && x.IsActive)
                .ToListAsync();

            foreach (var a in actives)
            {
                a.IsActive = false;
                _biometricDal.Update(a);
            }

            var bio = new Biometric
            {
                Id = Guid.NewGuid(),
                OwnerType = MediaOwnerType.FranchiseOwner,
                OwnerId = ownerId,
                Type = BiometricType.Fingerprint,
                Template = templateBytes,
                RelatedMediaAssetId = dto.RelatedMediaAssetId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _biometricDal.Add(bio);
            return new SuccessResult("اثر انگشت بروزرسانی شد.");
        }

        private static byte[] DecodeBase64(string input)
        {
            var s = (input ?? "").Trim();
            var idx = s.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) s = s.Substring(idx + "base64,".Length);
            return Convert.FromBase64String(s);
        }


        [SecuredOperation("franchiseowner.write,admin")]
        [ValidationAspect(typeof(FranchiseOwnerValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseOwnerService.Get")]
        public IResult Add(FranchiseOwner owner)
        {
            // Basit normalizasyon
            owner.DisplayName = owner.DisplayName?.Trim();
            owner.NationalId = owner.NationalId?.Trim();

            // Aynı isim + aynı kimlik numarası var mı?
            var duplicate = _ownerDal.Get(x =>
                x.DisplayName.ToLower() == owner.DisplayName.ToLower() &&
                x.NationalId.ToLower() == owner.NationalId.ToLower()
            );

            if (duplicate != null)
                return new ErrorResult("مالک/امتیاز با همین نام و شمارهٔ تذکره قبلاً ثبت شده است.");

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

        // ------------------------------------------------------------
        // Owner View: Tümü (filtre: active?) — N+1 yok, toplu çekim.
        // ------------------------------------------------------------
        [CacheAspect(10)]
        public IDataResult<List<FranchiseOwnerViewDto>> GetAllOwnerView(bool? active = null)
        {
            // 1) Owner çek
            var ownersQ = _ownerDal.QueryNoTracking();
            if (active.HasValue) ownersQ = ownersQ.Where(o => o.IsActive == active.Value);

            var owners = ownersQ
                .Select(o => new { o.Id, o.DisplayName, o.IsActive })
                .OrderByDescending(o => o.IsActive).ThenBy(o => o.DisplayName)
                .ToList();

            var ownerIds = owners.Select(x => x.Id).ToList();
            if (ownerIds.Count == 0)
                return new SuccessDataResult<List<FranchiseOwnerViewDto>>(new());

            // 2) Avatar (primary) — MediaAsset
            var avatars = _mediaDal.QueryNoTracking()
                .Where(m => ownerIds.Contains(m.OwnerId)
                            && m.OwnerType == MediaOwnerType.FranchiseOwner
                            && m.Kind == MediaKind.Avatar
                            && m.IsPrimary)
                .Select(m => new { m.OwnerId, m.StorageUrl })
                .ToList()
                .GroupBy(x => x.OwnerId)
                .ToDictionary(g => g.Key, g => g.First().StorageUrl);

            // 3) Primary Phone
            var phones = _phoneDal.QueryNoTracking()
                .Where(p => ownerIds.Contains(p.OwnerId)
                            && p.OwnerType == MediaOwnerType.FranchiseOwner
                            && p.IsPrimary)
                .Select(p => new { p.OwnerId, p.Number })
                .ToList()
                .GroupBy(x => x.OwnerId)
                .ToDictionary(g => g.Key, g => g.First().Number);

            // 4) Primary Address (+ Province, District)
            // Not: Province/District navigation'ı Address üzerinde tanımlı.
            var addresses = _addrDal.QueryNoTracking()
                .Where(a => ownerIds.Contains(a.OwnerId)
                            && a.OwnerType == MediaOwnerType.FranchiseOwner
                            && a.IsPrimary)
                .Include(a => a.Province)
                .Include(a => a.District)
                .Select(a => new
                {
                    a.OwnerId,
                    ProvinceName = a.Province != null ? a.Province.Name : null,
                    DistrictName = a.District != null ? a.District.Name : null,
                    a.Line1
                })
                .ToList()
                .GroupBy(x => x.OwnerId)
                .ToDictionary(g => g.Key, g => FormatAddress(g.First().ProvinceName, g.First().DistrictName, g.First().Line1));

            // 5) Balance (FranchiseBalanceEntry üzerinden)
            var balances = _balanceEntryDal.QueryNoTracking()
                .Where(e => ownerIds.Contains(e.FranchiseOwnerId) && e.IsActive)
                .GroupBy(e => e.FranchiseOwnerId)
                .Select(g => new
                {
                    FranchiseOwnerId = g.Key,
                    Balance =
                        (g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m)
                      - (g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m)
                })
                .ToList()
                .ToDictionary(x => x.FranchiseOwnerId, x => x.Balance);


            // 6) Birleştir
            var result = owners.Select(o => new FranchiseOwnerViewDto
            {
                Id = o.Id,
                DisplayName = o.DisplayName,
                IsActive = o.IsActive,
                Avatar = avatars.TryGetValue(o.Id, out var url) ? url : null,
                PrimaryPhone = phones.TryGetValue(o.Id, out var ph) ? ph : null,
                AddressFull = addresses.TryGetValue(o.Id, out var addr) ? addr : null,
                Balance = balances.TryGetValue(o.Id, out var bal) ? bal : 0m
            }).ToList();

            return new SuccessDataResult<List<FranchiseOwnerViewDto>>(result);
        }

        // ------------------------------------------------------------
        // Owner View: Paged + Search + Sort (QueryOptions)
        // ------------------------------------------------------------
        [CacheAspect(10)]
        public IDataResult<PagedResult<FranchiseOwnerViewDto>> GetOwnerViews(QueryOptions opt, bool? active = null)
        {
            opt ??= new QueryOptions();

            // 1) Owner base sorgu + filtreler
            var owners = _ownerDal.QueryNoTracking();
            if (active.HasValue) owners = owners.Where(o => o.IsActive == active.Value);

            owners = owners.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                var like = $"%{key}%";
                owners = owners.Where(o =>
                    EF.Functions.Like(o.DisplayName, like) ||
                    (o.Email != null && EF.Functions.Like(o.Email, like)) ||
                    (o.Notes != null && EF.Functions.Like(o.Notes, like))
                );
            }

            owners = owners.ApplySort(opt.Sort);
            if (!owners.Expression.ToString()!.Contains("OrderBy"))
                owners = owners.OrderByDescending(o => o.IsActive).ThenBy(o => o.DisplayName);

            // 2) Count
            var total = owners.Count();

            // 3) Page slice (yalın alanlar)
            var page = owners
                .Skip((opt.Page - 1) * opt.PageSize)
                .Take(opt.PageSize)
                .Select(o => new { o.Id, o.DisplayName, o.IsActive })
                .ToList();

            var ids = page.Select(x => x.Id).ToList();
            if (ids.Count == 0)
                return new SuccessDataResult<PagedResult<FranchiseOwnerViewDto>>(new PagedResult<FranchiseOwnerViewDto>
                {
                    Items = new List<FranchiseOwnerViewDto>(),
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });

            // 4) Avatars
            var avatars = _mediaDal.QueryNoTracking()
                .Where(m => ids.Contains(m.OwnerId)
                            && m.OwnerType == MediaOwnerType.FranchiseOwner
                            && m.Kind == MediaKind.Avatar
                            && m.IsPrimary)
                .Select(m => new { m.OwnerId, m.StorageUrl })
                .ToList()
                .GroupBy(x => x.OwnerId)
                .ToDictionary(g => g.Key, g => g.First().StorageUrl);

            // 5) Phones
            var phones = _phoneDal.QueryNoTracking()
                .Where(p => ids.Contains(p.OwnerId)
                            && p.OwnerType == MediaOwnerType.FranchiseOwner
                            && p.IsPrimary)
                .Select(p => new { p.OwnerId, p.Number })
                .ToList()
                .GroupBy(x => x.OwnerId)
                .ToDictionary(g => g.Key, g => g.First().Number);

            // 6) Addresses
            var addresses = _addrDal.QueryNoTracking()
                .Where(a => ids.Contains(a.OwnerId)
                            && a.OwnerType == MediaOwnerType.FranchiseOwner
                            && a.IsPrimary)
                .Include(a => a.Province)
                .Include(a => a.District)
                .Select(a => new
                {
                    a.OwnerId,
                    ProvinceName = a.Province != null ? a.Province.Name : null,
                    DistrictName = a.District != null ? a.District.Name : null,
                    a.Line1
                })
                .ToList()
                .GroupBy(x => x.OwnerId)
                .ToDictionary(g => g.Key, g => FormatAddress(g.First().ProvinceName, g.First().DistrictName, g.First().Line1));


            // 7) Balances (FranchiseBalanceEntry üzerinden)
            var balances = _balanceEntryDal.QueryNoTracking()
                .Where(e => ids.Contains(e.FranchiseOwnerId) && e.IsActive)
                .GroupBy(e => e.FranchiseOwnerId)
                .Select(g => new
                {
                    FranchiseOwnerId = g.Key,
                    Balance =
                        (g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m)
                      - (g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m)
                })
                .ToList()
                .ToDictionary(x => x.FranchiseOwnerId, x => x.Balance);


            // 8) Merge
            var items = page.Select(o => new FranchiseOwnerViewDto
            {
                Id = o.Id,
                DisplayName = o.DisplayName,
                IsActive = o.IsActive,
                Avatar = avatars.TryGetValue(o.Id, out var url) ? url : null,
                PrimaryPhone = phones.TryGetValue(o.Id, out var ph) ? ph : null,
                AddressFull = addresses.TryGetValue(o.Id, out var addr) ? addr : null,
                Balance = balances.TryGetValue(o.Id, out var bal) ? bal : 0m
            }).ToList();

            return new SuccessDataResult<PagedResult<FranchiseOwnerViewDto>>(new PagedResult<FranchiseOwnerViewDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            });
        }

        // ------------------------------------------------------------
        // Yardımcı
        // ------------------------------------------------------------
        private static string? FormatAddress(string? province, string? district, string? line1)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(province)) parts.Add(province);
            if (!string.IsNullOrWhiteSpace(district)) parts.Add(district);
           // if (!string.IsNullOrWhiteSpace(line1)) parts.Add(line1);
            return parts.Count == 0 ? null : string.Join(" - ", parts);
        }


        // FranchiseOwnerManager içine EKLE/DEĞİŞTİR
        [CacheAspect(10)]
        public IDataResult<OwnerProfileDto> GetProfile(Guid ownerId)
        {
            var ownerType = MediaOwnerType.FranchiseOwner;

            var kAvatar = MediaKind.Avatar;
            var kIdFront = MediaKind.IdFront;
            var kIdBack = MediaKind.IdBack;
            var kFinger = MediaKind.FingerBitmap;
            var kContract = MediaKind.FranchiseContract;

            var q =
                from o in _ownerDal.QueryNoTracking()
                where o.Id == ownerId

                // Phones
                let phonePrimary =
                    (from p in _phoneDal.QueryNoTracking()
                     where p.OwnerType == ownerType && p.OwnerId == o.Id
                     orderby p.IsPrimary descending, p.Id ascending
                     select p.Number).FirstOrDefault()

                let phoneSecondary =
                    (from p in _phoneDal.QueryNoTracking()
                     where p.OwnerType == ownerType && p.OwnerId == o.Id
                     orderby p.IsPrimary descending, p.Id ascending
                     select p.Number).Skip(1).FirstOrDefault()

                // Address (primary öncelikli)
                let addr =
                    (from a in _addrDal.QueryNoTracking()
                     where a.OwnerType == ownerType && a.OwnerId == o.Id
                     orderby a.IsPrimary descending
                     select new
                     {
                         Province = a.Province != null ? a.Province.Name : null,
                         District = a.District != null ? a.District.Name : null,
                         a.Line1
                     }).FirstOrDefault()

                // Media (primary öncelikli)
                let avatarUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == o.Id && m.Kind == kAvatar
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()
                let idFrontUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == o.Id && m.Kind == kIdFront
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()
                let idBackUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == o.Id && m.Kind == kIdBack
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()
                let fingerUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == o.Id && m.Kind == kFinger
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()
                let contractUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == o.Id && m.Kind == kContract
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                // Balance (FranchiseBalanceEntry’den)
                //let bal =
                //    (from b in _balanceEntryDal.QueryNoTracking()
                //     where b.FranchiseOwnerId == o.Id
                //     group b by 1 into g
                //     select new
                //     {
                //         Balance =
                //             (g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m)
                //           - (g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m)
                //     }).FirstOrDefault()

                    let bal =
                        (from b in _balanceEntryDal.QueryNoTracking()
                        where b.FranchiseOwnerId == o.Id
                        group b by 1 into g
                        select new
                        {
                            Balance = g.Sum(x => x.Direction == BalanceDirection.In
                                ? (decimal?)x.Amount
                                : -(decimal?)x.Amount) ?? 0m
                        }).FirstOrDefault()



                select new OwnerProfileDto
                {
                    Id = o.Id.ToString(),
                    DisplayName = o.DisplayName,
                    FatherName = o.FatherName,
                    Gender = o.Gender,
                    NationalId = o.NationalId,

                    PhonePrimary = phonePrimary,
                    PhoneSecondary = phoneSecondary,

                    Province = addr != null ? addr.Province : null,
                    District = addr != null ? addr.District : null,
                    AddressLine = addr != null ? addr.Line1 : null,

                    AddressFull =
                        (addr == null)
                            ? null
                            : (
                                (addr.Province ?? "") +
                                ((addr.Province != null && addr.District != null) ? ", " : "") +
                                (addr.District ?? "") +
                                (((addr.Province != null || addr.District != null) && addr.Line1 != null) ? " - " : "") +
                                (addr.Line1 ?? "")
                              ),

                    // Ayrı description yoksa Notes
                    Description = o.Notes,

                    Balance = bal != null ? bal.Balance : 0m,

                    AvatarUrl = avatarUrl,
                    IdFrontUrl = idFrontUrl,
                    IdBackUrl = idBackUrl,
                    FingerBitmapUrl = fingerUrl,
                    ContractUrl = contractUrl
                };

            var dto = q.FirstOrDefault();
            if (dto == null)
                return new ErrorDataResult<OwnerProfileDto>(null, "یافت نشد.");

            // ---- B seçeneği: Null stringleri boş stringe sabitle ----
            dto.DisplayName ??= string.Empty;
            dto.FatherName ??= string.Empty;
            dto.NationalId ??= string.Empty;

            dto.PhonePrimary ??= string.Empty;
            dto.PhoneSecondary ??= string.Empty;

            dto.Province ??= string.Empty;
            dto.District ??= string.Empty;
            dto.AddressLine ??= string.Empty;
            dto.AddressFull ??= string.Empty;

            dto.Description ??= string.Empty;

            dto.AvatarUrl ??= string.Empty;
            dto.IdFrontUrl ??= string.Empty;
            dto.IdBackUrl ??= string.Empty;
            dto.FingerBitmapUrl ??= string.Empty;
            dto.ContractUrl ??= string.Empty;

            return new SuccessDataResult<OwnerProfileDto>(dto);
        }

        public async Task<IDataResult<FranchiseOwnerBalanceEntryPageDto>> GetBalanceEntries(
            Guid ownerId,
            FranchiseOwnerBalanceEntryQueryOptions opt)
        {
            if (ownerId == Guid.Empty)
                return new ErrorDataResult<FranchiseOwnerBalanceEntryPageDto>(null, "ownerId is required.");

            var q = _balanceEntryDal.QueryNoTracking()
                .Where(x => x.FranchiseOwnerId == ownerId);

            // Date filter (FromDate <= OccurredAtUtc < ToDate+1)
            if (opt.FromDate.HasValue)
            {
                var from = DateTime.SpecifyKind(opt.FromDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                q = q.Where(x => x.OccurredAtUtc >= from);
            }
            if (opt.ToDate.HasValue)
            {
                var toExclusive = DateTime.SpecifyKind(opt.ToDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                q = q.Where(x => x.OccurredAtUtc < toExclusive);
            }

            if (opt.Direction.HasValue)
                q = q.Where(x => x.Direction == opt.Direction.Value);

            if (opt.PaymentMethod.HasValue)
                q = q.Where(x => x.PaymentMethod == opt.PaymentMethod.Value);

            if (opt.MinAmount.HasValue)
                q = q.Where(x => x.Amount >= opt.MinAmount.Value);

            if (opt.MaxAmount.HasValue)
                q = q.Where(x => x.Amount <= opt.MaxAmount.Value);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var s = opt.Search.Trim();
                q = q.Where(x =>
                    (x.Notes != null && x.Notes.Contains(s)) ||
                    (x.PaymentReference != null && x.PaymentReference.Contains(s)));
            }

            // Özet (filtrelenmiş tüm kayıtlar için)
            var summary = await q
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalIn = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                    TotalOut = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                })
                .FirstOrDefaultAsync();

            var total = await q.CountAsync();

            // Sıralama: profil için en mantıklısı newest-first
            q = q.OrderByDescending(x => x.OccurredAtUtc).ThenByDescending(x => x.CreatedAtUtc);

            var pageItems = await q
                .Skip((opt.Page - 1) * opt.PageSize)
                .Take(opt.PageSize)
                .Select(x => new FranchiseOwnerBalanceEntryListItemDto
                {
                    Id = x.Id,
                    OccurredAtUtc = x.OccurredAtUtc,
                    Direction = x.Direction,
                    Amount = x.Amount,
                    PaymentMethod = x.PaymentMethod,
                    PaymentReference = x.PaymentReference,
                    Notes = x.Notes,
                    CreatedByUserId = x.CreatedByUserId,
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToListAsync();

            var result = new FranchiseOwnerBalanceEntryPageDto
            {
                Page = new PagedResult<FranchiseOwnerBalanceEntryListItemDto>
                {
                    Items = pageItems,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                },
                TotalIn = summary?.TotalIn ?? 0m,
                TotalOut = summary?.TotalOut ?? 0m
            };

            return new SuccessDataResult<FranchiseOwnerBalanceEntryPageDto>(result);
        }


    }
}
