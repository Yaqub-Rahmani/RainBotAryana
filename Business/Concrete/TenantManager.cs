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
using Entities.Dtos.tenant;

namespace Business.Concrete
{
    public class TenantManager : ITenantService
    {
        private readonly ITenantDal _tenantDal;
        private readonly IMediaAssetDal _mediaDal;
        private readonly IPhoneDal _phoneDal;
        private readonly IAddressDal _addrDal;
        private readonly IAccrualDal _accrualDal;

        private readonly ICollectionDal _collectionDal;
        private readonly IContractDal _contractDal;
        private readonly IShopDal _shopDal;

        private readonly IBiometricDal _biometricDal;

        public TenantManager(
            ITenantDal tenantDal,
            IMediaAssetDal mediaDal,
            IPhoneDal phoneDal,
            IAddressDal addrDal,
            IAccrualDal accrualDal,
            ICollectionDal collectionDal,
            IContractDal contractDal,
            IShopDal shopDal,
            IBiometricDal biometricDal
            )
        {
            _tenantDal = tenantDal;
            _mediaDal = mediaDal;
            _phoneDal = phoneDal;
            _addrDal = addrDal;
            _accrualDal = accrualDal;
            _collectionDal = collectionDal;
            _contractDal = contractDal;
            _shopDal = shopDal;
            _biometricDal = biometricDal;

        }


        // ------- Reads -------
        [CacheAspect(10)]
        public IDataResult<List<Tenant>> GetAll(bool? onlyActive = null)
        {
            var q = _tenantDal.QueryNoTracking();
            if (onlyActive == true) q = q.Where(x => x.IsActive);
            return new SuccessDataResult<List<Tenant>>(q.OrderBy(x => x.DisplayName).ToList());
        }

        private static TenantDetailDto Project(Tenant t)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Önce aktif sözleşmeyi bul
            var active = t.Contracts
                          .Where(c => c.Status == ContractStatus.Active
                                   && c.StartDate <= today && today <= c.EndDate)
                          .OrderByDescending(c => c.StartDate)
                          .FirstOrDefault();

            // Yoksa en son sözleşme
            var contract = active ?? t.Contracts.OrderByDescending(c => c.StartDate).FirstOrDefault();

            return new TenantDetailDto
            {
                Id = t.Id,
                DisplayName = t.DisplayName,
                Email = t.Email,
                TaxOrIdNumber = t.TaxOrIdNumber,
                IsActive = t.IsActive,

                ContractId = contract?.Id,
                ContractStart = contract?.StartDate,
                ContractEnd = contract?.EndDate,
                ContractVersion = contract?.Version,
                ContractStatus = contract?.Status.ToString()
            };
        }


        [CacheAspect(10)]
        public IDataResult<Tenant> GetById(Guid id)
        {
            var e = _tenantDal.QueryNoTracking()
                        .Include(x => x.Contracts)
                        .FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<Tenant>(null, "یافت نشد.")
                             : new SuccessDataResult<Tenant>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<TenantDetailDto>> GetDetails(QueryOptions? opt, bool? onlyActive = null)
        {
            opt ??= new QueryOptions();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var q = _tenantDal.QueryNoTracking();

            if (onlyActive == true)
                q = q.Where(x => x.IsActive);

            // Sizin filtre extension’ınız
            q = q.ApplyFilters(opt.Filters);

            // Arama
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                var like = $"%{key}%";
                q = q.Where(x =>
                    EF.Functions.Like(x.DisplayName, like) ||
                    (x.Email != null && EF.Functions.Like(x.Email, like)) ||
                    (x.TaxOrIdNumber != null && EF.Functions.Like(x.TaxOrIdNumber, like)));
            }

            // DTO projeksiyonu – Include gerekmez
            var qDto =
                q.Select(x => new TenantDetailDto
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    Email = x.Email,
                    TaxOrIdNumber = x.TaxOrIdNumber,
                    IsActive = x.IsActive,

                    // Önce aktif & tarih aralığında olan sözleşme, yoksa en yeni sözleşme
                    ContractId =
                        x.Contracts
                         .Where(c => c.Status == ContractStatus.Active
                                  && c.StartDate <= today && today <= c.EndDate)
                         .OrderByDescending(c => c.StartDate)
                         .Select(c => (Guid?)c.Id)
                         .FirstOrDefault()
                        ?? x.Contracts
                         .OrderByDescending(c => c.StartDate)
                         .Select(c => (Guid?)c.Id)
                         .FirstOrDefault(),

                    ContractStart =
                        x.Contracts
                         .Where(c => c.Status == ContractStatus.Active
                                  && c.StartDate <= today && today <= c.EndDate)
                         .OrderByDescending(c => c.StartDate)
                         .Select(c => (DateOnly?)c.StartDate)
                         .FirstOrDefault()
                        ?? x.Contracts
                         .OrderByDescending(c => c.StartDate)
                         .Select(c => (DateOnly?)c.StartDate)
                         .FirstOrDefault(),

                    ContractEnd =
                        x.Contracts
                         .Where(c => c.Status == ContractStatus.Active
                                  && c.StartDate <= today && today <= c.EndDate)
                         .OrderByDescending(c => c.StartDate)
                         .Select(c => (DateOnly?)c.EndDate)
                         .FirstOrDefault()
                        ?? x.Contracts
                         .OrderByDescending(c => c.StartDate)
                         .Select(c => (DateOnly?)c.EndDate)
                         .FirstOrDefault(),

                    ContractVersion =
                        x.Contracts
                         .Where(c => c.Status == ContractStatus.Active
                                  && c.StartDate <= today && today <= c.EndDate)
                         .OrderByDescending(c => c.StartDate)
                         .Select(c => c.Version)
                         .FirstOrDefault()
                        ?? x.Contracts
                         .OrderByDescending(c => c.StartDate)
                         .Select(c => c.Version)
                         .FirstOrDefault(),

                    ContractStatus =
                        x.Contracts
                         .Where(c => c.Status == ContractStatus.Active
                                  && c.StartDate <= today && today <= c.EndDate)
                         .OrderByDescending(c => c.StartDate)
                         .Select(c => c.Status.ToString())
                         .FirstOrDefault()
                        ?? x.Contracts
                         .OrderByDescending(c => c.StartDate)
                         .Select(c => c.Status.ToString())
                         .FirstOrDefault(),
                });

            // Sıralama – sort yoksa varsayılan uygula
            qDto = qDto.ApplySort(opt.Sort);
            if (string.IsNullOrWhiteSpace(opt.Sort))
                qDto = qDto.OrderBy(x => x.DisplayName);

            var total = qDto.Count();

            var items = qDto
                .Skip((opt.Page - 1) * opt.PageSize)
                .Take(opt.PageSize)
                .ToList();

            var result = new PagedResult<TenantDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            };

            return new SuccessDataResult<PagedResult<TenantDetailDto>>(result);
        }

        [CacheAspect(10)]
        public IDataResult<TenantDetailDto> GetDetailById(Guid id)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Doğrudan projeksiyon
            var dto = _tenantDal.QueryNoTracking()
                          .Where(x => x.Id == id)
                          .Select(x => new TenantDetailDto
                          {
                              Id = x.Id,
                              DisplayName = x.DisplayName,
                              Email = x.Email,
                              TaxOrIdNumber = x.TaxOrIdNumber,
                              IsActive = x.IsActive,

                              ContractId =
                                  x.Contracts
                                   .Where(c => c.Status == ContractStatus.Active
                                            && c.StartDate <= today && today <= c.EndDate)
                                   .OrderByDescending(c => c.StartDate)
                                   .Select(c => (Guid?)c.Id)
                                   .FirstOrDefault()
                                  ?? x.Contracts
                                   .OrderByDescending(c => c.StartDate)
                                   .Select(c => (Guid?)c.Id)
                                   .FirstOrDefault(),

                              ContractStart =
                                  x.Contracts
                                   .Where(c => c.Status == ContractStatus.Active
                                            && c.StartDate <= today && today <= c.EndDate)
                                   .OrderByDescending(c => c.StartDate)
                                   .Select(c => (DateOnly?)c.StartDate)
                                   .FirstOrDefault()
                                  ?? x.Contracts
                                   .OrderByDescending(c => c.StartDate)
                                   .Select(c => (DateOnly?)c.StartDate)
                                   .FirstOrDefault(),

                              ContractEnd =
                                  x.Contracts
                                   .Where(c => c.Status == ContractStatus.Active
                                            && c.StartDate <= today && today <= c.EndDate)
                                   .OrderByDescending(c => c.StartDate)
                                   .Select(c => (DateOnly?)c.EndDate)
                                   .FirstOrDefault()
                                  ?? x.Contracts
                                   .OrderByDescending(c => c.StartDate)
                                   .Select(c => (DateOnly?)c.EndDate)
                                   .FirstOrDefault(),

                              ContractVersion =
                                  x.Contracts
                                   .Where(c => c.Status == ContractStatus.Active
                                            && c.StartDate <= today && today <= c.EndDate)
                                   .OrderByDescending(c => c.StartDate)
                                   .Select(c => c.Version)
                                   .FirstOrDefault()
                                  ?? x.Contracts
                                   .OrderByDescending(c => c.StartDate)
                                   .Select(c => c.Version)
                                   .FirstOrDefault(),

                              ContractStatus =
                                  x.Contracts
                                   .Where(c => c.Status == ContractStatus.Active
                                            && c.StartDate <= today && today <= c.EndDate)
                                   .OrderByDescending(c => c.StartDate)
                                   .Select(c => c.Status.ToString())
                                   .FirstOrDefault()
                                  ?? x.Contracts
                                   .OrderByDescending(c => c.StartDate)
                                   .Select(c => c.Status.ToString())
                                   .FirstOrDefault(),
                          })
                          .FirstOrDefault();

            return dto == null
                ? new ErrorDataResult<TenantDetailDto>(null, "یافت نشد.")
                : new SuccessDataResult<TenantDetailDto>(dto);
        }
    

    //[CacheAspect(10)]
    //public IDataResult<TenantDetailDto> GetDetailById(Guid id)
    //{
    //    var e = _dal.QueryNoTracking().Include(x => x.Contracts).FirstOrDefault(x => x.Id == id);
    //    return e == null ? new ErrorDataResult<TenantDetailDto>(null, "یافت نشد.")
    //                     : new SuccessDataResult<TenantDetailDto>(Project(e));
    //}

    // ------- Writes -------
          [SecuredOperation("tenant.write,admin")]
        [ValidationAspect(typeof(TenantValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ITenantService.Get")]
        public IResult Add(Tenant entity)
        {
            // Basit: isim uniq değilse de kabul (isterseniz uniq kuralı eklenir)
            _tenantDal.Add(entity);
            return new SuccessResult("مستأجر ثبت شد.");
        }

        [SecuredOperation("tenant.write,admin")]
        [ValidationAspect(typeof(TenantValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ITenantService.Get")]
        public IResult Update(Tenant entity)
        {
            var e = _tenantDal.Get(x => x.Id == entity.Id);
            if (e == null) return new ErrorResult("یافت نشد.");

            e.DisplayName = entity.DisplayName;
            e.Email = entity.Email;
            e.TaxOrIdNumber = entity.TaxOrIdNumber;
            e.IsActive = entity.IsActive;

            _tenantDal.Update(e);
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("tenant.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ITenantService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _tenantDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _tenantDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        // ------- Activate / Deactivate -------
        [SecuredOperation("tenant.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ITenantService.Get")]
        public IResult Activate(Guid id)
        {
            var e = _tenantDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.IsActive = true;
            _tenantDal.Update(e);
            return new SuccessResult("فعال شد.");
        }

        [SecuredOperation("tenant.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ITenantService.Get")]
        public IResult Deactivate(Guid id)
        {
            var e = _tenantDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.IsActive = false;
            _tenantDal.Update(e);
            return new SuccessResult("غیرفعال شد.");
        }

        [CacheAspect(10)]
        public IDataResult<List<TenantViewDto>> GetAllTenantView(bool? active = null)
        {
            // 1) Tenants
            var tenantsQ = _tenantDal.QueryNoTracking();
            if (active.HasValue) tenantsQ = tenantsQ.Where(t => t.IsActive == active.Value);

            var tenants = tenantsQ
                .Select(t => new { t.Id, t.DisplayName, t.IsActive })
                .OrderByDescending(t => t.IsActive).ThenBy(t => t.DisplayName)
                .ToList();

            var tenantIds = tenants.Select(x => x.Id).ToList();
            if (tenantIds.Count == 0)
                return new SuccessDataResult<List<TenantViewDto>>(new());

            // 2) Avatar (primary) — MediaAsset
            var avatars = _mediaDal.QueryNoTracking()
                .Where(m => tenantIds.Contains(m.OwnerId)
                            && m.OwnerType == MediaOwnerType.Tenant
                            && m.Kind == MediaKind.Avatar
                            && m.IsPrimary)
                .Select(m => new { m.OwnerId, m.StorageUrl })
                .ToList()
                .GroupBy(x => x.OwnerId)
                .ToDictionary(g => g.Key, g => g.First().StorageUrl);

            // 3) Primary Phone
            var phones = _phoneDal.QueryNoTracking()
                .Where(p => tenantIds.Contains(p.OwnerId)
                            && p.OwnerType == MediaOwnerType.Tenant
                            && p.IsPrimary)
                .Select(p => new { p.OwnerId, p.Number })
                .ToList()
                .GroupBy(x => x.OwnerId)
                .ToDictionary(g => g.Key, g => g.First().Number);

            // 4) Primary Address (+ Province, District)
            var addresses = _addrDal.QueryNoTracking()
                .Where(a => tenantIds.Contains(a.OwnerId)
                            && a.OwnerType == MediaOwnerType.Tenant
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

            // 5) Balance (mevcut toplam borç) — Accrual.OutstandingAmount toplamı
            var accrualAgg = _accrualDal.QueryNoTracking()
                .Where(a => tenantIds.Contains(a.TenantId))
                .GroupBy(a => a.TenantId)
                .Select(g => new { TenantId = g.Key, Debt = g.Sum(x => x.OutstandingAmount) })
                .ToList()
                .ToDictionary(x => x.TenantId, x => x.Debt);

            // 6) Merge
            var result = tenants.Select(t => new TenantViewDto
            {
                Id = t.Id,
                DisplayName = t.DisplayName,
                IsActive = t.IsActive,
                Avatar = avatars.TryGetValue(t.Id, out var url) ? url : null,
                PrimaryPhone = phones.TryGetValue(t.Id, out var ph) ? ph : null,
                AddressFull = addresses.TryGetValue(t.Id, out var addr) ? addr : null,
                Balance = accrualAgg.TryGetValue(t.Id, out var debt) ? debt : 0m
            }).ToList();

            return new SuccessDataResult<List<TenantViewDto>>(result);
        }

        private static string? FormatAddress(string? province, string? district, string? line1)
        {
            // Province + District + Line1 (boşları atla)
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(province)) parts.Add(province);
            if (!string.IsNullOrWhiteSpace(district)) parts.Add(district);
            if (!string.IsNullOrWhiteSpace(line1)) parts.Add(line1);
            return parts.Count == 0 ? null : string.Join(" - ", parts);
        }



        public IDataResult<TenantProfileDto> GetProfile(Guid id)
        {
            var ownerType = MediaOwnerType.Tenant;

            var q =
                from t in _tenantDal.QueryNoTracking()
                where t.Id == id

                let phonePrimary =
                    (from p in _phoneDal.QueryNoTracking()
                     where p.OwnerType == ownerType && p.OwnerId == t.Id
                     orderby p.IsPrimary descending, p.Id ascending
                     select p.Number).FirstOrDefault()

                let phoneSecondary =
                    (from p in _phoneDal.QueryNoTracking()
                     where p.OwnerType == ownerType && p.OwnerId == t.Id
                     orderby p.IsPrimary descending, p.Id ascending
                     select p.Number).Skip(1).FirstOrDefault()

                let addr =
                    (from a in _addrDal.QueryNoTracking()
                     where a.OwnerType == ownerType && a.OwnerId == t.Id
                     orderby a.IsPrimary descending
                     select new
                     {
                         Province = a.Province != null ? a.Province.Name : null,
                         District = a.District != null ? a.District.Name : null,
                         a.Line1
                     }).FirstOrDefault()

                let avatarUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == t.Id && m.Kind == MediaKind.Avatar
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let idFrontUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == t.Id && m.Kind == MediaKind.IdFront
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let idBackUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == t.Id && m.Kind == MediaKind.IdBack
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let fingerUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == t.Id && m.Kind == MediaKind.FingerBitmap
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let contractUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == t.Id && m.Kind == MediaKind.TenantContract
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let debt =
                    (from a in _accrualDal.QueryNoTracking()
                     where a.TenantId == t.Id
                     group a by 1 into g
                     select g.Sum(x => (decimal?)x.OutstandingAmount) ?? 0m).FirstOrDefault()

                select new TenantProfileDto
                {
                    Id = t.Id.ToString(),
                    DisplayName = t.DisplayName,
                    FatherName = t.FatherName,
                    Gender = t.Gender,
                    NationalId = t.NationalId,
                    IsActive = t.IsActive,

                    PhonePrimary = phonePrimary ?? "",
                    PhoneSecondary = phoneSecondary ?? "",

                    Province = addr != null ? (addr.Province ?? "") : "",
                    District = addr != null ? (addr.District ?? "") : "",
                    AddressLine = addr != null ? (addr.Line1 ?? "") : "",
                    AddressFull =
                        (addr == null) ? "" :
                        ((addr.Province ?? "") +
                         ((addr.Province != null && addr.District != null) ? ", " : "") +
                         (addr.District ?? "") +
                         (((addr.Province != null || addr.District != null) && addr.Line1 != null) ? " - " : "") +
                         (addr.Line1 ?? "")),

                    Balance = debt,

                    AvatarUrl = avatarUrl ?? "",
                    IdFrontUrl = idFrontUrl ?? "",
                    IdBackUrl = idBackUrl ?? "",
                    FingerBitmapUrl = fingerUrl ?? "",
                    ContractUrl = contractUrl ?? ""
                };

            var dto = q.FirstOrDefault();
            if (dto == null) return new ErrorDataResult<TenantProfileDto>(null, "یافت نشد.");

            return new SuccessDataResult<TenantProfileDto>(dto);
        }

        public IDataResult<List<TenantContractBriefDto>> GetContracts(Guid id)
        {
            var list = _tenantDal.QueryNoTracking()
                .Where(t => t.Id == id)
                .SelectMany(t => t.Contracts)
                .Include(c => c.Shop).ThenInclude(s => s.Market)
                .Include(c => c.Shop).ThenInclude(s => s.Floor)
                .OrderByDescending(c => c.StartDate)
                .Select(c => new TenantContractBriefDto
                {
                    ContractId = c.Id,
                    ShopId = c.ShopId,
                    MarketName = c.Shop.Market.MarketName,
                    FloorName = c.Shop.Floor.FloorName,
                    ShopName = c.Shop.Name ?? "",
                    UnitNumber = c.Shop.UnitNumber,
                    StartDate = c.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = c.EndDate.ToString("yyyy-MM-dd"),
                    Status = c.Status.ToString()
                })
                .ToList();

            return new SuccessDataResult<List<TenantContractBriefDto>>(list);
        }

        public async Task<IResult> UpsertFingerprintAsync(Guid id, TenantFingerprintUpsertDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.TemplateBase64))
                return new ErrorResult("TemplateBase64 zorunlu.");

            var exists = await _tenantDal.QueryNoTracking().AnyAsync(x => x.Id == id);
            if (!exists) return new ErrorResult("Kiracı یافت نشد.");

            byte[] DecodeBase64(string input)
            {
                var s = (input ?? "").Trim();
                var idx = s.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0) s = s.Substring(idx + "base64,".Length);
                return Convert.FromBase64String(s);
            }

            var templateBytes = DecodeBase64(dto.TemplateBase64);

            var actives = await _biometricDal.Query()
                .Where(x => x.OwnerType == MediaOwnerType.Tenant
                         && x.OwnerId == id
                         && x.Type == BiometricType.Fingerprint
                         && x.IsActive)
                .ToListAsync();

            foreach (var a in actives)
            {
                a.IsActive = false;
                _biometricDal.Update(a);
            }

            _biometricDal.Add(new Biometric
            {
                Id = Guid.NewGuid(),
                OwnerType = MediaOwnerType.Tenant,
                OwnerId = id,
                Type = BiometricType.Fingerprint,
                Template = templateBytes,
                RelatedMediaAssetId = dto.RelatedMediaAssetId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            return new SuccessResult("اثر انگشت بروزرسانی شد.");
        }

        public async Task<IDataResult<TenantPeriodSummaryPageDto>> GetPeriodSummaries(Guid id, QueryOptions opt)
        {
            if (id == Guid.Empty)
                return new ErrorDataResult<TenantPeriodSummaryPageDto>(null, "tenantId is required.");

            var q = _accrualDal.QueryNoTracking()
                .Where(a => a.TenantId == id)
                .GroupBy(a => new
                {
                    a.ShopId,
                    a.ContractId,
                    a.Year,
                    a.Month,
                    MarketName = a.Shop.Market.MarketName,
                    FloorName = a.Shop.Floor.FloorName,
                    ShopName = a.Shop.Name,
                    a.Shop.UnitNumber
                })
                .Select(g => new TenantPeriodSummaryRowDto
                {
                    ShopId = g.Key.ShopId,
                    ContractId = g.Key.ContractId,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MarketName = g.Key.MarketName,
                    FloorName = g.Key.FloorName,
                    ShopName = g.Key.ShopName ?? "",
                    UnitNumber = g.Key.UnitNumber,

                    TotalAccrued = g.Sum(x => x.Amount),
                    TotalPaid = g.Sum(x => x.Amount - x.OutstandingAmount),
                    TotalOutstanding = g.Sum(x => x.OutstandingAmount),
                });

            // ✅ QueryOptions: filters/sort/search
            q = q.ApplyFilters(opt.Filters);
            q = q.ApplySort(opt.Sort);
            //q = q.ApplySorting(opt.Sort);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var s = opt.Search.Trim();
                q = q.Where(x =>
                    x.MarketName.Contains(s) ||
                    x.FloorName.Contains(s) ||
                    (x.UnitNumber != null && x.UnitNumber.Contains(s)) ||
                    x.ShopName.Contains(s));
            }

            var summary = await q.GroupBy(_ => 1).Select(g => new
            {
                TotalAccrued = g.Sum(x => (decimal?)x.TotalAccrued) ?? 0m,
                TotalPaid = g.Sum(x => (decimal?)x.TotalPaid) ?? 0m,
                TotalOutstanding = g.Sum(x => (decimal?)x.TotalOutstanding) ?? 0m
            }).FirstOrDefaultAsync();

            var total = await q.CountAsync();

            var page = await q
                .Skip((opt.Page - 1) * opt.PageSize)
                .Take(opt.PageSize)
                .ToListAsync();

            var dto = new TenantPeriodSummaryPageDto
            {
                Page = new PagedResult<TenantPeriodSummaryRowDto>
                {
                    Items = page,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                },
                TotalAccrued = summary?.TotalAccrued ?? 0m,
                TotalPaid = summary?.TotalPaid ?? 0m,
                TotalOutstanding = summary?.TotalOutstanding ?? 0m
            };

            return new SuccessDataResult<TenantPeriodSummaryPageDto>(dto);
        }

    }
}
