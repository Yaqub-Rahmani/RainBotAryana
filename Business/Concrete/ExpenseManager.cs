using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation.Expense;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Validation;
using Core.Extensions;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos.Expense;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    public class ExpenseManager : IExpenseService
    {
        private readonly IExpenseDal _expenseDal;
        private readonly IMoneyEntryDal _moneyEntryDal;
        private readonly IMoneyAccountDal _moneyAccountDal;
        private readonly IDollarRateDal _dollarRateDal;
        private readonly IEmployeeDal _employeeDal;

        // ✅ Reserve
        private readonly IReserveAccountDal _reserveAccountDal;
        private readonly IReserveEntryDal _reserveEntryDal;

        private readonly ITimeProvider _time;

        public ExpenseManager(
            IExpenseDal expenseDal,
            IMoneyEntryDal moneyEntryDal,
            IMoneyAccountDal moneyAccountDal,
            IDollarRateDal dollarRateDal,
            IEmployeeDal employeeDal,
            IReserveAccountDal reserveAccountDal,
            IReserveEntryDal reserveEntryDal,
            ITimeProvider time)
        {
            _expenseDal = expenseDal;
            _moneyEntryDal = moneyEntryDal;
            _moneyAccountDal = moneyAccountDal;
            _dollarRateDal = dollarRateDal;
            _employeeDal = employeeDal;

            _reserveAccountDal = reserveAccountDal;
            _reserveEntryDal = reserveEntryDal;

            _time = time;
        }

        // ================================
        // Read
        // ================================
        //[SecuredOperation("expense.read,admin")]
        public IDataResult<PagedResult<ExpenseDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();

            var q = _expenseDal.QueryNoTracking()
                .Where(x => x.IsActive);

            if (opt.Filters?.Any() == true)
                q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var s = opt.Search.Trim();
                q = q.Where(x => x.DisplayName.Contains(s) || (x.Notes != null && x.Notes.Contains(s)));
            }

            // sort (fallback)
            q = q.ApplySort(opt.Sort) ?? q.OrderByDescending(x => x.OccurredAtUtc);

            var totalCount = q.Count();

            var page = q.ApplyPaging(opt.Page, opt.PageSize).ToList();

            // batch load relations
            var ids = page.Select(x => x.Id).ToList();

            var moneyEntries = _moneyEntryDal.QueryNoTracking()
                .Where(x => x.ExpenseId != null && ids.Contains(x.ExpenseId.Value))
                .ToList();

            var accountIds = moneyEntries.Select(x => x.MoneyAccountId).Distinct().ToList();
            var accounts = _moneyAccountDal.QueryNoTracking()
                .Where(x => accountIds.Contains(x.Id))
                .ToList();

            var employeeIds = page.Where(x => x.EmployeeId != null).Select(x => x.EmployeeId!.Value).Distinct().ToList();
            var employees = employeeIds.Count == 0
                ? new List<Employee>()
                : _employeeDal.QueryNoTracking().Where(x => employeeIds.Contains(x.Id)).ToList();

            var items = page.Select(e =>
            {
                var me = moneyEntries.FirstOrDefault(x => x.ExpenseId == e.Id);
                var acc = me == null ? null : accounts.FirstOrDefault(a => a.Id == me.MoneyAccountId);
                var emp = e.EmployeeId == null ? null : employees.FirstOrDefault(x => x.Id == e.EmployeeId);
                return Project(e, me, acc, emp);
            }).ToList();

            var result = new PagedResult<ExpenseDetailDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = opt.Page,
                PageSize = opt.PageSize
            };

            return new SuccessDataResult<PagedResult<ExpenseDetailDto>>(result);
        }

        //[SecuredOperation("expense.read,admin")]
        public IDataResult<ExpenseDetailDto> GetById(Guid id)
        {
            var e = _expenseDal.Get(x => x.Id == id);
            if (e == null) return new ErrorDataResult<ExpenseDetailDto>(null, "مصرف پیدا نشد.");

            var me = _moneyEntryDal.QueryNoTracking().FirstOrDefault(x => x.ExpenseId == id);
            var acc = me == null ? null : _moneyAccountDal.Get(x => x.Id == me.MoneyAccountId);
            Employee? emp = null;
            if (e.EmployeeId != null)
                emp = _employeeDal.Get(x => x.Id == e.EmployeeId);

            return new SuccessDataResult<ExpenseDetailDto>(Project(e, me, acc, emp));
        }

        //[SecuredOperation("expense.read,admin")]
        public IDataResult<decimal> GetMoneyAccountBalance(Guid moneyAccountId)
        {
            var acc = _moneyAccountDal.Get(x => x.Id == moneyAccountId);
            if (acc == null) return new ErrorDataResult<decimal>(0m, "حساب پیدا نشد.");

            var bal = GetMoneyAccountBalanceInternal(moneyAccountId);
            return new SuccessDataResult<decimal>(bal);
        }

        // ================================
        // Write
        // ================================
        [TransactionScopeAspect]
        //[SecuredOperation("expense.write,admin")]
        [ValidationAspect(typeof(ExpenseCreateValidator))]
        public IResult Create(ExpenseCreateFromUiDto dto, int userId)
        {
            // Salary -> employee zorunlu
            if (dto.Category == ExpenseCategory.Salary && dto.EmployeeId == null)
                return new ErrorResult("برای کتگوری معاش، انتخاب کارمند الزامی است.");

            var now = _time.UtcNow();
            var occurredAt = NormalizeUtc(dto.OccurredAtUtc) ?? now;

            // ✅ DTO nullable -> local zorunlu alanlar (Validator zaten garanti ediyor)
            var amount = dto.Amount;
            var direction = dto.Direction;
            var category = dto.Category;
            var source = dto.Source;
            var method = dto.PaymentMethod;
            var moneyAccountId = dto.MoneyAccountId;

            var acc = _moneyAccountDal.Get(x => x.Id == moneyAccountId && x.IsActive);
            if (acc == null) return new ErrorResult("حساب پولی نامعتبر است.");

            // ✅ MoneyAccount balance check
            if (direction == BalanceDirection.Out)
            {
                var bal = GetMoneyAccountBalanceInternal(acc.Id);
                if (amount > bal)
                    return new ErrorResult($"موجودی کافی نیست. موجودی فعلی: {bal}");
            }

            // ✅ Dollar rate
            var rate = GetEffectiveDollarRate(occurredAt);
            if (rate == null) return new ErrorResult("نرخ دالر برای این تاریخ موجود نیست.");

            var usdEq = ComputeUsdEquivalent(amount, acc.Currency, rate.AfnPerUsd);

            // ✅ Reserve plan + overdraw check (BEFORE any DB write)
            ReserveAccount? reserveAcc = null;
            decimal? reserveAmountAfn = null;

            var reserveType = MapSourceToReserveAccountType(source);
            if (reserveType != null)
            {
                reserveAcc = _reserveAccountDal.QueryNoTracking()
                    .FirstOrDefault(x => x.Type == reserveType.Value && x.IsActive);

                if (reserveAcc == null)
                    return new ErrorResult("برای منبع انتخاب شده، حساب رزرو تعریف نشده است.");

                reserveAmountAfn = ToAfn(amount, acc.Currency, rate.AfnPerUsd);

                if (direction == BalanceDirection.Out)
                {
                    var rbal = GetReserveBalanceInternal(reserveAcc.Id);
                    if (reserveAmountAfn.Value > rbal)
                        return new ErrorResult($"موجودی کافی در حساب رزرو نیست. موجودی فعلی: {rbal}");
                }
            }

            var expense = new Entities.Concrete.Expense
            {
                Id = Guid.NewGuid(),
                DisplayName = dto.DisplayName!.Trim(),
                Amount = amount,
                Category = category,
                Source = source,
                EmployeeId = dto.EmployeeId,
                OccurredAtUtc = occurredAt,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                PaymentMethod = method,
                CreatedByUserId = userId,
                CreatedAtUtc = now,
                UpdatedByUserId = null,
                UpdatedAtUtc = null,
                IsActive = true,
                MoneyEntryId = null
            };

            _expenseDal.Add(expense);

            var moneyEntry = new MoneyEntry
            {
                Id = Guid.NewGuid(),
                MoneyAccountId = acc.Id,
                Direction = direction,
                Amount = amount,
                Currency = acc.Currency,
                AfnPerUsdAtThatTime = rate.AfnPerUsd,
                UsdEquivalent = usdEq,
                OccurredAtUtc = occurredAt,
                CreatedByUserId = userId,
                CreatedAtUtc = now,
                ExpenseId = expense.Id,
                CollectionId = null
            };

            _moneyEntryDal.Add(moneyEntry);

            expense.MoneyEntryId = moneyEntry.Id;
            _expenseDal.Update(expense);

            // ✅ ReserveEntry write (AFN ledger)
            if (reserveAcc != null && reserveAmountAfn.HasValue)
            {
                var re = new ReserveEntry
                {
                    Id = Guid.NewGuid(),
                    ReserveAccountId = reserveAcc.Id,
                    Direction = direction,
                    Amount = Round2(reserveAmountAfn.Value),
                    MoneyEntryId = moneyEntry.Id,
                    OccurredAtUtc = occurredAt,
                    Description = BuildReserveDescription(expense),
                    PaymentMethod = method,
                    ExternalDocNo = ReserveDocNo(expense.Id),
                    CreatedByUserId = userId,
                    CreatedAtUtc = now
                };

                _reserveEntryDal.Add(re);
            }

            return new SuccessResult("مصرف ثبت شد.");
        }

        [TransactionScopeAspect]
        //[SecuredOperation("expense.write,admin")]
        [ValidationAspect(typeof(ExpenseUpdateValidator))]
        public IResult Update(Guid id, ExpenseUpdateFromUiDto dto, int userId)
        {
            if (id != dto.Id) return new ErrorResult("شناسه با مدل هم‌خوانی ندارد.");

            if (dto.Category == ExpenseCategory.Salary && dto.EmployeeId == null)
                return new ErrorResult("برای کتگوری معاش، انتخاب کارمند الزامی است.");

            var expense = _expenseDal.Get(x => x.Id == id && x.IsActive);
            if (expense == null) return new ErrorResult("مصرف پیدا نشد.");

            var moneyEntry = _moneyEntryDal.Query().FirstOrDefault(x => x.ExpenseId == id);
            if (moneyEntry == null) return new ErrorResult("حرکت پولی مرتبط پیدا نشد.");

            var now = _time.UtcNow();
            var occurredAt = NormalizeUtc(dto.OccurredAtUtc) ?? expense.OccurredAtUtc;

            // ✅ DTO nullable -> local zorunlu alanlar (Validator garanti ediyor)
            var amount = dto.Amount;
            var direction = dto.Direction;
            var category = dto.Category;
            var source = dto.Source;
            var method = dto.PaymentMethod;
            var moneyAccountId = dto.MoneyAccountId;

            var newAcc = _moneyAccountDal.Get(x => x.Id == moneyAccountId && x.IsActive);
            if (newAcc == null) return new ErrorResult("حساب پولی نامعتبر است.");

            // ✅ MoneyAccount balance check
            if (direction == BalanceDirection.Out)
            {
                var available = GetAvailableBalanceForUpdate(newAcc.Id, moneyEntry);
                if (amount > available)
                    return new ErrorResult($"موجودی کافی نیست. موجودی قابل مصرف: {available}");
            }

            // ✅ Dollar rate
            var rate = GetEffectiveDollarRate(occurredAt);
            if (rate == null) return new ErrorResult("نرخ دالر برای این تاریخ موجود نیست.");

            var usdEq = ComputeUsdEquivalent(amount, newAcc.Currency, rate.AfnPerUsd);

            // ✅ Reserve pre-check (BEFORE any DB write)
            var docNo = ReserveDocNo(expense.Id);
            var existingReserveEntry = _reserveEntryDal.Query().FirstOrDefault(x => x.ExternalDocNo == docNo);

            ReserveAccount? targetReserveAcc = null;
            decimal? targetReserveAmountAfn = null;

            var reserveType = MapSourceToReserveAccountType(source);
            if (reserveType != null)
            {
                targetReserveAcc = _reserveAccountDal.QueryNoTracking()
                    .FirstOrDefault(x => x.Type == reserveType.Value && x.IsActive);

                if (targetReserveAcc == null)
                    return new ErrorResult("برای منبع انتخاب شده، حساب رزرو تعریف نشده است.");

                targetReserveAmountAfn = ToAfn(amount, newAcc.Currency, rate.AfnPerUsd);

                if (direction == BalanceDirection.Out)
                {
                    var availableReserve = existingReserveEntry == null
                        ? GetReserveBalanceInternal(targetReserveAcc.Id)
                        : GetAvailableReserveBalanceForUpdate(targetReserveAcc.Id, existingReserveEntry);

                    if (targetReserveAmountAfn.Value > availableReserve)
                        return new ErrorResult($"موجودی کافی در حساب رزرو نیست. موجودی فعلی: {availableReserve}");
                }
            }

            // update expense
            expense.DisplayName = dto.DisplayName!.Trim();
            expense.Amount = amount;
            expense.Category = category;
            expense.Source = source;
            expense.EmployeeId = dto.EmployeeId;
            expense.OccurredAtUtc = occurredAt;
            expense.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
            expense.PaymentMethod = method;
            expense.UpdatedByUserId = userId;
            expense.UpdatedAtUtc = now;

            _expenseDal.Update(expense);

            // update money entry
            moneyEntry.MoneyAccountId = newAcc.Id;
            moneyEntry.Direction = direction;
            moneyEntry.Amount = amount;
            moneyEntry.Currency = newAcc.Currency;
            moneyEntry.AfnPerUsdAtThatTime = rate.AfnPerUsd;
            moneyEntry.UsdEquivalent = usdEq;
            moneyEntry.OccurredAtUtc = occurredAt;

            _moneyEntryDal.Update(moneyEntry);

            // ✅ ReserveEntry sync
            if (reserveType == null)
            {
                if (existingReserveEntry != null)
                    _reserveEntryDal.Delete(existingReserveEntry);
            }
            else
            {
                if (existingReserveEntry == null)
                {
                    var re = new ReserveEntry
                    {
                        Id = Guid.NewGuid(),
                        ReserveAccountId = targetReserveAcc!.Id,
                        Direction = direction,
                        Amount = Round2(targetReserveAmountAfn!.Value),
                        MoneyEntryId = moneyEntry.Id,
                        OccurredAtUtc = occurredAt,
                        Description = BuildReserveDescription(expense),
                        PaymentMethod = method,
                        ExternalDocNo = docNo,
                        CreatedByUserId = userId,
                        CreatedAtUtc = now
                    };
                    _reserveEntryDal.Add(re);
                }
                else
                {
                    existingReserveEntry.ReserveAccountId = targetReserveAcc!.Id;
                    existingReserveEntry.Direction = direction;
                    existingReserveEntry.Amount = Round2(targetReserveAmountAfn!.Value);
                    existingReserveEntry.MoneyEntryId = moneyEntry.Id;
                    existingReserveEntry.OccurredAtUtc = occurredAt;
                    existingReserveEntry.Description = BuildReserveDescription(expense);
                    existingReserveEntry.PaymentMethod = method;
                    existingReserveEntry.ExternalDocNo = docNo;
                    existingReserveEntry.UpdatedByUserId = userId;
                    existingReserveEntry.UpdatedAtUtc = now;

                    _reserveEntryDal.Update(existingReserveEntry);
                }
            }

            return new SuccessResult("مصرف به‌روزرسانی شد.");
        }

        [TransactionScopeAspect]
        //[SecuredOperation("expense.write,admin")]
        public IResult Delete(Guid id, int userId)
        {
            var expense = _expenseDal.Get(x => x.Id == id && x.IsActive);
            if (expense == null) return new ErrorResult("مصرف پیدا نشد.");

            // ✅ delete reserve entries by docno
            var docNo = ReserveDocNo(expense.Id);
            var resEntries = _reserveEntryDal.Query().Where(x => x.ExternalDocNo == docNo).ToList();
            foreach (var re in resEntries)
                _reserveEntryDal.Delete(re);

            // remove money entry (affects balances)
            var me = _moneyEntryDal.Query().FirstOrDefault(x => x.ExpenseId == id);
            if (me != null) _moneyEntryDal.Delete(me);

            expense.IsActive = false;
            expense.UpdatedByUserId = userId;
            expense.UpdatedAtUtc = _time.UtcNow();
            _expenseDal.Update(expense);

            return new SuccessResult("مصرف حذف شد.");
        }

        // ================================
        // Helpers
        // ================================
        private static DateTime? NormalizeUtc(DateTime? dt)
        {
            if (dt == null) return null;
            if (dt.Value.Kind == DateTimeKind.Utc) return dt.Value;
            if (dt.Value.Kind == DateTimeKind.Unspecified) return DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc);
            return dt.Value.ToUniversalTime();
        }

        private DollarRate? GetEffectiveDollarRate(DateTime occurredAtUtc)
        {
            return _dollarRateDal.QueryNoTracking()
                .Where(x => x.EffectiveFromUtc <= occurredAtUtc)
                .OrderByDescending(x => x.EffectiveFromUtc)
                .FirstOrDefault();
        }

        private static decimal ComputeUsdEquivalent(decimal amount, Currency currency, decimal rateAfnPerUsd)
        {
            if (currency == Currency.Usd) return amount;
            if (rateAfnPerUsd <= 0) return 0;
            return Math.Round(amount / rateAfnPerUsd, 4, MidpointRounding.AwayFromZero);
        }

        private decimal GetMoneyAccountBalanceInternal(Guid moneyAccountId)
        {
            var entries = _moneyEntryDal.QueryNoTracking().Where(x => x.MoneyAccountId == moneyAccountId);
            var totalIn = entries.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m;
            var totalOut = entries.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m;
            return totalIn - totalOut;
        }

        private decimal GetAvailableBalanceForUpdate(Guid targetAccountId, MoneyEntry currentEntry)
        {
            // balance considering that we are going to replace currentEntry with a new value
            var bal = GetMoneyAccountBalanceInternal(targetAccountId);

            if (currentEntry.MoneyAccountId == targetAccountId)
            {
                // remove current entry effect
                if (currentEntry.Direction == BalanceDirection.Out) bal += currentEntry.Amount;
                else if (currentEntry.Direction == BalanceDirection.In) bal -= currentEntry.Amount;
            }

            return bal;
        }

        // ---------- Reserve helpers ----------
        private static string ReserveDocNo(Guid expenseId) => $"EXP:{expenseId:N}";

        private static ReserveAccountType? MapSourceToReserveAccountType(ExpenseSource s)
        {
            return s switch
            {
                ExpenseSource.MarketShare => ReserveAccountType.MarketMain,
                ExpenseSource.MarketReserve => ReserveAccountType.MarketReserve,
                ExpenseSource.OwnerReserve => ReserveAccountType.OwnerReserve,
                ExpenseSource.ElectricityReserve => ReserveAccountType.ElectricityReserve,
                _ => null
            };
        }

        private static decimal ToAfn(decimal amount, Currency currency, decimal afnPerUsd)
        {
            if (currency == Currency.Afn) return Round2(amount);
            if (afnPerUsd <= 0) return 0m;
            return Round2(amount * afnPerUsd);
        }

        private decimal GetReserveBalanceInternal(Guid reserveAccountId)
        {
            var entries = _reserveEntryDal.QueryNoTracking().Where(x => x.ReserveAccountId == reserveAccountId);
            var totalIn = entries.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m;
            var totalOut = entries.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m;
            return totalIn - totalOut;
        }

        private decimal GetAvailableReserveBalanceForUpdate(Guid targetReserveAccountId, ReserveEntry currentEntry)
        {
            var bal = GetReserveBalanceInternal(targetReserveAccountId);

            if (currentEntry.ReserveAccountId == targetReserveAccountId)
            {
                // remove current entry effect
                if (currentEntry.Direction == BalanceDirection.Out) bal += currentEntry.Amount;
                else if (currentEntry.Direction == BalanceDirection.In) bal -= currentEntry.Amount;
            }

            return bal;
        }

        private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        private static string BuildReserveDescription(Entities.Concrete.Expense e)
        {
            // ReserveEntry.Description max 500
            var title = (e.DisplayName ?? "").Trim();
            var notes = (e.Notes ?? "").Trim();

            if (string.IsNullOrWhiteSpace(notes))
                return title.Length <= 500 ? title : title.Substring(0, 500);

            var combined = $"{title} | {notes}";
            if (combined.Length <= 500) return combined;
            return combined.Substring(0, 500);
        }

        // ---------- UI label helpers ----------
        private static string CurrencyToLabel(Currency c) => c == Currency.Afn ? "؋" : "USD";

        private static string DirectionToLabel(BalanceDirection d)
            => d == BalanceDirection.In ? "ورود" : "خروج";

        private static string PaymentMethodToLabel(PaymentMethod m)
        {
            return m switch
            {
                PaymentMethod.Cash => "نقد",
                PaymentMethod.BankTransfer => "بانک",
                PaymentMethod.Pos => "POS",
                PaymentMethod.Cheque => "چک",
                PaymentMethod.Other => "دیگر",
                _ => "نامشخص"
            };
        }

        private static string ExpenseSourceToLabel(ExpenseSource s)
        {
            return s switch
            {
                ExpenseSource.MarketShare => "سهم مارکیت",
                ExpenseSource.MarketReserve => "ریزرف مارکیت",
                ExpenseSource.OwnerReserve => "ریزرف امتیازدار",
                ExpenseSource.ElectricityReserve => "ریزرف برق",
                _ => "نامشخص"
            };
        }

        private static string ExpenseCategoryToLabel(ExpenseCategory c)
        {
            return c switch
            {
                ExpenseCategory.Salary => "معاش کارمند",
                ExpenseCategory.StaffFood => "نان و چای کارمندان",
                ExpenseCategory.DailyMarketExpense => "مصارف روزانه مارکیت",
                ExpenseCategory.GoodsAndSupplies => "خریداری جنس و لوازم",
                ExpenseCategory.Office => "مصارف دفتر و اداری",
                ExpenseCategory.Utilities => "بل‌ها: برق، آب، تلفن و انترنت",
                ExpenseCategory.Maintenance => "ترمیم، تعمیرات",
                ExpenseCategory.Consumables => "مواد مصرفی و سوختنی",
                ExpenseCategory.TransportRent => "کرایه ترانسپورت",
                ExpenseCategory.PenaltiesAndFees => "جریمه و فیس‌ها",
                ExpenseCategory.Other => "دیگر مصارف",
                _ => "نامشخص"
            };
        }

        private ExpenseDetailDto Project(Expense e, MoneyEntry? me, MoneyAccount? acc, Employee? emp)
        {
            var currency = me?.Currency ?? (acc?.Currency ?? Currency.Afn);
            var direction = me?.Direction ?? BalanceDirection.Out;

            return new ExpenseDetailDto
            {
                Id = e.Id,
                DisplayName = e.DisplayName,
                Amount = e.Amount,
                Currency = currency,
                CurrencyLabel = CurrencyToLabel(currency),
                Direction = direction,
                DirectionLabel = DirectionToLabel(direction),
                Category = e.Category,
                CategoryLabel = ExpenseCategoryToLabel(e.Category),
                Source = e.Source,
                SourceLabel = ExpenseSourceToLabel(e.Source),
                EmployeeId = e.EmployeeId,
                EmployeeName = emp?.DisplayName,
                OccurredAtUtc = e.OccurredAtUtc,
                PaymentMethod = e.PaymentMethod,
                PaymentMethodLabel = PaymentMethodToLabel(e.PaymentMethod),
                Notes = e.Notes,
                MoneyEntryId = me?.Id ?? e.MoneyEntryId,
                MoneyAccountId = acc?.Id ?? me?.MoneyAccountId ?? Guid.Empty,
                MoneyAccountName = acc?.Name ?? "",
                UsdEquivalent = me?.UsdEquivalent ?? 0m,
                AfnPerUsdAtThatTime = me?.AfnPerUsdAtThatTime ?? 0m,
                IsActive = e.IsActive
            };
        }
    }
}
