using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Validation;
using Core.Extensions;
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

namespace Business.Concrete
{
    public class ContractManager : IContractService
    {
        private readonly IContractDal _contractDal;
        private readonly IShopDal _shopDal;
        private readonly ITenantDal _tenantDal;

        public ContractManager(IContractDal contractDal, IShopDal shopDal, ITenantDal tenantDal)
        {
            _contractDal = contractDal;
            _shopDal = shopDal;
            _tenantDal = tenantDal;
        }

        // -------- Okumalar --------

        [CacheAspect(10)]
        public IDataResult<List<Contract>> GetAll()
            => new SuccessDataResult<List<Contract>>(
                _contractDal.QueryNoTracking()
                            .OrderByDescending(x => x.CreatedAtUtc)
                            .ToList()
            );

        [CacheAspect(10)]
        public IDataResult<Contract> GetById(Guid id)
        {
            var e = _contractDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<Contract>(null, "یافت نشد.") : new SuccessDataResult<Contract>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<ContractDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();
            var q = _contractDal.QueryNoTracking();

            // Basit filtreler: shopId=..., tenantId=..., status=Active/Archived, dueDayOfMonth=...
            q = q.ApplyFilters(opt.Filters);

            // Search: notlar ve versiyon
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(c => (c.ElectricityNote ?? "").Contains(key) ||
                                 (c.Version ?? "").Contains(key) ||
                                 (c.ArchiveReason ?? "").Contains(key));
            }

            // Sort
            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(c => c.StartDate);

            // Projeksiyon
            var projected =
                from c in q
                join s in _shopDal.QueryNoTracking() on c.ShopId equals s.Id
                join t in _tenantDal.QueryNoTracking() on c.TenantId equals t.Id
                select new ContractDetailDto
                {
                    Id = c.Id,
                    ShopId = c.ShopId,
                    ShopName = s.Name,
                    TenantId = c.TenantId,
                    TenantName = t.DisplayName,
                    SignedOn = c.SignedOn,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    DueDayOfMonth = c.DueDayOfMonth,
                    OwnerPortionAmount = c.OwnerPortionAmount,
                    FranchisePortionAmount = c.FranchisePortionAmount,
                    MarketPortionAmount = c.MarketPortionAmount,
                    TotalFixedRent = c.OwnerPortionAmount + c.FranchisePortionAmount + c.MarketPortionAmount,
                    ElectricityNote = c.ElectricityNote,
                    Status = c.Status.ToString(),
                    Version = c.Version,
                    ArchiveReason = c.ArchiveReason,
                    CreatedAtUtc = c.CreatedAtUtc,
                    UpdatedAtUtc = c.UpdatedAtUtc
                };

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<ContractDetailDto>>(
                new PagedResult<ContractDetailDto> { Items = items, Page = opt.Page, PageSize = opt.PageSize, TotalCount = total }
            );
        }

        [CacheAspect(10)]
        public IDataResult<ContractDetailDto> GetDetailById(Guid id)
        {
            var dto = (from c in _contractDal.QueryNoTracking().Where(x => x.Id == id)
                       join s in _shopDal.QueryNoTracking() on c.ShopId equals s.Id
                       join t in _tenantDal.QueryNoTracking() on c.TenantId equals t.Id
                       select new ContractDetailDto
                       {
                           Id = c.Id,
                           ShopId = c.ShopId,
                           ShopName = s.Name,
                           TenantId = c.TenantId,
                           TenantName = t.DisplayName,
                           SignedOn = c.SignedOn,
                           StartDate = c.StartDate,
                           EndDate = c.EndDate,
                           DueDayOfMonth = c.DueDayOfMonth,
                           OwnerPortionAmount = c.OwnerPortionAmount,
                           FranchisePortionAmount = c.FranchisePortionAmount,
                           MarketPortionAmount = c.MarketPortionAmount,
                           TotalFixedRent = c.OwnerPortionAmount + c.FranchisePortionAmount + c.MarketPortionAmount,
                           ElectricityNote = c.ElectricityNote,
                           Status = c.Status.ToString(),
                           Version = c.Version,
                           ArchiveReason = c.ArchiveReason,
                           CreatedAtUtc = c.CreatedAtUtc,
                           UpdatedAtUtc = c.UpdatedAtUtc
                       }).FirstOrDefault();

            return dto == null ? new ErrorDataResult<ContractDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<ContractDetailDto>(dto);
        }

        // -------- Yardımcı: Overlap kontrolü --------
        public IDataResult<bool> CheckOverlap(Guid shopId, Guid tenantId, DateOnly start, DateOnly end, Guid? excludeId = null)
        {
            var q = _contractDal.QueryNoTracking().Where(c => c.ShopId == shopId && c.TenantId == tenantId);
            if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);

            // [start,end] aralığı ile çakışan var mı?
            var has = q.Any(c => !(end < c.StartDate || start > c.EndDate));
            return new SuccessDataResult<bool>(has);
        }

        // -------- Yazmalar --------

        [SecuredOperation("contract.write,admin")]
        [ValidationAspect(typeof(ContractValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IContractService.Get")]
        public IResult Add(Contract contract)
        {
            if (_shopDal.Get(x => x.Id == contract.ShopId) == null)
                return new ErrorResult("فروشگاه یافت نشد.");
            if (_tenantDal.Get(x => x.Id == contract.TenantId) == null)
                return new ErrorResult("مستاجر یافت نشد.");

            // overlap kontrolü
            if (CheckOverlap(contract.ShopId, contract.TenantId, contract.StartDate, contract.EndDate).Data)
                return new ErrorResult("تداخل قرارداد در همین بازه زمانی وجود دارد.");

            contract.CreatedAtUtc = DateTime.UtcNow;
            _contractDal.Add(contract);
            return new SuccessResult("قرارداد ثبت شد.");
        }

        [SecuredOperation("contract.write,admin")]
        [ValidationAspect(typeof(ContractValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IContractService.Get")]
        public IResult Update(Contract contract)
        {
            var current = _contractDal.Get(x => x.Id == contract.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            // overlap kontrolü (kendisi hariç)
            if (CheckOverlap(contract.ShopId, contract.TenantId, contract.StartDate, contract.EndDate, contract.Id).Data)
                return new ErrorResult("تداخل قرارداد در همین بازه زمانی وجود دارد.");

            current.ShopId = contract.ShopId;
            current.TenantId = contract.TenantId;
            current.SignedOn = contract.SignedOn;
            current.StartDate = contract.StartDate;
            current.EndDate = contract.EndDate;
            current.DueDayOfMonth = contract.DueDayOfMonth;
            current.OwnerPortionAmount = contract.OwnerPortionAmount;
            current.FranchisePortionAmount = contract.FranchisePortionAmount;
            current.MarketPortionAmount = contract.MarketPortionAmount;
            current.ElectricityNote = contract.ElectricityNote;
            current.Status = contract.Status;
            current.Version = contract.Version;
            current.ArchiveReason = contract.ArchiveReason;
            current.UpdatedAtUtc = DateTime.UtcNow;

            _contractDal.Update(current);
            return new SuccessResult("قرارداد بروزرسانی شد.");
        }

        [SecuredOperation("contract.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IContractService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _contractDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _contractDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        [SecuredOperation("contract.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IContractService.Get")]
        public IResult Archive(Guid id, string? reason)
        {
            var e = _contractDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            e.Status = ContractStatus.Archived;
            e.ArchiveReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            e.UpdatedAtUtc = DateTime.UtcNow;

            _contractDal.Update(e);
            return new SuccessResult("قرارداد آرشیو شد.");
        }

        [SecuredOperation("contract.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IContractService.Get")]
        public IResult Activate(Guid id)
        {
            var e = _contractDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            e.Status = ContractStatus.Active;
            e.ArchiveReason = null;
            e.UpdatedAtUtc = DateTime.UtcNow;

            _contractDal.Update(e);
            return new SuccessResult("قرارداد فعال شد.");
        }
    }
}
