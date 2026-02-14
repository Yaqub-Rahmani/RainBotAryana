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
    public class ReserveEntryManager : IReserveEntryService
    {
        private readonly IReserveEntryDal _entryDal;
        private readonly IReserveAccountDal _accDal;

        public ReserveEntryManager(IReserveEntryDal entryDal, IReserveAccountDal accDal)
        {
            _entryDal = entryDal;
            _accDal = accDal;
        }

        // ---------- Helpers ----------
        private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        private (decimal totalIn, decimal totalOut) SumForAccount(Guid accountId, DateTime? fromUtc = null, DateTime? toUtc = null, Guid? excludeEntryId = null)
        {
            var q = _entryDal.QueryNoTracking().Where(e => e.ReserveAccountId == accountId);
            if (fromUtc.HasValue) q = q.Where(e => e.OccurredAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) q = q.Where(e => e.OccurredAtUtc <= toUtc.Value);
            if (excludeEntryId.HasValue) q = q.Where(e => e.Id != excludeEntryId.Value);

            var totalIn = q.Where(e => e.Direction == BalanceDirection.In).Sum(e => (decimal?)e.Amount) ?? 0m;
            var totalOut = q.Where(e => e.Direction == BalanceDirection.Out).Sum(e => (decimal?)e.Amount) ?? 0m;
            return (totalIn, totalOut);
        }

        private void EnsureNoOverdraw(Guid accountId, decimal outAmount, Guid? excludeEntryId = null)
        {
            var (ti, to) = SumForAccount(accountId, null, null, excludeEntryId);
            var balance = ti - to;
            if (outAmount > balance)
                throw new InvalidOperationException("موجودی کافی برای خروجی وجود ندارد.");
        }

        private ReserveEntryDetailDto Project(ReserveEntry e)
        {
            var acc = _accDal.QueryNoTracking().FirstOrDefault(a => a.Id == e.ReserveAccountId);
            var title = acc?.DisplayName ?? acc?.Type.ToString() ?? "";
            var type = acc?.Type.ToString() ?? "";
            return new ReserveEntryDetailDto
            {
                Id = e.Id,
                ReserveAccountId = e.ReserveAccountId,
                AccountTitle = title,
                AccountType = type,
                Direction = e.Direction.ToString(),
                Amount = e.Amount,
                OccurredAtUtc = e.OccurredAtUtc,
                Description = e.Description,
                ExternalDocNo = e.ExternalDocNo,
                CollectionId = e.CollectionId,
                CollectionAllocationId = e.CollectionAllocationId,
                MeterReadingId = e.MeterReadingId,
                ContractId = e.ContractId
            };
        }

        // ---------- Okumalar ----------
        [CacheAspect(10)]
        public IDataResult<List<ReserveEntry>> GetAll(Guid? accountId = null, BalanceDirection? dir = null, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            var q = _entryDal.QueryNoTracking();
            if (accountId.HasValue && accountId.Value != Guid.Empty) q = q.Where(x => x.ReserveAccountId == accountId.Value);
            if (dir.HasValue) q = q.Where(x => x.Direction == dir.Value);
            if (fromUtc.HasValue) q = q.Where(x => x.OccurredAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) q = q.Where(x => x.OccurredAtUtc <= toUtc.Value);

            var list = q.OrderByDescending(x => x.OccurredAtUtc).ThenByDescending(x => x.Id).ToList();
            return new SuccessDataResult<List<ReserveEntry>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<ReserveEntry> GetById(Guid id)
        {
            var e = _entryDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<ReserveEntry>(null, "یافت نشد.") : new SuccessDataResult<ReserveEntry>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<ReserveEntryDetailDto>> GetDetails(QueryOptions opt, Guid? accountId = null, BalanceDirection? dir = null, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            opt ??= new QueryOptions();
            var q = _entryDal.QueryNoTracking();

            if (accountId.HasValue && accountId.Value != Guid.Empty) q = q.Where(x => x.ReserveAccountId == accountId.Value);
            if (dir.HasValue) q = q.Where(x => x.Direction == dir.Value);
            if (fromUtc.HasValue) q = q.Where(x => x.OccurredAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) q = q.Where(x => x.OccurredAtUtc <= toUtc.Value);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.Description ?? "").Contains(key) || (x.ExternalDocNo ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.OccurredAtUtc).ThenByDescending(x => x.Id);

            var total = q.Count();
            var pageItems = q.ApplyPaging(opt.Page, opt.PageSize).ToList().Select(Project).ToList();

            return new SuccessDataResult<PagedResult<ReserveEntryDetailDto>>(
                new PagedResult<ReserveEntryDetailDto>
                {
                    Items = pageItems,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<ReserveEntryDetailDto> GetDetailById(Guid id)
        {
            var e = _entryDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<ReserveEntryDetailDto>(null, "یافت نشد.")
                             : new SuccessDataResult<ReserveEntryDetailDto>(Project(e));
        }

        [CacheAspect(5)]
        public IDataResult<ReserveAccountBalanceDto> GetBalance(Guid accountId, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            var acc = _accDal.QueryNoTracking().FirstOrDefault(a => a.Id == accountId);
            if (acc == null) return new ErrorDataResult<ReserveAccountBalanceDto>(null, "حساب یافت نشد.");

            var (ti, to) = SumForAccount(accountId, fromUtc, toUtc);
            return new SuccessDataResult<ReserveAccountBalanceDto>(new ReserveAccountBalanceDto
            {
                ReserveAccountId = accountId,
                AccountTitle = acc.DisplayName ?? acc.Type.ToString(),
                AccountType = acc.Type.ToString(),
                TotalIn = ti,
                TotalOut = to,
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
        }

        // ---------- Yazmalar ----------
        [SecuredOperation("reserveentry.write,admin")]
        [ValidationAspect(typeof(ReserveEntryValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IReserveEntryService.Get")]
        public IResult Add(ReserveEntry entry, bool allowOverdraw = false)
        {
            if (_accDal.Get(x => x.Id == entry.ReserveAccountId) == null)
                return new ErrorResult("حساب رزرو یافت نشد.");

            entry.Amount = Round2(entry.Amount);

            if (entry.Direction == BalanceDirection.Out && !allowOverdraw)
                EnsureNoOverdraw(entry.ReserveAccountId, entry.Amount);

            _entryDal.Add(entry);
            return new SuccessResult("ثبت شد.");
        }

        [SecuredOperation("reserveentry.write,admin")]
        [ValidationAspect(typeof(ReserveEntryValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IReserveEntryService.Get")]
        public IResult Update(ReserveEntry entry, bool allowOverdraw = false)
        {
            var current = _entryDal.Get(x => x.Id == entry.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            if (_accDal.Get(x => x.Id == entry.ReserveAccountId) == null)
                return new ErrorResult("حساب رزرو یافت نشد.");

            // Overdraw check (mevcut kaydı hariç tut)
            if (entry.Direction == BalanceDirection.Out && !allowOverdraw)
            {
                var (ti, to) = SumForAccount(entry.ReserveAccountId, null, null, entry.Id);
                var balance = ti - to;
                if (entry.Amount > balance)
                    return new ErrorResult("موجودی کافی برای خروجی وجود ندارد.");
            }

            current.ReserveAccountId = entry.ReserveAccountId;
            current.Direction = entry.Direction;
            current.Amount = Round2(entry.Amount);
            current.OccurredAtUtc = entry.OccurredAtUtc;
            current.Description = entry.Description;
            current.CollectionId = entry.CollectionId;
            current.CollectionAllocationId = entry.CollectionAllocationId;
            current.MeterReadingId = entry.MeterReadingId;
            current.ContractId = entry.ContractId;
            current.ExternalDocNo = entry.ExternalDocNo;

            _entryDal.Update(current);
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("reserveentry.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IReserveEntryService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _entryDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _entryDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
