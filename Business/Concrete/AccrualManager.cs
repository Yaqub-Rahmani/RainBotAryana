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
    public class AccrualManager : IAccrualService
    {
        private readonly IAccrualDal _accrualDal;
        private readonly IShopDal _shopDal;           // varsayım: mevcut
        private readonly ITenantDal _tenantDal;       // varsayım: mevcut
        private readonly IContractDal _contractDal;   // varsayım: mevcut
        private readonly IMeterReadingDal _meterDal;  // varsayım: mevcut

        public AccrualManager(
            IAccrualDal accrualDal,
            IShopDal shopDal,
            ITenantDal tenantDal,
            IContractDal contractDal,
            IMeterReadingDal meterDal)
        {
            _accrualDal = accrualDal;
            _shopDal = shopDal;
            _tenantDal = tenantDal;
            _contractDal = contractDal;
            _meterDal = meterDal;
        }

        // ------- READS -------

        [CacheAspect(10)]
        public IDataResult<List<Accrual>> GetAll()
            => new SuccessDataResult<List<Accrual>>(
                _accrualDal.QueryNoTracking()
                           .OrderByDescending(x => x.CreatedAtUtc)
                           .ToList()
            );

        [CacheAspect(10)]
        public IDataResult<Accrual> GetById(Guid id)
        {
            var a = _accrualDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return a == null
                ? new ErrorDataResult<Accrual>(null, "رکورد پیدا نشد.")
                : new SuccessDataResult<Accrual>(a);
        }

        [CacheAspect(10)]
        [CacheAspect(10)]
        public IDataResult<PagedResult<AccrualDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();

            var q = _accrualDal.QueryNoTracking();

            // Dinamik filtreler (shopId, tenantId, kind, status, year, month...)
            q = q.ApplyFilters(opt.Filters);

            // Arama (Notes üzerinde LIKE)
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                var like = $"%{key}%";
                q = q.Where(x => x.Notes != null && EF.Functions.Like(x.Notes!, like));
            }

            // Sıralama (yoksa varsayılan)
            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id);

            // --- Sunucu tarafı projeksiyon: sadece ham alanlar ---
            var qProj = q.Select(a => new
            {
                // Accrual
                a.Id,
                a.ShopId,
                a.TenantId,
                a.ContractId,
                a.MeterReadingId,
                a.Kind,
                a.Year,
                a.Month,
                a.CreatedAtUtc,
                a.Status,
                a.Amount,
                a.OutstandingAmount,
                a.Notes,

                // Relational raw fields (LEFT JOIN otomatik)
                ShopName = a.Shop != null ? a.Shop.Name : null,
                TenantName = a.Tenant != null ? a.Tenant.DisplayName : null,

                // Contract'ta Number yok → Version ya da Tarih aralığı
                ContractVersion = a.Contract != null ? a.Contract.Version : null,
                ContractStart = a.Contract != null ? (DateOnly?)a.Contract.StartDate : null,
                ContractEnd = a.Contract != null ? (DateOnly?)a.Contract.EndDate : null,

                // MeterReading'te ReadingNo yok → Year-Month başlığı
                MRYear = a.MeterReading != null ? (int?)a.MeterReading.Year : null,
                MRMonth = a.MeterReading != null ? (int?)a.MeterReading.Month : null
            });

            var total = qProj.Count();

            var rows = qProj
                .Skip((opt.Page - 1) * opt.PageSize)
                .Take(opt.PageSize)
                .ToList(); // DB’den ham veriler çekildi

            // --- Bellekte güvenli formatlama ve DTO inşası ---
            var items = rows.Select(r =>
            {
                // Contract görünen ad: öncelik Version, yoksa tarih aralığı
                string contractName =
                    !string.IsNullOrWhiteSpace(r.ContractVersion) ? $"v{r.ContractVersion}"
                    : (r.ContractStart.HasValue && r.ContractEnd.HasValue
                        ? $"{r.ContractStart:yyyy-MM-dd}→{r.ContractEnd:yyyy-MM-dd}"
                        : "");

                // MeterReading görünen ad: YYYY-MM
                string meterReadingName =
                    (r.MRYear.HasValue && r.MRMonth.HasValue)
                        ? $"{r.MRYear}-{r.MRMonth.Value.ToString().PadLeft(2, '0')}"
                        : "";

                return new AccrualDetailDto
                {
                    Id = r.Id,
                    ShopId = r.ShopId,
                    TenantId = r.TenantId,
                    ContractId = r.ContractId,
                    MeterReadingId = r.MeterReadingId,
                    Kind = r.Kind.ToString(),
                    Year = r.Year,
                    Month = r.Month,
                    CreatedAtUtc = r.CreatedAtUtc,
                    Status = r.Status.ToString(),
                    Amount = r.Amount,
                    OutstandingAmount = r.OutstandingAmount,
                    Notes = r.Notes,

                    Shop = new RefDto { Id = r.ShopId, Name = r.ShopName ?? "", Code = null },
                    Tenant = new RefDto { Id = r.TenantId, Name = r.TenantName ?? "", Code = null },
                    Contract = new RefDto { Id = r.ContractId, Name = contractName, Code = null },
                    MeterReading = r.MeterReadingId == null ? null :
                        new RefDto { Id = r.MeterReadingId.Value, Name = meterReadingName, Code = null }
                };
            }).ToList();

            var page = new PagedResult<AccrualDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            };

            return new SuccessDataResult<PagedResult<AccrualDetailDto>>(page);
        }


        [CacheAspect(10)]
        public IDataResult<AccrualDetailDto> GetDetailById(Guid id)
        {
            var data = _accrualDal.QueryNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    // Accrual fields
                    a.Id,
                    a.ShopId,
                    a.TenantId,
                    a.ContractId,
                    a.MeterReadingId,
                    a.Kind,
                    a.Year,
                    a.Month,
                    a.CreatedAtUtc,
                    a.Status,
                    a.Amount,
                    a.OutstandingAmount,
                    a.Notes,

                    // Shop/Tenant (left join)
                    ShopName = a.Shop != null ? a.Shop.Name : null,
                    TenantName = a.Tenant != null ? a.Tenant.DisplayName : null,

                    // Contract (left join)
                    ContractVersion = a.Contract != null ? a.Contract.Version : null,
                    ContractStart = a.Contract != null ? (DateOnly?)a.Contract.StartDate : null,
                    ContractEnd = a.Contract != null ? (DateOnly?)a.Contract.EndDate : null,

                    // MeterReading (left join)
                    MRYear = a.MeterReading != null ? (int?)a.MeterReading.Year : null,
                    MRMonth = a.MeterReading != null ? (int?)a.MeterReading.Month : null
                })
                .FirstOrDefault();

            if (data == null)
                return new ErrorDataResult<AccrualDetailDto>(null, "رکورد پیدا نشد.");

            // Bellekte güvenle formatla
            var contractName =
                data.ContractVersion?.Length > 0
                    ? $"v{data.ContractVersion}"
                    : (data.ContractStart.HasValue && data.ContractEnd.HasValue
                           ? $"{data.ContractStart:yyyy-MM-dd}→{data.ContractEnd:yyyy-MM-dd}"
                           : "");

            var meterReadingName =
                (data.MRYear.HasValue && data.MRMonth.HasValue)
                    ? $"{data.MRYear}-{data.MRMonth.Value.ToString().PadLeft(2, '0')}"
                    : "";

            var dto = new AccrualDetailDto
            {
                Id = data.Id,
                ShopId = data.ShopId,
                TenantId = data.TenantId,
                ContractId = data.ContractId,
                MeterReadingId = data.MeterReadingId,
                Kind = data.Kind.ToString(),
                Year = data.Year,
                Month = data.Month,
                CreatedAtUtc = data.CreatedAtUtc,
                Status = data.Status.ToString(),
                Amount = data.Amount,
                OutstandingAmount = data.OutstandingAmount,
                Notes = data.Notes,

                Shop = new RefDto { Id = data.ShopId, Name = data.ShopName ?? "", Code = null },
                Tenant = new RefDto { Id = data.TenantId, Name = data.TenantName ?? "", Code = null },
                Contract = new RefDto { Id = data.ContractId, Name = contractName, Code = null },
                MeterReading = data.MeterReadingId == null ? null :
                               new RefDto { Id = data.MeterReadingId.Value, Name = meterReadingName, Code = null }
            };

            return new SuccessDataResult<AccrualDetailDto>(dto);
        }

        // ------- WRITES -------

        [SecuredOperation("accrual.write,admin")]
        [ValidationAspect(typeof(AccrualValidator))]
        [CacheRemoveAspect("IAccrualService.Get")]
        public IResult Add(Accrual accrual)
        {
            if (accrual.Amount < 0) return new ErrorResult("مبلغ نمی‌تواند منفی باشد.");
            if (accrual.OutstandingAmount < 0) return new ErrorResult("بدهی باقیمانده نمی‌تواند منفی باشد.");

            // Varsayılan: Outstanding yoksa Amount’a eşitle
            if (accrual.OutstandingAmount == 0 && accrual.Amount > 0)
                accrual.InitializeOutstanding();

            _accrualDal.Add(accrual);
            return new SuccessResult("ثبت بدهی انجام شد.");
        }

        [SecuredOperation("accrual.write,admin")]
        [ValidationAspect(typeof(AccrualValidator))]
        [CacheRemoveAspect("IAccrualService.Get")]
        public IResult Update(Accrual accrual)
        {
            var current = _accrualDal.Get(x => x.Id == accrual.Id);
            if (current == null) return new ErrorResult("رکورد پیدا نشد.");

            // Temel alanlar
            current.ShopId = accrual.ShopId;
            current.TenantId = accrual.TenantId;
            current.ContractId = accrual.ContractId;
            current.MeterReadingId = accrual.MeterReadingId;
            current.Kind = accrual.Kind;
            current.Year = accrual.Year;
            current.Month = accrual.Month;
            current.Amount = accrual.Amount;
            current.Notes = accrual.Notes;

            // Outstanding mantığı: Amount düşerse üstte kalıyorsa kesmeyelim; manual edit’e izin veriyorsan doğrula
            if (accrual.OutstandingAmount >= 0)
                current.OutstandingAmount = accrual.OutstandingAmount;

            // Status otomatik ayar
            current.Status = current.OutstandingAmount == 0 ? AccrualStatus.Settled :
                             (current.OutstandingAmount == current.Amount ? AccrualStatus.Open : AccrualStatus.PartiallySettled);

            _accrualDal.Update(current);
            return new SuccessResult("ویرایش انجام شد.");
        }

        [SecuredOperation("accrual.write,admin")]
        [CacheRemoveAspect("IAccrualService.Get")]
        public IResult Delete(Guid id)
        {
            var entity = _accrualDal.Get(x => x.Id == id);
            if (entity == null) return new ErrorResult("یافت نشد.");

            _accrualDal.Delete(entity);
            return new SuccessResult("حذف انجام شد.");
        }

        [SecuredOperation("accrual.settle,admin")]
        [CacheRemoveAspect("IAccrualService.Get")]
        public IResult InitializeOutstanding(Guid id)
        {
            var e = _accrualDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.InitializeOutstanding();
            e.Status = e.OutstandingAmount == 0 ? AccrualStatus.Settled : AccrualStatus.PartiallySettled;
            _accrualDal.Update(e);
            return new SuccessResult("بدهی اولیه تنظیم شد.");
        }

        [SecuredOperation("accrual.settle,admin")]
        [CacheRemoveAspect("IAccrualService.Get")]
        public IResult ApplySettlement(Guid id, decimal amount)
        {
            if (amount <= 0) return new ErrorResult("مبلغ تسویه باید بزرگتر از صفر باشد.");

            var e = _accrualDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            if (e.OutstandingAmount == 0) return new ErrorResult("این بدهی قبلاً تسویه شده است.");

            try
            {
                e.ApplySettlement(amount);
                _accrualDal.Update(e);
                return new SuccessResult("تسویه با موفقیت انجام شد.");
            }
            catch (Exception)
            {
                return new ErrorResult("مبلغ تسویه نامعتبر است.");
            }
        }
    }
}
