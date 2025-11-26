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

        public TenantManager(
            ITenantDal tenantDal,
            IMediaAssetDal mediaDal,
            IPhoneDal phoneDal,
            IAddressDal addrDal,
            IAccrualDal accrualDal)
        {
            _tenantDal = tenantDal;
            _mediaDal = mediaDal;
            _phoneDal = phoneDal;
            _addrDal = addrDal;
            _accrualDal = accrualDal;
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
    }
}
