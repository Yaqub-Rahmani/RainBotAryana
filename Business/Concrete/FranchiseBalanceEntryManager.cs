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
using Core.Extensions;
using System.Globalization;
using Entities.Dtos.FranchiseOwner;
using Entities.Dtos.FranchiseOwner.entry;
using Core.Utilities.KabulDateTime;

namespace Business.Concrete
{
    public class FranchiseBalanceEntryManager : IFranchiseBalanceEntryService
    {
        private readonly IFranchiseBalanceEntryDal _entryDal;
        private readonly IFranchiseOwnerDal _ownerDal;
        private readonly IUserDal _userDal;
        private readonly ITimeProvider _timeProvider;
        private readonly IMoneyEntryDal _moneyEntryDal;

        public FranchiseBalanceEntryManager(IFranchiseBalanceEntryDal entryDal, IFranchiseOwnerDal ownerDal, IUserDal userDal, IMoneyEntryDal moneyEntryDal, ITimeProvider timeProvider)
        {
            _entryDal = entryDal;
            _ownerDal = ownerDal;
            _userDal = userDal;
            _timeProvider = timeProvider;
            _moneyEntryDal = moneyEntryDal;
        }

        // ------ Okumalar ------

        public IDataResult<PagedWithSummaryDto<oldFranchiseBalanceEntryDetailDto>> GetDetails(QueryOptions opt, Guid? ownerId = null)
        {
            opt ??= new QueryOptions();

            var baseQ = _entryDal.QueryNoTracking();

            if (ownerId.HasValue && ownerId.Value != Guid.Empty)
                baseQ = baseQ.Where(x => x.FranchiseOwnerId == ownerId.Value);

            baseQ = baseQ.ApplyIncludes(opt.Includes);

            var searchFields = opt.ExtractSearchFields(out var remainingFilters);

            // Entity filtreleri (bilinenler) + DTO'ya ertelenenler
            baseQ = baseQ.ApplyFilters(remainingFilters, out var deferredEntityFilters);

            var owners = _ownerDal.QueryNoTracking();
            var users = _userDal.QueryNoTracking();

            var projected =
                from e in baseQ
                join o in owners on e.FranchiseOwnerId equals o.Id
                join u in users on e.CreatedByUserId equals u.Id into uj
                from u in uj.DefaultIfEmpty()
                select new oldFranchiseBalanceEntryDetailDto
                {
                    Id = e.Id,

                    FranchiseOwnerId = e.FranchiseOwnerId,
                    FranchiseOwnerName = o.DisplayName,

                    CreatedByUserId = e.CreatedByUserId,
                    CreatedByUserName = u != null ? (u.FirstName ?? "") : "",
                    CreatedAtUtc = DateOnly.FromDateTime(e.CreatedAtUtc),

                    Direction = e.Direction.ToString(),
                    ReferenceNo = (e.PaymentReference ?? ""),
                    Amount = e.Amount,
                    paymentMethod = e.PaymentMethod,

                    OccurredAtUtc = e.OccurredAtUtc,

                    PeriodYear = e.PeriodYear,
                    PeriodMonth = e.PeriodMonth,

                    CollectionId = e.CollectionId,
                    CollectionAllocationId = e.CollectionAllocationId,
                    ContractId = e.ContractId,
                    ShopId = e.ShopId,

                    Notes = e.Notes
                };

            // DTO tarafına ertelenen filtreler (OwnerName / CreatedByUserName vb.)
            projected = projected.ApplyFilters(deferredEntityFilters);

            // Search (OR contains)
            if (!string.IsNullOrWhiteSpace(opt.Search))
                projected = projected.ApplySearch(opt.Search, searchFields);

            // ✅ Summary: filtre+search uygulanmış query'den, paging/sort öncesi
            var summary = projected
                .GroupBy(_ => 1)
                .Select(g => new BalanceSummaryDto
                {
                    TotalIn = g.Where(x => x.Direction == "In").Select(x => (decimal?)x.Amount).Sum() ?? 0m,
                    TotalOut = g.Where(x => x.Direction == "Out").Select(x => (decimal?)x.Amount).Sum() ?? 0m
                })
                .FirstOrDefault() ?? new BalanceSummaryDto();

            // Count
            var total = projected.Count();

            // Sort (mevcut davranış aynen)
            projected = projected.ApplySort(opt.Sort);
            if (!projected.Expression.ToString()!.Contains("OrderBy"))
                projected = projected.OrderByDescending(x => x.OccurredAtUtc);

            // Paging + materialize
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            // Post compute
            FillComputedFields(items);

            return new SuccessDataResult<PagedWithSummaryDto<oldFranchiseBalanceEntryDetailDto>>(
                new PagedWithSummaryDto<oldFranchiseBalanceEntryDetailDto>
                {
                    Page = new PagedResult<oldFranchiseBalanceEntryDetailDto>
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

        private static void FillComputedFields(List<oldFranchiseBalanceEntryDetailDto> items)
        {
            foreach (var x in items)
            {
                // 12-hour time (e.g., 03:25 PM) - English digits
                //x.OccurredTime = x.OccurredAtUtc.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                var en = CultureInfo.GetCultureInfo("en-US");
                x.OccurredTime = x.OccurredAtUtc.ToString("hh:mm tt", en); // 03:25 PM



                // Day name (فارسی)
                x.DayName = x.OccurredAtUtc.ToString("dddd", FaCulture);

                // Direction label (فارسی)
                x.DirectionLabel = string.Equals(x.Direction, "In", StringComparison.OrdinalIgnoreCase)
                    ? "دریافت (ورود به حساب)"
                    : string.Equals(x.Direction, "Out", StringComparison.OrdinalIgnoreCase)
                        ? "پرداخت (خروج از حساب)"
                        : x.Direction;

                // PeriodLabel "1404-04"
                x.PeriodLabel = (x.PeriodYear.HasValue && x.PeriodMonth.HasValue)
                    ? $"{x.PeriodYear.Value}-{x.PeriodMonth.Value:00}"
                    : null;
            }
        }


        [SecuredOperation("franchisebalance.write,admin")]
        [ValidationAspect(typeof(FranchiseBalanceEntryValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseBalanceEntryService.Get")]

        public IResult Add(FranchiseBalanceEntryCreateDto dto, int userId)
        {
            if (_ownerDal.Get(x => x.Id == dto.FranchiseOwnerId) == null)
                return new ErrorResult("صاحب امتیاز یافت نشد.");

            if (dto.MoneyEntryId == null)
                return new ErrorResult("MoneyEntryId الزامی است.");

            if (_moneyEntryDal.Get(x => x.Id == dto.MoneyEntryId) == null)
                return new ErrorResult("MoneyEntry یافت نشد.");

            var entry = new FranchiseBalanceEntry
            {
                Id = dto.Id,
                Direction = dto.Direction,
                Amount = Math.Round(dto.Amount, 2, MidpointRounding.AwayFromZero),
                OccurredAtUtc = DateTime.SpecifyKind(dto.OccurredAtUtc, DateTimeKind.Utc),
                Notes = dto.Notes,
                PaymentMethod = dto.PaymentMethod,
                PaymentReference = dto.PaymentReference,
                CreatedByUserId = userId,
                IsActive = true,
                MoneyEntryId = dto.MoneyEntryId,
                FranchiseOwnerId = dto.FranchiseOwnerId,
            };

            _entryDal.Add(entry);
            return new SuccessResult("ورودی تراز امتیاز ثبت شد.");
        }
    }
}
