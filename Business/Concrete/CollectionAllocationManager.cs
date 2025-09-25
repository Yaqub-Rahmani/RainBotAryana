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
    public class CollectionAllocationManager : ICollectionAllocationService
    {
        private readonly ICollectionAllocationDal _allocationDal;
        private readonly IShopDal _shopDal;
        private readonly ITenantDal _tenantDal;

        public CollectionAllocationManager(ICollectionAllocationDal allocationDal, IShopDal shopDal, ITenantDal tenantDal)
        {
            _allocationDal = allocationDal;
            _shopDal = shopDal;
            _tenantDal = tenantDal;
        }

        [CacheAspect(10)]
        public IDataResult<List<CollectionAllocation>> GetAll()
            => new SuccessDataResult<List<CollectionAllocation>>(
                _allocationDal.QueryNoTracking().OrderByDescending(x => x.Id).ToList()
            );

        [CacheAspect(10)]
        public IDataResult<CollectionAllocation> GetById(Guid id)
        {
            var e = _allocationDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<CollectionAllocation>(null, "یافت نشد.")
                             : new SuccessDataResult<CollectionAllocation>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<CollectionAllocationDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();
            var q = _allocationDal.QueryNoTracking();

            q = q.ApplyFilters(opt.Filters);
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(a => (a.Notes ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(a => a.Id);

            var projected = from a in q
                            join s in _shopDal.QueryNoTracking() on a.ShopId equals s.Id into shopj
                            from sj in shopj.DefaultIfEmpty()
                            join t in _tenantDal.QueryNoTracking() on a.TenantId equals t.Id into tenj
                            from tj in tenj.DefaultIfEmpty()
                            select new CollectionAllocationDetailDto
                            {
                                Id = a.Id,
                                CollectionId = a.CollectionId,
                                Kind = a.Kind.ToString(),
                                Amount = a.Amount,
                                ContractId = a.ContractId,
                                ShopId = a.ShopId,
                                TenantId = a.TenantId,
                                MeterReadingId = a.MeterReadingId,
                                PeriodYear = a.PeriodYear,
                                PeriodMonth = a.PeriodMonth,
                                Notes = a.Notes,
                                ShopName = sj != null ? sj.Name : null,
                                TenantName = tj != null ? tj.DisplayName : null
                            };

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<CollectionAllocationDetailDto>>(
                new PagedResult<CollectionAllocationDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<CollectionAllocationDetailDto> GetDetailById(Guid id)
        {
            var dto = (from a in _allocationDal.QueryNoTracking().Where(x => x.Id == id)
                       join s in _shopDal.QueryNoTracking() on a.ShopId equals s.Id into shopj
                       from sj in shopj.DefaultIfEmpty()
                       join t in _tenantDal.QueryNoTracking() on a.TenantId equals t.Id into tenj
                       from tj in tenj.DefaultIfEmpty()
                       select new CollectionAllocationDetailDto
                       {
                           Id = a.Id,
                           CollectionId = a.CollectionId,
                           Kind = a.Kind.ToString(),
                           Amount = a.Amount,
                           ContractId = a.ContractId,
                           ShopId = a.ShopId,
                           TenantId = a.TenantId,
                           MeterReadingId = a.MeterReadingId,
                           PeriodYear = a.PeriodYear,
                           PeriodMonth = a.PeriodMonth,
                           Notes = a.Notes,
                           ShopName = sj != null ? sj.Name : null,
                           TenantName = tj != null ? tj.DisplayName : null
                       }).FirstOrDefault();

            return dto == null ? new ErrorDataResult<CollectionAllocationDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<CollectionAllocationDetailDto>(dto);
        }

        [SecuredOperation("collectionallocation.write,admin")]
        [ValidationAspect(typeof(CollectionAllocationValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ICollectionAllocationService.Get")]
        public IResult Add(CollectionAllocation allocation)
        {
            _allocationDal.Add(allocation);
            return new SuccessResult("تخصیص ثبت شد.");
        }

        [SecuredOperation("collectionallocation.write,admin")]
        [ValidationAspect(typeof(CollectionAllocationValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ICollectionAllocationService.Get")]
        public IResult Update(CollectionAllocation allocation)
        {
            var current = _allocationDal.Get(x => x.Id == allocation.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            current.CollectionId = allocation.CollectionId;
            current.Kind = allocation.Kind;
            current.Amount = allocation.Amount;
            current.ContractId = allocation.ContractId;
            current.ShopId = allocation.ShopId;
            current.TenantId = allocation.TenantId;
            current.MeterReadingId = allocation.MeterReadingId;
            current.PeriodYear = allocation.PeriodYear;
            current.PeriodMonth = allocation.PeriodMonth;
            current.Notes = allocation.Notes;

            _allocationDal.Update(current);
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("collectionallocation.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ICollectionAllocationService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _allocationDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _allocationDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
