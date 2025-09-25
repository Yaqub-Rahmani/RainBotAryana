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
    public class ReserveAccountManager : IReserveAccountService
    {
        private readonly IReserveAccountDal _accDal;
        private readonly IReserveEntryDal _entryDal; // varsayım: mevcut (ReserveEntry için)

        public ReserveAccountManager(IReserveAccountDal accDal, IReserveEntryDal entryDal)
        {
            _accDal = accDal;
            _entryDal = entryDal;
        }

        // -------- Helpers --------
        private static string TypeName(ReserveAccountType t) => t.ToString();

        private ReserveAccountDetailDto Project(ReserveAccount a, DateTime? from = null, DateTime? to = null)
        {
            var entries = _entryDal.QueryNoTracking().Where(e => e.ReserveAccountId == a.Id);
            if (from.HasValue) entries = entries.Where(e => e.OccurredAtUtc >= from.Value);
            if (to.HasValue) entries = entries.Where(e => e.OccurredAtUtc <= to.Value);

            var totalIn = entries.Where(e => e.Direction == ReserveEntryDirection.In).Sum(e => (decimal?)e.Amount) ?? 0m;
            var totalOut = entries.Where(e => e.Direction == ReserveEntryDirection.Out).Sum(e => (decimal?)e.Amount) ?? 0m;

            return new ReserveAccountDetailDto
            {
                Id = a.Id,
                Type = TypeName(a.Type),
                DisplayName = a.DisplayName,
                IsActive = a.IsActive,
                TotalIn = totalIn,
                TotalOut = totalOut
            };
        }

        // -------- Okumalar --------
        [CacheAspect(10)]
        public IDataResult<List<ReserveAccount>> GetAll(ReserveAccountType? type = null, bool? active = null)
        {
            var q = _accDal.QueryNoTracking();
            if (type.HasValue) q = q.Where(x => x.Type == type.Value);
            if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);
            return new SuccessDataResult<List<ReserveAccount>>(q.OrderBy(x => x.Type).ThenBy(x => x.DisplayName).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<ReserveAccount> GetById(Guid id)
        {
            var e = _accDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<ReserveAccount>(null, "یافت نشد.") : new SuccessDataResult<ReserveAccount>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<ReserveAccountDetailDto>> GetDetails(QueryOptions opt, ReserveAccountType? type = null, bool? active = null)
        {
            opt ??= new QueryOptions();
            var q = _accDal.QueryNoTracking();

            if (type.HasValue) q = q.Where(x => x.Type == type.Value);
            if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x =>
                    (x.DisplayName ?? "").Contains(key) ||
                    x.Type.ToString().Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderBy(x => x.Type).ThenBy(x => x.DisplayName);

            // Not: Detay DTO’sunda entry özetleri var; paging uygularız
            var total = q.Count();
            var pageAccs = q.ApplyPaging(opt.Page, opt.PageSize).ToList();

            // (İsteğe bağlı) tarih filtresi gönderilirse opt.Filters içerisine koyabilirsin; burada sade tuttum.
            var items = pageAccs.Select(a => Project(a)).ToList();

            return new SuccessDataResult<PagedResult<ReserveAccountDetailDto>>(
                new PagedResult<ReserveAccountDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<ReserveAccountDetailDto> GetDetailById(Guid id)
        {
            var a = _accDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (a == null) return new ErrorDataResult<ReserveAccountDetailDto>(null, "یافت نشد.");
            return new SuccessDataResult<ReserveAccountDetailDto>(Project(a));
        }

        // -------- Özet / Rapor --------
        [CacheAspect(5)]
        public IDataResult<List<ReserveAccountSummaryDto>> GetBalanceByAccount(DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            var accounts = _accDal.QueryNoTracking().ToList();
            var entries = _entryDal.QueryNoTracking();

            if (fromUtc.HasValue) entries = entries.Where(e => e.OccurredAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) entries = entries.Where(e => e.OccurredAtUtc <= toUtc.Value);

            var grouped =
                from e in entries
                group e by e.ReserveAccountId into g
                select new
                {
                    AccountId = g.Key,
                    TotalIn = g.Where(x => x.Direction == ReserveEntryDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                    TotalOut = g.Where(x => x.Direction == ReserveEntryDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
        };

            var list = grouped.AsEnumerable().Select(x =>
            {
                var acc = accounts.FirstOrDefault(a => a.Id == x.AccountId);
                var title = acc?.DisplayName ?? acc?.Type.ToString() ?? "(unknown)";
                return new ReserveAccountSummaryDto
                {
                    AccountId = x.AccountId,
                    GroupKey = title,
                    TotalIn = x.TotalIn,
                    TotalOut = x.TotalOut
                };
            })
            .OrderByDescending(x => x.Balance)
            .ToList();

            return new SuccessDataResult<List<ReserveAccountSummaryDto>>(list);
        }

        [CacheAspect(5)]
        public IDataResult<List<ReserveAccountSummaryDto>> GetBalanceByType(DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            var entries = _entryDal.QueryNoTracking();

            if (fromUtc.HasValue) entries = entries.Where(e => e.OccurredAtUtc >= fromUtc.Value);
            if (toUtc.HasValue) entries = entries.Where(e => e.OccurredAtUtc <= toUtc.Value);

            // Account join → type’a göre grupla
            var accs = _accDal.QueryNoTracking();

            var q =
                from e in entries
                join a in accs on e.ReserveAccountId equals a.Id
                group new { e, a } by a.Type into g
                select new
                {
                    Type = g.Key,
                    TotalIn = g.Where(x => x.e.Direction == ReserveEntryDirection.In).Sum(x => (decimal?)x.e.Amount) ?? 0m,
                    TotalOut = g.Where(x => x.e.Direction == ReserveEntryDirection.Out).Sum(x => (decimal?)x.e.Amount) ?? 0m
                };

            var list = q.AsEnumerable()
                        .Select(x => new ReserveAccountSummaryDto
                        {
                            AccountId = null,
                            GroupKey = x.Type.ToString(),
                            TotalIn = x.TotalIn,
                            TotalOut = x.TotalOut
                        })
                        .OrderByDescending(x => x.Balance)
                        .ToList();

            return new SuccessDataResult<List<ReserveAccountSummaryDto>>(list);
        }

        // -------- Yazmalar --------
        [SecuredOperation("reserveaccount.write,admin")]
        [ValidationAspect(typeof(ReserveAccountValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IReserveAccountService.Get")]
        public IResult Add(ReserveAccount account)
        {
            // (Opsiyonel) aynı tipte aynı isimde aktif hesap engeli
            var dup = _accDal.Get(x => x.IsActive
                                    && x.Type == account.Type
                                    && ((x.DisplayName ?? "").ToLower() == (account.DisplayName ?? "").ToLower()));
            if (dup != null) return new ErrorResult("حساب فعالی با همین نوع و عنوان وجود دارد.");

            _accDal.Add(account);
            return new SuccessResult("حساب رزرو ایجاد شد.");
        }

        [SecuredOperation("reserveaccount.write,admin")]
        [ValidationAspect(typeof(ReserveAccountValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IReserveAccountService.Get")]
        public IResult Update(ReserveAccount account)
        {
            var current = _accDal.Get(x => x.Id == account.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            // (Opsiyonel) çakışma kontrolü
            var dup = _accDal.Get(x => x.Id != account.Id && x.IsActive
                                    && x.Type == account.Type
                                    && ((x.DisplayName ?? "").ToLower() == (account.DisplayName ?? "").ToLower()));
            if (dup != null) return new ErrorResult("حساب فعالی با همین نوع و عنوان وجود دارد.");

            current.Type = account.Type;
            current.DisplayName = account.DisplayName;
            current.IsActive = account.IsActive;

            _accDal.Update(current);
            return new SuccessResult("حساب بروزرسانی شد.");
        }

        [SecuredOperation("reserveaccount.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IReserveAccountService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _accDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _accDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        // -------- İşlemsel --------
        [SecuredOperation("reserveaccount.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IReserveAccountService.Get")]
        public IResult Activate(Guid id)
        {
            var e = _accDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.IsActive = true;
            _accDal.Update(e);
            return new SuccessResult("فعال شد.");
        }

        [SecuredOperation("reserveaccount.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IReserveAccountService.Get")]
        public IResult Deactivate(Guid id)
        {
            var e = _accDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.IsActive = false;
            _accDal.Update(e);
            return new SuccessResult("غیرفعال شد.");
        }
    }
}
