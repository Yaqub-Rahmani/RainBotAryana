using Business.Abstract;
using Core.Extensions;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos.employee;
using System;
using System.Linq;

namespace Business.Concrete
{
    public class EmployeeTransactionManager : IEmployeeTransactionService
    {
        private readonly IEmployeeTransactionDal _txDal;
        private readonly ITimeProvider _time;

        public EmployeeTransactionManager(IEmployeeTransactionDal txDal, ITimeProvider time)
        {
            _txDal = txDal;
            _time = time;
        }

        public IDataResult<PagedResult<EmployeeTransactionListItemDto>> GetByEmployee(Guid employeeId, QueryOptions? opt, int? year = null, int? month = null)
        {
            opt ??= new QueryOptions();

            var q = _txDal.QueryNoTracking().Where(x => x.EmployeeId == employeeId);
            if (year.HasValue) q = q.Where(x => x.Year == year.Value);
            if (month.HasValue) q = q.Where(x => x.Month == month.Value);

            // dynamic filters/sort from CustomTableV2
            if (opt.Filters?.Any() == true) q = q.ApplyFilters(opt.Filters);
            q = q.ApplySort(opt.Sort);

            // basic search: amount or moneyEntryId
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var s = opt.Search.Trim();
                if (decimal.TryParse(s, out var amt))
                    q = q.Where(x => x.Amount == amt);
                else if (Guid.TryParse(s, out var gid))
                    q = q.Where(x => x.Id == gid || x.MoneyEntryId == gid);
            }

            var total = q.Count();
            var items = q
                .Skip((opt.Page - 1) * opt.PageSize)
                .Take(opt.PageSize)
                .Select(x => new EmployeeTransactionListItemDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    Direction = x.Direction,
                    Amount = x.Amount,
                    MoneyEntryId = x.MoneyEntryId,
                    Year = x.Year,
                    Month = x.Month,
                    PaidAtUtc = x.PaidAtUtc
                })
                .ToList();

            return new SuccessDataResult<PagedResult<EmployeeTransactionListItemDto>>(
                new PagedResult<EmployeeTransactionListItemDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                }
            );
        }

        public IResult Add(EmployeeTransactionCreateDto dto, int userId)
        {
            if (dto.EmployeeId == Guid.Empty)
                return new ErrorResult("کارمند نامعتبر است.");

            if (dto.Amount <= 0)
                return new ErrorResult("مقدار نامعتبر است.");

            var (py, pm, _) = _time.TodayPersian();
            var year = (dto.Year.HasValue && dto.Year.Value > 0) ? dto.Year.Value : py;
            var month = (dto.Month.HasValue && dto.Month.Value > 0) ? dto.Month.Value : pm;

            var tx = new EmployeeTransaction
            {
                Id = Guid.NewGuid(),
                EmployeeId = dto.EmployeeId,
                Direction = dto.Direction,
                Amount = dto.Amount,
                MoneyEntryId = dto.MoneyEntryId,
                Year = year,
                Month = month,
                PaidAtUtc = dto.PaidAtUtc ?? _time.UtcNow(),
                CreatedByUserId = userId
            };

            _txDal.Add(tx);
            return new SuccessResult("ثبت شد.");
        }

        public IResult Delete(Guid id)
        {
            var e = _txDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            _txDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
