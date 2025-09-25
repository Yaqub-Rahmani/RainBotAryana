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
    public class CollectionManager : ICollectionService
    {
        private readonly ICollectionDal _collectionDal;
        private readonly ITenantDal _tenantDal;
        private readonly IShopDal _shopDal;

        public CollectionManager(ICollectionDal collectionDal, ITenantDal tenantDal, IShopDal shopDal)
        {
            _collectionDal = collectionDal;
            _tenantDal = tenantDal;
            _shopDal = shopDal;
        }

        // ---------- Okumalar ----------
        [CacheAspect(10)]
        public IDataResult<List<Collection>> GetAll()
            => new SuccessDataResult<List<Collection>>(
                _collectionDal.QueryNoTracking().OrderByDescending(x => x.PaidAtUtc).ToList()
            );

        [CacheAspect(10)]
        public IDataResult<Collection> GetById(Guid id)
        {
            var e = _collectionDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<Collection>(null, "یافت نشد.") : new SuccessDataResult<Collection>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<CollectionDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();
            var q = _collectionDal.QueryNoTracking();

            q = q.ApplyFilters(opt.Filters);
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(c => (c.Notes ?? "").Contains(key) || (c.ReferenceNo ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(c => c.PaidAtUtc);

            var projected = from c in q
                            join t in _tenantDal.QueryNoTracking() on c.TenantId equals t.Id
                            join s in _shopDal.QueryNoTracking() on c.ShopId equals s.Id
                            select new CollectionDetailDto
                            {
                                Id = c.Id,
                                TenantId = c.TenantId,
                                TenantName = t.DisplayName,
                                ShopId = c.ShopId,
                                ShopName = s.Name,
                                ContractId = c.ContractId,
                                PaidAtUtc = c.PaidAtUtc,
                                AmountReceived = c.AmountReceived,
                                Method = c.Method.ToString(),
                                ReferenceNo = c.ReferenceNo,
                                TargetYear = c.TargetYear,
                                TargetMonth = c.TargetMonth,
                                Notes = c.Notes,
                                AllocationCount = c.Allocations.Count
                            };

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            var page = new PagedResult<CollectionDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            };
            return new SuccessDataResult<PagedResult<CollectionDetailDto>>(page);
        }

        [CacheAspect(10)]
        public IDataResult<CollectionDetailDto> GetDetailById(Guid id)
        {
            var dto = (from c in _collectionDal.QueryNoTracking().Where(x => x.Id == id)
                       join t in _tenantDal.QueryNoTracking() on c.TenantId equals t.Id
                       join s in _shopDal.QueryNoTracking() on c.ShopId equals s.Id
                       select new CollectionDetailDto
                       {
                           Id = c.Id,
                           TenantId = c.TenantId,
                           TenantName = t.DisplayName,
                           ShopId = c.ShopId,
                           ShopName = s.Name,
                           ContractId = c.ContractId,
                           PaidAtUtc = c.PaidAtUtc,
                           AmountReceived = c.AmountReceived,
                           Method = c.Method.ToString(),
                           ReferenceNo = c.ReferenceNo,
                           TargetYear = c.TargetYear,
                           TargetMonth = c.TargetMonth,
                           Notes = c.Notes,
                           AllocationCount = c.Allocations.Count
                       }).FirstOrDefault();

            return dto == null ? new ErrorDataResult<CollectionDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<CollectionDetailDto>(dto);
        }

        // ---------- Yazmalar ----------
        [SecuredOperation("collection.write,admin")]
        [ValidationAspect(typeof(CollectionValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ICollectionService.Get")]
        public IResult Add(Collection collection)
        {
            if (_tenantDal.Get(x => x.Id == collection.TenantId) == null)
                return new ErrorResult("مستاجر یافت نشد.");
            if (_shopDal.Get(x => x.Id == collection.ShopId) == null)
                return new ErrorResult("فروشگاه یافت نشد.");

            _collectionDal.Add(collection);
            return new SuccessResult("دریافت وجه ثبت شد.");
        }

        [SecuredOperation("collection.write,admin")]
        [ValidationAspect(typeof(CollectionValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ICollectionService.Get")]
        public IResult Update(Collection collection)
        {
            var current = _collectionDal.Get(x => x.Id == collection.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            current.TenantId = collection.TenantId;
            current.ShopId = collection.ShopId;
            current.ContractId = collection.ContractId;
            current.PaidAtUtc = collection.PaidAtUtc;
            current.AmountReceived = collection.AmountReceived;
            current.Method = collection.Method;
            current.ReferenceNo = collection.ReferenceNo;
            current.TargetYear = collection.TargetYear;
            current.TargetMonth = collection.TargetMonth;
            current.Notes = collection.Notes;

            _collectionDal.Update(current);
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("collection.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ICollectionService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _collectionDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _collectionDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
