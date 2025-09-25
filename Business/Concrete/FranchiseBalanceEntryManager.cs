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
    public class FranchiseBalanceEntryManager : IFranchiseBalanceEntryService
    {
        private readonly IFranchiseBalanceEntryDal _entryDal;
        private readonly IFranchiseOwnerDal _ownerDal;

        public FranchiseBalanceEntryManager(IFranchiseBalanceEntryDal entryDal, IFranchiseOwnerDal ownerDal)
        {
            _entryDal = entryDal;
            _ownerDal = ownerDal;
        }

        // ------ Okumalar ------

        [CacheAspect(10)]
        public IDataResult<List<FranchiseBalanceEntry>> GetAll(Guid? ownerId = null)
        {
            var q = _entryDal.QueryNoTracking();
            if (ownerId.HasValue && ownerId.Value != Guid.Empty)
                q = q.Where(x => x.FranchiseOwnerId == ownerId.Value);

            return new SuccessDataResult<List<FranchiseBalanceEntry>>(
                q.OrderByDescending(x => x.OccurredAtUtc).ToList()
            );
        }

        [CacheAspect(10)]
        public IDataResult<FranchiseBalanceEntry> GetById(Guid id)
        {
            var e = _entryDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<FranchiseBalanceEntry>(null, "یافت نشد.")
                             : new SuccessDataResult<FranchiseBalanceEntry>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<FranchiseBalanceEntryDetailDto>> GetDetails(QueryOptions opt, Guid? ownerId = null)
        {
            opt ??= new QueryOptions();
            var q = _entryDal.QueryNoTracking();

            if (ownerId.HasValue && ownerId.Value != Guid.Empty)
                q = q.Where(x => x.FranchiseOwnerId == ownerId.Value);

            // Filters: periodYear, periodMonth, shopId, contractId, direction, from/to, etc.
            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.Notes ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.OccurredAtUtc);

            var owners = _ownerDal.QueryNoTracking();
            var projected =
                from e in q
                join o in owners on e.FranchiseOwnerId equals o.Id
                select new FranchiseBalanceEntryDetailDto
                {
                    Id = e.Id,
                    FranchiseOwnerId = e.FranchiseOwnerId,
                    FranchiseOwnerName = o.DisplayName, // Name kullanıyorsan burayı değiştir
                    Direction = e.Direction.ToString(),
                    Amount = e.Amount,
                    OccurredAtUtc = e.OccurredAtUtc,
                    CollectionId = e.CollectionId,
                    CollectionAllocationId = e.CollectionAllocationId,
                    ContractId = e.ContractId,
                    ShopId = e.ShopId,
                    PeriodYear = e.PeriodYear,
                    PeriodMonth = e.PeriodMonth,
                    Notes = e.Notes
                };

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<FranchiseBalanceEntryDetailDto>>(
                new PagedResult<FranchiseBalanceEntryDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<FranchiseBalanceEntryDetailDto> GetDetailById(Guid id)
        {
            var dto = (from e in _entryDal.QueryNoTracking().Where(x => x.Id == id)
                       join o in _ownerDal.QueryNoTracking() on e.FranchiseOwnerId equals o.Id
                       select new FranchiseBalanceEntryDetailDto
                       {
                           Id = e.Id,
                           FranchiseOwnerId = e.FranchiseOwnerId,
                           FranchiseOwnerName = o.DisplayName,
                           Direction = e.Direction.ToString(),
                           Amount = e.Amount,
                           OccurredAtUtc = e.OccurredAtUtc,
                           CollectionId = e.CollectionId,
                           CollectionAllocationId = e.CollectionAllocationId,
                           ContractId = e.ContractId,
                           ShopId = e.ShopId,
                           PeriodYear = e.PeriodYear,
                           PeriodMonth = e.PeriodMonth,
                           Notes = e.Notes
                       }).FirstOrDefault();

            return dto == null ? new ErrorDataResult<FranchiseBalanceEntryDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<FranchiseBalanceEntryDetailDto>(dto);
        }

        // ------ Özet / Rapor ------

        [CacheAspect(5)]
        public IDataResult<List<FranchiseBalanceSummaryDto>> GetSummary(Guid? ownerId = null, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            var q = _entryDal.QueryNoTracking();

            if (ownerId.HasValue && ownerId.Value != Guid.Empty)
                q = q.Where(x => x.FranchiseOwnerId == ownerId.Value);

            if (fromUtc.HasValue) q = q.Where(x => x.OccurredAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) q = q.Where(x => x.OccurredAtUtc <= toUtc.Value);

            var grouped = from e in q
                          group e by e.FranchiseOwnerId into g
                          select new
                          {
                              FranchiseOwnerId = g.Key,
                              TotalIn = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                              TotalOut = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                          };

            var owners = _ownerDal.QueryNoTracking().ToList();
            var list = grouped.AsEnumerable()
                .Select(x => new FranchiseBalanceSummaryDto
                {
                    FranchiseOwnerId = x.FranchiseOwnerId,
                    FranchiseOwnerName = owners.FirstOrDefault(o => o.Id == x.FranchiseOwnerId)?.DisplayName ?? "",
                    TotalIn = x.TotalIn,
                    TotalOut = x.TotalOut
                })
                .OrderByDescending(x => x.Balance)
                .ToList();

            return new SuccessDataResult<List<FranchiseBalanceSummaryDto>>(list);
        }

        // ------ Yazmalar ------

        [SecuredOperation("franchisebalance.write,admin")]
        [ValidationAspect(typeof(FranchiseBalanceEntryValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseBalanceEntryService.Get")]
        public IResult Add(FranchiseBalanceEntry entry)
        {
            if (_ownerDal.Get(x => x.Id == entry.FranchiseOwnerId) == null)
                return new ErrorResult("صاحب امتیاز یافت نشد.");

            _entryDal.Add(entry);
            return new SuccessResult("ورودی تراز امتیاز ثبت شد.");
        }

        [SecuredOperation("franchisebalance.write,admin")]
        [ValidationAspect(typeof(FranchiseBalanceEntryValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseBalanceEntryService.Get")]
        public IResult Update(FranchiseBalanceEntry entry)
        {
            var current = _entryDal.Get(x => x.Id == entry.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            if (_ownerDal.Get(x => x.Id == entry.FranchiseOwnerId) == null)
                return new ErrorResult("صاحب امتیاز یافت نشد.");

            current.FranchiseOwnerId = entry.FranchiseOwnerId;
            current.Direction = entry.Direction;
            current.Amount = entry.Amount;
            current.OccurredAtUtc = entry.OccurredAtUtc;
            current.CollectionId = entry.CollectionId;
            current.CollectionAllocationId = entry.CollectionAllocationId;
            current.ContractId = entry.ContractId;
            current.ShopId = entry.ShopId;
            current.PeriodYear = entry.PeriodYear;
            current.PeriodMonth = entry.PeriodMonth;
            current.Notes = entry.Notes;

            _entryDal.Update(current);
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("franchisebalance.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseBalanceEntryService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _entryDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            _entryDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
