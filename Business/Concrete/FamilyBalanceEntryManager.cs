using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Validation;
using Core.Extensions;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.FamilyMember;
using Entities.Dtos.FranchiseOwner;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Business.Concrete
{
    public class FamilyBalanceEntryManager : IFamilyBalanceEntryService
    {
        private readonly IFamilyBalanceEntryDal _entryDal;
        private readonly IFamilyMemberDal _memberDal;
        private readonly ITimeProvider _time;
        private readonly IUserDal _userDal;
        public FamilyBalanceEntryManager(
            IFamilyBalanceEntryDal entryDal,
            IFamilyMemberDal memberDal,
            ITimeProvider time,
            IUserDal userDal)
        {
            _entryDal = entryDal;
            _memberDal = memberDal;
            _time = time;
            _userDal = userDal;
        }

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
        public IDataResult<FamilyBalanceEntryDetailDto> GetDetailById(Guid id)
        {
            var dto = (from e in _entryDal.QueryNoTracking().Where(x => x.Id == id)
                       join m in _memberDal.QueryNoTracking() on e.FamilyMemberId equals m.Id
                       select new Entities.Dtos.FamilyMember.FamilyBalanceEntryDetailDto
                       {
                           Id = e.Id,
                           FamilyMemberId = e.FamilyMemberId,
                           FamilyMemberName = m.DisplayName,
                           Direction = e.Direction.ToString(),
                           DirectionLabel = e.Direction == BalanceDirection.In ? "دریافت (ورود)" : "پرداخت (خروج)",
                           Amount = e.Amount,
                           OccurredAtUtc = e.OccurredAtUtc,
                           OccurredTime = _time.FormatTimeAmPm(e.OccurredAtUtc),
                           DayName = ToDariDayName(TimeZoneInfo.ConvertTimeFromUtc(e.OccurredAtUtc, TimeZoneInfo.FindSystemTimeZoneById("Asia/Kabul")).DayOfWeek),
                           FamilyPayoutId = e.FamilyPayoutId,
                           OwnerDistributionId = e.OwnerDistributionId,
                           CollectionId = e.CollectionId,
                           ContractId = e.ContractId,
                           Notes = e.Notes
                       }).FirstOrDefault();

            return dto == null
                ? new ErrorDataResult<Entities.Dtos.FamilyMember.FamilyBalanceEntryDetailDto>(null, "یافت نشد.")
                : new SuccessDataResult<Entities.Dtos.FamilyMember.FamilyBalanceEntryDetailDto>(dto);
        }

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

        [SecuredOperation("familybalance.write,admin")]
        [ValidationAspect(typeof(FamilyBalanceEntryValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect(".*IFamilyBalanceEntryService.*Get.*")]
        public IResult Add(FamilyBalanceEntry entry)
        {
            if (_memberDal.Get(x => x.Id == entry.FamilyMemberId) == null)
                return new ErrorResult("عضو خانواده یافت نشد.");

            _entryDal.Add(entry);
            return new SuccessResult("ورودی تراز خانواده ثبت شد.");
        }

        [SecuredOperation("familybalance.write,admin")]
        [ValidationAspect(typeof(FamilyBalanceEntryValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect(".*IFamilyBalanceEntryService.*Get.*")]
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
        [CacheRemoveAspect(".*IFamilyBalanceEntryService.*Get.*")]
        public IResult Delete(Guid id)
        {
            var e = _entryDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _entryDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        private static string ToDariDayName(DayOfWeek dow) => dow switch
        {
            DayOfWeek.Saturday => "شنبه",
            DayOfWeek.Sunday => "یکشنبه",
            DayOfWeek.Monday => "دوشنبه",
            DayOfWeek.Tuesday => "سه‌شنبه",
            DayOfWeek.Wednesday => "چهارشنبه",
            DayOfWeek.Thursday => "پنجشنبه",
            DayOfWeek.Friday => "جمعه",
            _ => ""
        };



        //[CacheAspect(10)]
        public IDataResult<PagedWithSummaryDto<FamilyBalanceEntryDetailDto>> GetDetails(QueryOptions opt, Guid? familyMemberId = null)
        {
            opt ??= new QueryOptions();

            var baseQ = _entryDal.QueryNoTracking();

            if (familyMemberId.HasValue && familyMemberId.Value != Guid.Empty)
                baseQ = baseQ.Where(x => x.FamilyMemberId == familyMemberId.Value);

            baseQ = baseQ.ApplyIncludes(opt.Includes);

            var searchFields = opt.ExtractSearchFields(out var remainingFilters);

            // ✅ Entity filtreleri + DTO'ya ertelenenler
            baseQ = baseQ.ApplyFilters(remainingFilters, out var deferredEntityFilters);

            var members = _memberDal.QueryNoTracking();
            var users = _userDal.QueryNoTracking();

            var projected =
                from e in baseQ
                join m in members on e.FamilyMemberId equals m.Id
                join u in users on e.CreatedByUserId equals u.Id into uj
                from u in uj.DefaultIfEmpty()
                select new FamilyBalanceEntryDetailDto
                {
                    Id = e.Id,

                    FamilyMemberId = e.FamilyMemberId,
                    FamilyMemberName = m.DisplayName,

                    CreatedByUserId = e.CreatedByUserId,
                    CreatedByUserName = u != null ? $"{u.FirstName} {u.LastName}".Trim() : "",
                    CreatedAtUtc = DateOnly.FromDateTime(e.CreatedAtUtc),

                    Direction = e.Direction.ToString(),
                    Amount = e.Amount,
                    paymentMethod = e.PaymentMethod,
                    ReferenceNo = e.PaymentReference ?? "",

                    OccurredAtUtc = e.OccurredAtUtc,

                    PeriodYear = e.PeriodYear,
                    PeriodMonth = e.PeriodMonth,

                    FamilyPayoutId = e.FamilyPayoutId,
                    OwnerDistributionId = e.OwnerDistributionId,
                    CollectionId = e.CollectionId,
                    ContractId = e.ContractId,

                    Notes = e.Notes
                };

            // ✅ DTO tarafına ertelenen filtreler (FamilyMemberName / CreatedByUserName vb.)
            projected = projected.ApplyFilters(deferredEntityFilters);

            // Search (kullanıyorsan)
            if (!string.IsNullOrWhiteSpace(opt.Search))
                projected = projected.ApplySearch(opt.Search, searchFields);

            // ✅ Summary: filtre+search sonrası, paging öncesi
            var summary = projected
                .GroupBy(_ => 1)
                .Select(g => new BalanceSummaryDto
                {
                    TotalIn = g.Where(x => x.Direction == "In").Select(x => (decimal?)x.Amount).Sum() ?? 0m,
                    TotalOut = g.Where(x => x.Direction == "Out").Select(x => (decimal?)x.Amount).Sum() ?? 0m
                })
                .FirstOrDefault() ?? new BalanceSummaryDto();

            var total = projected.Count();

            projected = projected.ApplySort(opt.Sort);
            if (!projected.Expression.ToString()!.Contains("OrderBy"))
                projected = projected.OrderByDescending(x => x.OccurredAtUtc);

            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();
            FillComputedFields(items);

            return new SuccessDataResult<PagedWithSummaryDto<FamilyBalanceEntryDetailDto>>(
                new PagedWithSummaryDto<FamilyBalanceEntryDetailDto>
                {
                    Page = new PagedResult<FamilyBalanceEntryDetailDto>
                    {
                        Items = items,
                        Page = opt.Page,
                        PageSize = opt.PageSize,
                        TotalCount = total
                    },
                    Summary = summary
                }
            );
        }

        private static readonly CultureInfo FaCulture = new("fa-IR");

        private static void FillComputedFields(List<FamilyBalanceEntryDetailDto> items)
        {
            var en = CultureInfo.GetCultureInfo("en-US");

            foreach (var x in items)
            {
                x.OccurredTime = x.OccurredAtUtc.ToString("hh:mm tt", en);
                x.DayName = x.OccurredAtUtc.ToString("dddd", FaCulture);

                x.DirectionLabel = string.Equals(x.Direction, "In", StringComparison.OrdinalIgnoreCase)
                    ? "دریافت (ورود به حساب)"
                    : string.Equals(x.Direction, "Out", StringComparison.OrdinalIgnoreCase)
                        ? "پرداخت (خروج از حساب)"
                        : x.Direction;

                x.PeriodLabel = (x.PeriodYear.HasValue && x.PeriodMonth.HasValue)
                    ? $"{x.PeriodYear.Value}-{x.PeriodMonth.Value:00}"
                    : null;
            }
        }
    }
}
