using Business.Abstract;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Pdf.Abstractions;
using Core.Utilities.Pdf.Contracts;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Business.Concrete;

public sealed class ReportManager : IReportService
{
    private readonly ICollectionDal _collectionDal;
    private readonly IPdfReportEngine _pdf;
    private readonly ITimeProvider _time;
    private readonly IUserDisplayNameResolver _userNames;
    private readonly IContractDal _contractDal;
    private readonly IAccrualDal _accrualDal;
    private readonly IMeterReadingDal _meterReadingDal;

    public ReportManager(
        ICollectionDal collectionDal,
        IPdfReportEngine pdf,
        ITimeProvider time,
        IUserDisplayNameResolver userNames,
        IContractDal contractDal,
        IAccrualDal accrualDal, 
        IMeterReadingDal meterReadingDal)
    {
        _collectionDal = collectionDal;
        _pdf = pdf;
        _time = time;
        _userNames = userNames;
        _contractDal = contractDal;
        _accrualDal = accrualDal;
        _meterReadingDal = meterReadingDal;
    }

    public IDataResult<byte[]> GeneratePaymentReceiptA6(Guid collectionId, PdfRenderOptions options)
    {
        var c = _collectionDal.QueryNoTracking()
            .Where(x => x.Id == collectionId)
            .Include(x => x.Shop).ThenInclude(s => s.Market)
            .Include(x => x.Shop).ThenInclude(s => s.Floor)
            .Include(x => x.Shop).ThenInclude(s => s.ShopFranchiseShares).ThenInclude(ss => ss.FranchiseOwner)
            .Include(x => x.Tenant)
            .Include(x => x.Allocations)
            .Include(x => x.Contract)
            .FirstOrDefault();

        if (c is null)
            return new ErrorDataResult<byte[]>(null, "یافت نشد.");

        var model = MapToReceiptModel(c);

        var pdfBytes = _pdf.Render("payment-receipt-a6", model, options);
        return new SuccessDataResult<byte[]>(pdfBytes);
    }

    public IDataResult<byte[]> GeneratePaymentReceiptsA6(IReadOnlyList<Guid> collectionIds, PdfRenderOptions options)
    {
        if (collectionIds is null || collectionIds.Count == 0)
            return new ErrorDataResult<byte[]>(null, "لیست خالی است.");

        // tek query ile hepsini çek
        var list = _collectionDal.QueryNoTracking()
            .Where(x => collectionIds.Contains(x.Id))
            .Include(x => x.Shop).ThenInclude(s => s.Market)
            .Include(x => x.Shop).ThenInclude(s => s.Floor)
            .Include(x => x.Shop).ThenInclude(s => s.ShopFranchiseShares).ThenInclude(ss => ss.FranchiseOwner)
            .Include(x => x.Tenant)
            .Include(x => x.Allocations)
            .Include(x => x.Contract)
            .ToList();

        // bulunmayanları yakala
        var foundIds = list.Select(x => x.Id).ToHashSet();
        var missing = collectionIds.Where(id => !foundIds.Contains(id)).ToList();
        if (missing.Count > 0)
            return new ErrorDataResult<byte[]>(null, $"برخی موارد یافت نشد. Count: {missing.Count}");

        // input sırasını koru
        var dict = list.ToDictionary(x => x.Id, x => x);

        var models = new List<Core.Utilities.Pdf.Contracts.PaymentReceiptA6Model>(collectionIds.Count);
        foreach (var id in collectionIds)
        {
            var c = dict[id];
            var model = MapToReceiptModel(c); // senin mevcut mapper
            models.Add(model);
        }

        // Tek PDF, her model ayrı sayfa
        var pdfBytes = _pdf.RenderMany("payment-receipt-a6", models, options);
        return new SuccessDataResult<byte[]>(pdfBytes);
    }
    private PaymentReceiptA6Model MapToReceiptModel(Collection c)
        {
            // Franchise owners (active at PaidAtUtc)
            var paidDate = DateOnly.FromDateTime(c.PaidAtUtc);

            var activeOwners = c.Shop?.ShopFranchiseShares?
                .Where(s => s.EffectiveFrom <= paidDate && (s.EffectiveTo == null || paidDate <= s.EffectiveTo.Value))
                .OrderByDescending(s => s.SharePercent)
                .Select(s => s.FranchiseOwner.DisplayName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList() ?? new List<string>();

            string franchiseText = "";
            if (activeOwners.Count == 1)
                franchiseText = activeOwners[0];
            else if (activeOwners.Count > 1)
                franchiseText = $"{activeOwners[0]} و {activeOwners.Count - 1} نفر دیگر";

            var floor = c.Shop?.Floor;
            var floorText = floor == null
                ? ""
                : (!string.IsNullOrWhiteSpace(floor.FloorName) ? floor.FloorName : $"طبقه {floor.Level}");

            // Amount with AFN symbol fixed
            var amt = c.AmountReceived.ToString("N0", CultureInfo.InvariantCulture);
            var amountBig = _time.RtlSafeLatin($"{amt} ؋");

            // Weekday (Dari)
            var weekday = WeekdayDari(ToKabul(c.PaidAtUtc).DayOfWeek);

            // User display name (must exist)
            var creatorName = _userNames.GetDisplayName(c.CreatedByUserId);
            var createdByText = $"ادمین: {creatorName}";

            var model = new PaymentReceiptA6Model
            {
                MarketName = c.Shop?.Market?.MarketName ?? "",
                FloorText = floorText,
                ShopName = !string.IsNullOrWhiteSpace(c.Shop?.Name) ? c.Shop!.Name! : (c.Shop?.UnitNumber ?? ""),
                FranchiseOwnerText = franchiseText,
                TenantName = c.Tenant?.DisplayName ?? "",

                PaidWeekday = weekday,
                PaidDateBig = _time.FormatPersianDateWithMonthNameDash(c.PaidAtUtc),
                PaidTime = _time.FormatTimeAmPm(c.PaidAtUtc),

                AmountBig = amountBig,
                MethodText = PaymentMethodFa(c.Method),

                CreatedByName = createdByText,
                BusinessActivity = c.Contract?.BusinessActivity ?? "",

            };

            foreach (var a in c.Allocations
                         .OrderBy(x => x.Kind)
                         .ThenBy(x => x.PeriodYear)
                         .ThenBy(x => x.PeriodMonth))
            {
                var period = BuildPeriod(a.PeriodYear, a.PeriodMonth, c.TargetYear, c.TargetMonth);

                model.Lines.Add(new PaymentReceiptA6LineModel
                {
                    Period = _time.RtlSafeLatin(period),
                    Kind = AllocationKindFa(a.Kind),
                    Amount = _time.RtlSafeLatin(a.Amount.ToString("N0", CultureInfo.InvariantCulture) + " ؋")
                });
            }

            return model;
        }

    private static string BuildPeriod(int? y, int? m, int? fy, int? fm)
    {
        var yy = y ?? fy;
        var mm = m ?? fm;
        if (yy is null || mm is null) return "";
        return $"{yy}/{mm}";
    }

    private static DateTime ToKabul(DateTime utcOrAny)
    {
#if WINDOWS
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Afghanistan Standard Time");
#else
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kabul");
#endif
        return TimeZoneInfo.ConvertTime(utcOrAny, tz);
    }

    private static string WeekdayDari(DayOfWeek d) => d switch
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

    private static string PaymentMethodFa(PaymentMethod m) => m switch
    {
        PaymentMethod.Cash => "نقده حضوری",
        PaymentMethod.BankTransfer => "بانک",
        PaymentMethod.Pos => "حواله",
        PaymentMethod.Cheque => "چک",
        PaymentMethod.Other => "دیگر",
        _ => "نامعلوم"
    };

    private static string AllocationKindFa(AllocationKind k) => k switch
    {
        AllocationKind.Electricity => "بل برق",
        AllocationKind.Market => "سریداری",
        AllocationKind.Owner => "سهم مالک",
        AllocationKind.Franchise => "امتیازی",
        _ => "نامعلوم"
    };

    public IDataResult<byte[]> GenerateContractDebtStatusA6(Guid contractId, PdfRenderOptions options)
    {
        var c = LoadContractForDebtReport(contractId);
        if (c is null) return new ErrorDataResult<byte[]>(null, "یافت نشد.");

        var model = MapToContractDebtModel(c);
        var pdfBytes = _pdf.Render("contract-debt-a6", model, options);
        return new SuccessDataResult<byte[]>(pdfBytes);
    }

    public IDataResult<byte[]> GenerateContractDebtStatusesA6(IReadOnlyList<Guid> contractIds, PdfRenderOptions options)
    {
        if (contractIds is null || contractIds.Count == 0)
            return new ErrorDataResult<byte[]>(null, "لیست خالی است.");

        var list = _contractDal.QueryNoTracking()
            .Where(x => contractIds.Contains(x.Id))
            .Include(x => x.Shop).ThenInclude(s => s.Market)
            .Include(x => x.Shop).ThenInclude(s => s.Floor)
            .Include(x => x.Shop).ThenInclude(s => s.ShopFranchiseShares).ThenInclude(ss => ss.FranchiseOwner)
            .Include(x => x.Tenant)
            .Include(x => x.Accruals).ThenInclude(a => a.MeterReading) // electricity kWh :contentReference[oaicite:7]{index=7}
            .ToList();

        var foundIds = list.Select(x => x.Id).ToHashSet();
        var missing = contractIds.Where(id => !foundIds.Contains(id)).ToList();
        if (missing.Count > 0)
            return new ErrorDataResult<byte[]>(null, $"برخی قراردادها یافت نشد. Count: {missing.Count}");

        var dict = list.ToDictionary(x => x.Id, x => x);
        var models = new List<ContractDebtStatusA6Model>(contractIds.Count);

        foreach (var id in contractIds)
            models.Add(MapToContractDebtModel(dict[id]));

        var pdfBytes = _pdf.RenderMany("contract-debt-a6", models, options);
        return new SuccessDataResult<byte[]>(pdfBytes);
    }

    private Contract? LoadContractForDebtReport(Guid contractId)
    {
        return _contractDal.QueryNoTracking()
            .Where(x => x.Id == contractId)
            .Include(x => x.Shop).ThenInclude(s => s.Market)
            .Include(x => x.Shop).ThenInclude(s => s.Floor)
            .Include(x => x.Shop).ThenInclude(s => s.ShopFranchiseShares).ThenInclude(ss => ss.FranchiseOwner)
            .Include(x => x.Tenant)
            .Include(x => x.Accruals).ThenInclude(a => a.MeterReading)
            .FirstOrDefault();
    }

    private ContractDebtStatusA6Model MapToContractDebtModel(Contract c)
    {
        var nowUtc = _time.UtcNow();
        var today = _time.Today();                // DateOnly (Kabul)
        var (py, pm, _) = _time.TodayPersian();   // current persian period

        // Active franchise owners (names)
        var activeOwners = c.Shop?.ShopFranchiseShares?
            .Where(s => s.EffectiveFrom <= today && (s.EffectiveTo == null || today <= s.EffectiveTo.Value))
            .OrderByDescending(s => s.SharePercent)
            .Select(s => s.FranchiseOwner.DisplayName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .ToList() ?? new List<string>();

        string ownersLabel = "";
        if (activeOwners.Count == 1) ownersLabel = activeOwners[0];
        else if (activeOwners.Count > 1) ownersLabel = $"{activeOwners[0]} و {activeOwners.Count - 1} نفر دیگر";

        // Open debts (Outstanding + Status) :contentReference[oaicite:8]{index=8}
        var openAccruals = (c.Accruals ?? new List<Accrual>())
            .Where(a => a.Status != AccrualStatus.Settled && a.OutstandingAmount > 0m)
            .ToList();

        // Old debts grouped by period (before current period)
        var oldGroups = openAccruals
            .Where(a => a.Year < py || (a.Year == py && a.Month < pm))
            .GroupBy(a => new { a.Year, a.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Amount = g.Sum(x => x.OutstandingAmount)
            })
            .Where(x => x.Amount > 0m)
            .ToList();

        decimal oldDebtTotal = oldGroups.Sum(x => x.Amount);

        // Current period totals + breakdown by kind (AllocationKind in Accrual) :contentReference[oaicite:9]{index=9}
        var currentAccruals = openAccruals.Where(a => a.Year == py && a.Month == pm).ToList();
        decimal currentTotal = currentAccruals.Sum(a => a.OutstandingAmount);

        decimal currOwner = currentAccruals.Where(a => a.Kind == AllocationKind.Owner).Sum(a => a.OutstandingAmount);
        decimal currMarket = currentAccruals.Where(a => a.Kind == AllocationKind.Market).Sum(a => a.OutstandingAmount);
        decimal currFranchise = currentAccruals.Where(a => a.Kind == AllocationKind.Franchise).Sum(a => a.OutstandingAmount);
        decimal currElectricity = currentAccruals.Where(a => a.Kind == AllocationKind.Electricity).Sum(a => a.OutstandingAmount);

        // Electricity kWh from MeterReading.ConsumptionKwh :contentReference[oaicite:10]{index=10}
        decimal kwh = currentAccruals
            .Where(a => a.Kind == AllocationKind.Electricity && a.MeterReading != null)
            .Sum(a => a.MeterReading!.ConsumptionKwh);

        // Prepared by user (from accruals)
        int? createdBy = openAccruals
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => (int?)a.CreatedByUserId)
            .FirstOrDefault();

        var preparedByName = createdBy.HasValue ? _userNames.GetDisplayName(createdBy.Value) : "";

        // Formatting helpers
        string FmtMoney(decimal x)
        {
            var s = x.ToString("N0", CultureInfo.InvariantCulture);
            return _time.RtlSafeLatin($"{s} ؋");
        }

        string FmtKwh(decimal x)
        {
            var s = x.ToString("N1", CultureInfo.InvariantCulture);
            return _time.RtlSafeLatin($"{s} kWh");
        }

        string Period(int y, int m) => _time.RtlSafeLatin($"{m:00}/{y}");

        var floor = c.Shop?.Floor;
        var floorText = floor == null
            ? ""
            : (!string.IsNullOrWhiteSpace(floor.FloorName) ? floor.FloorName : $"طبقه {floor.Level}");

        var model = new ContractDebtStatusA6Model
        {
            MarketName = c.Shop?.Market?.MarketName ?? "",
            FloorText = floorText,
            ShopName = !string.IsNullOrWhiteSpace(c.Shop?.Name) ? c.Shop!.Name! : (c.Shop?.UnitNumber ?? ""),
            BusinessActivity = c.BusinessActivity ?? "",
            TenantName = c.Tenant?.DisplayName ?? "",

            HasFranchiseOwners = activeOwners.Count > 0,
            FranchiseOwnersText = activeOwners.Count > 0 ? $"امتیاز: {ownersLabel}" : "",

            ReportWeekday = WeekdayDari(ToKabul(nowUtc).DayOfWeek),
            ReportDateBig = _time.FormatPersianDateWithMonthNameDash(nowUtc),
            ReportTime = _time.FormatTimeAmPm(nowUtc),

            OldDebtTotal = FmtMoney(oldDebtTotal),
            CurrentDebtTotal = FmtMoney(currentTotal),
            TotalDebt = FmtMoney(oldDebtTotal + currentTotal),

            ShowOldDebtDetail = oldGroups.Count > 0,
            OldDebtLines = oldGroups
                .Select(x => new ContractDebtPeriodLineModel
                {
                    Period = Period(x.Year, x.Month),
                    Amount = FmtMoney(x.Amount)
                })
                .ToList(),

            CurrentPeriod = _time.RtlSafeLatin($"{py}/{pm:00}"),

            CurrentOwnerPortion = FmtMoney(currOwner),
            CurrentMarketPortion = FmtMoney(currMarket),
            CurrentFranchisePortion = FmtMoney(currFranchise),

            CurrentElectricityKwh = FmtKwh(kwh),
            CurrentElectricityAmount = FmtMoney(currElectricity),

            PreparedByUserName = preparedByName ?? "",
            DisclaimerText = "این سند بدون امضا و مهر معتبر است."
        };

        return model;
    }

    public IDataResult<byte[]> GenerateContractPeriodStatementA6(
       Guid contractId,
       DateOnly startDate,
       DateOnly endDate,
       PdfRenderOptions options,
       string? stampInfoText)
    {
        var c = LoadContractForPeriodStatement(contractId);
        if (c is null)
            return new ErrorDataResult<byte[]>(null, "یافت نشد.");

        var model = MapToContractPeriodStatementModel(c, startDate, endDate, stampInfoText);

        var bytes = _pdf.Render("contract-period-statement-a6", model, options);
        return new SuccessDataResult<byte[]>(bytes);
    }

    public IDataResult<byte[]> GenerateContractPeriodStatementsA6(
        IReadOnlyList<Guid> contractIds,
        DateOnly startDate,
        DateOnly endDate,
        PdfRenderOptions options,
        string? stampInfoText)
    {
        if (contractIds is null || contractIds.Count == 0)
            return new ErrorDataResult<byte[]>(null, "لیست خالی است.");

        var list = _contractDal.QueryNoTracking()
            .Where(x => contractIds.Contains(x.Id))
            .Include(x => x.Shop).ThenInclude(s => s.Market)
            .Include(x => x.Shop).ThenInclude(s => s.Floor)
            .Include(x => x.Shop).ThenInclude(s => s.ShopFranchiseShares).ThenInclude(ss => ss.FranchiseOwner) // ✅
            .Include(x => x.Tenant)
            .Include(x => x.Accruals).ThenInclude(a => a.MeterReading)
            .ToList();


        var foundIds = list.Select(x => x.Id).ToHashSet();
        var missing = contractIds.Where(id => !foundIds.Contains(id)).ToList();
        if (missing.Count > 0)
            return new ErrorDataResult<byte[]>(null, $"برخی قراردادها یافت نشد. Count: {missing.Count}");

        var dict = list.ToDictionary(x => x.Id, x => x);

        var models = new List<ContractPeriodStatementA6Model>(contractIds.Count);
        foreach (var id in contractIds)
            models.Add(MapToContractPeriodStatementModel(dict[id], startDate, endDate, stampInfoText));

        var bytes = _pdf.RenderMany("contract-period-statement-a6", models, options);
        return new SuccessDataResult<byte[]>(bytes);
    }

    private Contract? LoadContractForPeriodStatement(Guid contractId)
    {
        // Contract has Accruals navigation :contentReference[oaicite:5]{index=5}
        return _contractDal.QueryNoTracking()
            .Where(x => x.Id == contractId)
            .Include(x => x.Shop).ThenInclude(s => s.Market)
            .Include(x => x.Shop).ThenInclude(s => s.Floor)
            .Include(x => x.Shop).ThenInclude(s => s.ShopFranchiseShares).ThenInclude(ss => ss.FranchiseOwner) // ✅
            .Include(x => x.Tenant)
            .Include(x => x.Accruals).ThenInclude(a => a.MeterReading)
            .FirstOrDefault();

    }

    private ContractPeriodStatementA6Model MapToContractPeriodStatementModel(
        Contract c,
        DateOnly startDate,
        DateOnly endDate,
        string? stampInfoText) // parametre artık kullanılmıyor, signature kalsın istersen
    {
        // Normalize input order
        var s = startDate <= endDate ? startDate : endDate;
        var e = startDate <= endDate ? endDate : startDate;

        // "25'in ayını al" kuralı: her Gregorian ayın 25'i üzerinden Persian y/m hesapla
        var startYm = ToPersianYearMonth_ByDay25(s);
        var endYm = ToPersianYearMonth_ByDay25(e);

        // current persian y/m (for zero-fill in future)
        var (curY, curM, _) = _time.TodayPersian();

        // Create month sequence (inclusive)
        var months = EnumeratePersianMonths(startYm.year, startYm.month, endYm.year, endYm.month);

        // Accrual data
        var accruals = (c.Accruals ?? new List<Accrual>()).ToList();

        // Prepared by user: pick latest accrual
        var latestUserId = accruals
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => (int?)a.CreatedByUserId)
            .FirstOrDefault();

        var preparedBy = latestUserId.HasValue ? _userNames.GetDisplayName(latestUserId.Value) : "";

        // Franchise owners (active today)
        var refDate = _time.Today(); // DateOnly
        var activeOwners = c.Shop?.ShopFranchiseShares?
            .Where(sx => sx.EffectiveFrom <= refDate && (sx.EffectiveTo == null || refDate <= sx.EffectiveTo.Value))
            .OrderByDescending(sx => sx.SharePercent)
            .Select(sx => sx.FranchiseOwner.DisplayName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .ToList() ?? new List<string>();

        string ownersLabel = "";
        if (activeOwners.Count == 1)
            ownersLabel = activeOwners[0];
        else if (activeOwners.Count > 1)
            ownersLabel = $"{activeOwners[0]} و {activeOwners.Count - 1} نفر دیگر";

        // Format helpers
        string Money(decimal x) => _time.RtlSafeLatin($"{x.ToString("N0", CultureInfo.InvariantCulture)} ؋");
        string Kwh(decimal x) => _time.RtlSafeLatin($"{x.ToString("N1", CultureInfo.InvariantCulture)} kWh");
        string Period(int y, int m) => _time.RtlSafeLatin($"{y}/{m:00}");

        // Build lines + compute remaining sum (no string-parse)
        var lines = new List<ContractPeriodStatementA6LineModel>(months.Count);
        int i = 1;
        decimal rangeRemaining = 0m;

        foreach (var (y, m) in months)
        {
            bool isFuture = (y > curY) || (y == curY && m > curM);

            if (isFuture)
            {
                lines.Add(new ContractPeriodStatementA6LineModel
                {
                    RowNo = _time.RtlSafeLatin(i++.ToString()),
                    Period = Period(y, m),
                    Kwh = Kwh(0),
                    TotalDebt = Money(0),
                    Paid = Money(0),
                    Remaining = Money(0),
                });
                continue;
            }

            var monthAccruals = accruals.Where(a => a.Year == y && a.Month == m).ToList();

            decimal total = monthAccruals.Sum(a => a.Amount);
            decimal remaining = monthAccruals.Sum(a => a.OutstandingAmount);
            decimal paid = total - remaining;

            decimal kwh = monthAccruals
                .Where(a => a.MeterReading != null)
                .Sum(a => a.MeterReading!.ConsumptionKwh);

            rangeRemaining += remaining;

            lines.Add(new ContractPeriodStatementA6LineModel
            {
                RowNo = _time.RtlSafeLatin(i++.ToString()),
                Period = Period(y, m),
                Kwh = Kwh(kwh),
                TotalDebt = Money(total),
                Paid = Money(paid),
                Remaining = Money(remaining),
            });
        }

        // Floor text
        var floor = c.Shop?.Floor;
        var floorText = floor == null
            ? ""
            : (!string.IsNullOrWhiteSpace(floor.FloorName) ? floor.FloorName : $"طبقه {floor.Level}");

        var nowUtc = _time.UtcNow();

        var model = new ContractPeriodStatementA6Model
        {
            MarketName = c.Shop?.Market?.MarketName ?? "",
            FloorText = floorText,
            ShopName = !string.IsNullOrWhiteSpace(c.Shop?.Name) ? c.Shop!.Name! : (c.Shop?.UnitNumber ?? ""),
            TenantName = c.Tenant?.DisplayName ?? "",
            BusinessActivity = c.BusinessActivity ?? "",

            // NEW
            FranchiseOwnerText = ownersLabel,

            ReportWeekday = WeekdayDari(ToKabul(nowUtc).DayOfWeek),
            ReportDateBig = _time.FormatPersianDateWithMonthNameDash(nowUtc),
            ReportTime = _time.FormatTimeAmPm(nowUtc),

            DebtStatusText = _time.RtlSafeLatin($"جمله باقی: {Money(rangeRemaining)}"),

            // RTL range text (start -> end)
            RangeText = _time.RtlSafeLatin(
                $"{Period(endYm.year, endYm.month)} تا تاریخ ___ {Period(startYm.year, startYm.month)} از تاریخ"
            ),

            Lines = lines,

            PreparedByUserName = preparedBy ?? "",

            // Parametre ile gelmeyecek -> sabit
            StampInfoText = "این سند می‌تواند بدون مهر/امضا نیز استفاده شود."
        };

        return model;
    }


    private static (int year, int month) ToPersianYearMonth_ByDay25(DateOnly d)
    {
        int day = Math.Min(25, DateTime.DaysInMonth(d.Year, d.Month));
        var dt = new DateTime(d.Year, d.Month, day, 12, 0, 0, DateTimeKind.Unspecified);

        var pc = new PersianCalendar();
        return (pc.GetYear(dt), pc.GetMonth(dt));
    }

    private static List<(int y, int m)> EnumeratePersianMonths(int sy, int sm, int ey, int em)
    {
        var list = new List<(int y, int m)>();
        int y = sy, m = sm;

        while (y < ey || (y == ey && m <= em))
        {
            list.Add((y, m));
            m++;
            if (m == 13) { m = 1; y++; }
        }
        return list;
    }

}
