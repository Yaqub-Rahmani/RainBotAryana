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

namespace Business.Concrete
{
    public class OwnerDistributionManager : IOwnerDistributionService
    {
        private readonly IOwnerDistributionDal _dal;

        public OwnerDistributionManager(IOwnerDistributionDal dal)
        {
            _dal = dal;
        }

        // ----- Helpers -----
        private OwnerDistributionDetailDto Project(OwnerDistribution d) => new()
        {
            Id = d.Id,
            ContractId = d.ContractId,
            CollectionId = d.CollectionId,
            CollectionAllocationId = d.CollectionAllocationId,
            OwnerAmountInput = d.OwnerAmountInput,
            RateTax = d.RateTax,
            RateZakat = d.RateZakat,
            RateCharity = d.RateCharity,
            RateOwnerReserve = d.RateOwnerReserve,
            RateFamilyPool = d.RateFamilyPool,
            AmountTax = d.AmountTax,
            AmountZakat = d.AmountZakat,
            AmountCharity = d.AmountCharity,
            AmountOwnerReserve = d.AmountOwnerReserve,
            AmountFamilyPool = d.AmountFamilyPool,
            OccurredAtUtc = d.OccurredAtUtc,
            Notes = d.Notes
        };

        // ----- Okumalar -----
        [CacheAspect(10)]
        public IDataResult<List<OwnerDistribution>> GetAll(Guid? contractId = null)
        {
            var q = _dal.QueryNoTracking();
            if (contractId.HasValue) q = q.Where(x => x.ContractId == contractId.Value);

            return new SuccessDataResult<List<OwnerDistribution>>(q.OrderByDescending(x => x.OccurredAtUtc).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<OwnerDistribution> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<OwnerDistribution>(null, "یافت نشد.")
                : new SuccessDataResult<OwnerDistribution>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<OwnerDistributionDetailDto>> GetDetails(QueryOptions opt, Guid? contractId = null)
        {
            opt ??= new QueryOptions();
            var q = _dal.QueryNoTracking();

            if (contractId.HasValue) q = q.Where(x => x.ContractId == contractId.Value);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.Notes ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.OccurredAtUtc);

            var items = q.Select(x => Project(x));

            var total = items.Count();
            var pageItems = items.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<OwnerDistributionDetailDto>>(
                new PagedResult<OwnerDistributionDetailDto>
                {
                    Items = pageItems,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<OwnerDistributionDetailDto> GetDetailById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<OwnerDistributionDetailDto>(null, "یافت نشد.")
                : new SuccessDataResult<OwnerDistributionDetailDto>(Project(e));
        }

        // ----- Yazmalar -----
        [SecuredOperation("ownerdistribution.write,admin")]
        [ValidationAspect(typeof(OwnerDistributionValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IOwnerDistributionService.Get")]
        public IResult Add(OwnerDistribution d)
        {
            // snapshot tutarları hesapla
            var totalRate = d.RateTax + d.RateZakat + d.RateCharity + d.RateOwnerReserve + d.RateFamilyPool;
            if (totalRate != 100m)
                return new ErrorResult("مجموع درصدها باید برابر ۱۰۰ باشد.");

            d.AmountTax = Math.Round(d.OwnerAmountInput * d.RateTax / 100m, 2);
            d.AmountZakat = Math.Round(d.OwnerAmountInput * d.RateZakat / 100m, 2);
            d.AmountCharity = Math.Round(d.OwnerAmountInput * d.RateCharity / 100m, 2);
            d.AmountOwnerReserve = Math.Round(d.OwnerAmountInput * d.RateOwnerReserve / 100m, 2);
            d.AmountFamilyPool = Math.Round(d.OwnerAmountInput * d.RateFamilyPool / 100m, 2);

            _dal.Add(d);
            return new SuccessResult("توزیع مالک ثبت شد.");
        }

        [SecuredOperation("ownerdistribution.write,admin")]
        [ValidationAspect(typeof(OwnerDistributionValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IOwnerDistributionService.Get")]
        public IResult Update(OwnerDistribution d)
        {
            var current = _dal.Get(x => x.Id == d.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            var totalRate = d.RateTax + d.RateZakat + d.RateCharity + d.RateOwnerReserve + d.RateFamilyPool;
            if (totalRate != 100m)
                return new ErrorResult("مجموع درصدها باید برابر ۱۰۰ باشد.");

            current.ContractId = d.ContractId;
            current.CollectionId = d.CollectionId;
            current.CollectionAllocationId = d.CollectionAllocationId;
            current.OwnerAmountInput = d.OwnerAmountInput;
            current.RateTax = d.RateTax;
            current.RateZakat = d.RateZakat;
            current.RateCharity = d.RateCharity;
            current.RateOwnerReserve = d.RateOwnerReserve;
            current.RateFamilyPool = d.RateFamilyPool;
            current.AmountTax = Math.Round(d.OwnerAmountInput * d.RateTax / 100m, 2);
            current.AmountZakat = Math.Round(d.OwnerAmountInput * d.RateZakat / 100m, 2);
            current.AmountCharity = Math.Round(d.OwnerAmountInput * d.RateCharity / 100m, 2);
            current.AmountOwnerReserve = Math.Round(d.OwnerAmountInput * d.RateOwnerReserve / 100m, 2);
            current.AmountFamilyPool = Math.Round(d.OwnerAmountInput * d.RateFamilyPool / 100m, 2);
            current.Notes = d.Notes;

            _dal.Update(current);
            return new SuccessResult("توزیع بروزرسانی شد.");
        }

        [SecuredOperation("ownerdistribution.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IOwnerDistributionService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
