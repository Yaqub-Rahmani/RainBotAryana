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
    public class FamilyBalanceEntryManager : IFamilyBalanceEntryService
    {
        private readonly IFamilyBalanceEntryDal _entryDal;
        private readonly IFamilyMemberDal _memberDal;

        public FamilyBalanceEntryManager(IFamilyBalanceEntryDal entryDal, IFamilyMemberDal memberDal)
        {
            _entryDal = entryDal;
            _memberDal = memberDal;
        }

        // ------ Okumalar ------

        [CacheAspect(10)]
        public IDataResult<List<FamilyBalanceEntry>> GetAll()
            => new SuccessDataResult<List<FamilyBalanceEntry>>(
                _entryDal.QueryNoTracking().OrderByDescending(x => x.OccurredAtUtc).ToList()
            );

        [CacheAspect(10)]
        public IDataResult<FamilyBalanceEntry> GetById(Guid id)
        {
            var e = _entryDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<FamilyBalanceEntry>(null, "یافت نشد.") : new SuccessDataResult<FamilyBalanceEntry>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<FamilyBalanceEntryDetailDto>> GetDetails(QueryOptions opt, Guid? familyMemberId = null)
        {
            opt ??= new QueryOptions();
            var q = _entryDal.QueryNoTracking();

            if (familyMemberId.HasValue && familyMemberId.Value != Guid.Empty)
                q = q.Where(x => x.FamilyMemberId == familyMemberId.Value);

            // Tarih aralığı filtreleri QueryOptions.Filters ile de verilebilir: "from=2025-01-01"
            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.Notes ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.OccurredAtUtc);

            var projected =
                from e in q
                join m in _memberDal.QueryNoTracking() on e.FamilyMemberId equals m.Id
                select new FamilyBalanceEntryDetailDto
                {
                    Id = e.Id,
                    FamilyMemberId = e.FamilyMemberId,
                    FamilyMemberName = m.DisplayName,
                    Direction = e.Direction.ToString(),
                    Amount = e.Amount,
                    OccurredAtUtc = e.OccurredAtUtc,
                    FamilyPayoutId = e.FamilyPayoutId,
                    OwnerDistributionId = e.OwnerDistributionId,
                    CollectionId = e.CollectionId,
                    ContractId = e.ContractId,
                    Notes = e.Notes
                };

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<FamilyBalanceEntryDetailDto>>(
                new PagedResult<FamilyBalanceEntryDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<FamilyBalanceEntryDetailDto> GetDetailById(Guid id)
        {
            var dto = (from e in _entryDal.QueryNoTracking().Where(x => x.Id == id)
                       join m in _memberDal.QueryNoTracking() on e.FamilyMemberId equals m.Id
                       select new FamilyBalanceEntryDetailDto
                       {
                           Id = e.Id,
                           FamilyMemberId = e.FamilyMemberId,
                           FamilyMemberName = m.DisplayName,
                           Direction = e.Direction.ToString(),
                           Amount = e.Amount,
                           OccurredAtUtc = e.OccurredAtUtc,
                           FamilyPayoutId = e.FamilyPayoutId,
                           OwnerDistributionId = e.OwnerDistributionId,
                           CollectionId = e.CollectionId,
                           ContractId = e.ContractId,
                           Notes = e.Notes
                       }).FirstOrDefault();

            return dto == null ? new ErrorDataResult<FamilyBalanceEntryDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<FamilyBalanceEntryDetailDto>(dto);
        }

        // ------ Özet / Rapor ------

        [CacheAspect(5)]
        public IDataResult<List<FamilyBalanceSummaryDto>> GetSummary(Guid? familyMemberId = null, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            var q = _entryDal.QueryNoTracking();

            if (familyMemberId.HasValue && familyMemberId.Value != Guid.Empty)
                q = q.Where(x => x.FamilyMemberId == familyMemberId.Value);

            if (fromUtc.HasValue) q = q.Where(x => x.OccurredAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) q = q.Where(x => x.OccurredAtUtc <= toUtc.Value);

            var grouped = from e in q
                          group e by e.FamilyMemberId into g
                          select new
                          {
                              FamilyMemberId = g.Key,
                              TotalIn = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                              TotalOut = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                          };

            var members = _memberDal.QueryNoTracking().ToList();
            var result = grouped
                .AsEnumerable()
                .Select(x => new FamilyBalanceSummaryDto
                {
                    FamilyMemberId = x.FamilyMemberId,
                    FamilyMemberName = members.FirstOrDefault(m => m.Id == x.FamilyMemberId)?.DisplayName ?? "",
                    TotalIn = x.TotalIn,
                    TotalOut = x.TotalOut
                })
                .OrderByDescending(x => x.Balance)
                .ToList();

            return new SuccessDataResult<List<FamilyBalanceSummaryDto>>(result);
        }

        // ------ Yazmalar ------

        [SecuredOperation("familybalance.write,admin")]
        [ValidationAspect(typeof(FamilyBalanceEntryValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyBalanceEntryService.Get")]
        public IResult Add(FamilyBalanceEntry entry)
        {
            if (_memberDal.Get(x => x.Id == entry.FamilyMemberId) == null)
                return new ErrorResult("عضو خانواده یافت نشد.");

            // Not: Amount > 0; yön In/Out zaten DTO’da.
            _entryDal.Add(entry);
            return new SuccessResult("ورودی تراز خانواده ثبت شد.");
        }

        [SecuredOperation("familybalance.write,admin")]
        [ValidationAspect(typeof(FamilyBalanceEntryValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyBalanceEntryService.Get")]
        public IResult Update(FamilyBalanceEntry entry)
        {
            var current = _entryDal.Get(x => x.Id == entry.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            if (_memberDal.Get(x => x.Id == entry.FamilyMemberId) == null)
                return new ErrorResult("عضو خانواده یافت نشد.");

            current.FamilyMemberId = entry.FamilyMemberId;
            current.Direction = entry.Direction;
            current.Amount = entry.Amount;
            current.OccurredAtUtc = entry.OccurredAtUtc;
            current.FamilyPayoutId = entry.FamilyPayoutId;
            current.OwnerDistributionId = entry.OwnerDistributionId;
            current.CollectionId = entry.CollectionId;
            current.ContractId = entry.ContractId;
            current.Notes = entry.Notes;

            _entryDal.Update(current);
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("familybalance.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyBalanceEntryService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _entryDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _entryDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
