using Business.Abstract;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Validation;
using Core.Extensions;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos.Employee;
using Microsoft.EntityFrameworkCore;

namespace Business.Concrete
{
    public class EmployeeManager : IEmployeeService
    {
        private readonly IEmployeeDal _employeeDal;
        private readonly IEmployeeWorkInfoDal _workDal;
        private readonly IPhoneDal _phoneDal;
        private readonly IAddressDal _addrDal;
        private readonly IMediaAssetDal _mediaDal;
        private readonly IExpenseDal _expenseDal;
        private readonly IMoneyEntryDal _moneyEntryDal;
        private readonly IMoneyAccountDal _moneyAccountDal;
        private readonly IBiometricDal _biometricDal;

        public EmployeeManager(
            IEmployeeDal employeeDal,
            IEmployeeWorkInfoDal workDal,
            IPhoneDal phoneDal,
            IAddressDal addrDal,
            IMediaAssetDal mediaDal,
            IExpenseDal expenseDal,
            IMoneyEntryDal moneyEntryDal,
            IMoneyAccountDal moneyAccountDal,
            IBiometricDal biometricDal)
        {
            _employeeDal = employeeDal;
            _workDal = workDal;
            _phoneDal = phoneDal;
            _addrDal = addrDal;
            _mediaDal = mediaDal;
            _expenseDal = expenseDal;
            _moneyEntryDal = moneyEntryDal;
            _moneyAccountDal = moneyAccountDal;
            _biometricDal = biometricDal;
        }

        public IDataResult<PagedResult<EmployeeSalaryPaymentListItemDto>> GetSalaryPayments(Guid employeeId, QueryOptions opt)
        {
            opt ??= new QueryOptions();

            var q = _expenseDal.QueryNoTracking()
                .Where(x => x.EmployeeId == employeeId && x.Category == ExpenseCategory.Salary && x.IsActive);

            q = q.ApplyFilters(opt.Filters);
            q = q.ApplySort(opt.Sort);
            q = q.OrderByDescending(x => x.OccurredAtUtc).ThenByDescending(x => x.CreatedAtUtc);

            var total = q.Count();

            var page = q.ApplyPaging(opt.Page, opt.PageSize)
                .Select(x => new
                {
                    x.Id,
                    x.MoneyEntryId,
                    x.OccurredAtUtc,
                    x.Amount,
                    x.PaymentMethod,
                    x.DisplayName,
                    x.Notes,
                    x.CreatedAtUtc,
                    x.CreatedByUserId
                })
                .ToList();

            var moneyEntryIds = page.Where(x => x.MoneyEntryId.HasValue).Select(x => x.MoneyEntryId!.Value).ToList();

            var entries = _moneyEntryDal.QueryNoTracking()
                .Where(m => moneyEntryIds.Contains(m.Id))
                .Select(m => new
                {
                    m.Id,
                    m.MoneyAccountId,
                    m.Currency,
                    m.AfnPerUsdAtThatTime,
                    m.UsdEquivalent
                })
                .ToList();

            var accIds = entries.Select(x => x.MoneyAccountId).Distinct().ToList();
            var accounts = _moneyAccountDal.QueryNoTracking()
                .Where(a => accIds.Contains(a.Id))
                .Select(a => new { a.Id, a.Name })
                .ToList();

            var mapEntry = entries.ToDictionary(x => x.Id, x => x);
            var mapAcc = accounts.ToDictionary(x => x.Id, x => x.Name);

            var items = page.Select(x =>
            {
                var meId = x.MoneyEntryId ?? Guid.Empty;
                mapEntry.TryGetValue(meId, out var me);
                var accId = me != null ? me.MoneyAccountId : Guid.Empty;
                var accName = (accId != Guid.Empty && mapAcc.TryGetValue(accId, out var nm)) ? nm : "";

                return new EmployeeSalaryPaymentListItemDto
                {
                    ExpenseId = x.Id,
                    MoneyEntryId = meId,
                    OccurredAtUtc = x.OccurredAtUtc,
                    Amount = x.Amount,
                    Currency = me != null ? me.Currency : Currency.Afn,
                    MoneyAccountId = accId,
                    MoneyAccountName = accName,
                    PaymentMethod = x.PaymentMethod,
                    DisplayName = x.DisplayName,
                    Notes = x.Notes,
                    AfnPerUsdAtThatTime = me != null ? me.AfnPerUsdAtThatTime : 0m,
                    UsdEquivalent = me != null ? me.UsdEquivalent : 0m,
                    CreatedAtUtc = x.CreatedAtUtc,
                    CreatedByUserId = x.CreatedByUserId
                };
            }).ToList();

            return new SuccessDataResult<PagedResult<EmployeeSalaryPaymentListItemDto>>(new PagedResult<EmployeeSalaryPaymentListItemDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            });
        }

        [CacheAspect(10)]
        public IDataResult<Employee> GetById(Guid id)
        {
            var e = _employeeDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<Employee>(null, "یافت نشد.")
                : new SuccessDataResult<Employee>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<EmployeeDetailDto>> GetDetails(QueryOptions opt, bool? active = null)
        {
            opt ??= new QueryOptions();

            var q = _employeeDal.QueryNoTracking();
            if (active.HasValue) q = q.Where(x => x.IsActive == active.Value);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                var like = $"%{key}%";
                q = q.Where(x =>
                    EF.Functions.Like(x.DisplayName, like) ||
                    EF.Functions.Like(x.FatherName, like) ||
                    (x.NationalId != null && EF.Functions.Like(x.NationalId, like))
                );
            }

            q = q.ApplySort(opt.Sort);
            q = q.OrderByDescending(x => x.IsActive).ThenBy(x => x.DisplayName);

            var total = q.Count();

            var page = q
                .ApplyPaging(opt.Page, opt.PageSize)
                .Select(e => new
                {
                    e.Id,
                    e.DisplayName,
                    e.FatherName,
                    e.Gender,
                    e.NationalId,
                    e.IsActive
                })
                .ToList();

            var ids = page.Select(x => x.Id).ToList();

            var workAgg = _workDal.QueryNoTracking()
                .Where(w => ids.Contains(w.EmployeeId))
                .GroupBy(w => w.EmployeeId)
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    JobTitle = g.OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc).Select(x => x.JobTitle).FirstOrDefault(),
                    SalaryAmount = g.OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc).Select(x => x.SalaryAmount).FirstOrDefault()
                })
                .ToList();

            var mapJob = workAgg.ToDictionary(x => x.EmployeeId, x => x.JobTitle ?? "");
            var mapSalary = workAgg.ToDictionary(x => x.EmployeeId, x => x.SalaryAmount);

            var ownerType = MediaOwnerType.Employee;

            // phone
            var phoneRows = _phoneDal.QueryNoTracking()
                .Where(p => p.OwnerType == ownerType && ids.Contains(p.OwnerId))
                .Select(p => new { p.OwnerId, p.Number, p.IsPrimary, p.Id })
                .ToList();

            var phoneMap = phoneRows
                .GroupBy(x => x.OwnerId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.IsPrimary)
                          .ThenBy(x => x.Id)
                          .Select(x => x.Number)
                          .FirstOrDefault() ?? ""
                );

            // avatar
            var avatarRows = _mediaDal.QueryNoTracking()
                .Where(m => m.OwnerType == ownerType && ids.Contains(m.OwnerId) && m.Kind == MediaKind.Avatar)
                .Select(m => new { m.OwnerId, m.StorageUrl, m.IsPrimary, m.Id })
                .ToList();

            var avatarMap = avatarRows
                .GroupBy(x => x.OwnerId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.IsPrimary)
                          .ThenByDescending(x => x.Id)
                          .Select(x => x.StorageUrl)
                          .FirstOrDefault() ?? ""
                );


            var items = page.Select(x => new EmployeeDetailDto
            {
                Id = x.Id.ToString(),
                DisplayName = x.DisplayName,
                FatherName = x.FatherName,
                Gender = x.Gender,
                NationalId = x.NationalId ?? "",
                IsActive = x.IsActive,
                JobTitle = mapJob.TryGetValue(x.Id, out var jt) ? jt : "",
                SalaryAmount = mapSalary.TryGetValue(x.Id, out var sa) ? sa : null,
                PhonePrimary = phoneMap.TryGetValue(x.Id, out var ph) ? (ph ?? "") : "",
                AvatarUrl = avatarMap.TryGetValue(x.Id, out var av) ? (av ?? "") : "",
            }).ToList();

            return new SuccessDataResult<PagedResult<EmployeeDetailDto>>(new PagedResult<EmployeeDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            });
        }

        [CacheAspect(10)]
        public IDataResult<EmployeeProfileDto> GetProfile(Guid id)
        {
            var ownerType = MediaOwnerType.Employee;

            var q =
                from e in _employeeDal.QueryNoTracking()
                where e.Id == id

                let phonePrimary =
                    (from p in _phoneDal.QueryNoTracking()
                     where p.OwnerType == ownerType && p.OwnerId == e.Id
                     orderby p.IsPrimary descending, p.Id ascending
                     select p.Number).FirstOrDefault()

                let phoneSecondary =
                    (from p in _phoneDal.QueryNoTracking()
                     where p.OwnerType == ownerType && p.OwnerId == e.Id
                     orderby p.IsPrimary descending, p.Id ascending
                     select p.Number).Skip(1).FirstOrDefault()

                let addr =
                    (from a in _addrDal.QueryNoTracking()
                     where a.OwnerType == ownerType && a.OwnerId == e.Id
                     orderby a.IsPrimary descending
                     select new
                     {
                         Province = a.Province != null ? a.Province.Name : null,
                         District = a.District != null ? a.District.Name : null,
                         a.Line1
                     }).FirstOrDefault()

                let avatarUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == e.Id && m.Kind == MediaKind.Avatar
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let idFrontUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == e.Id && m.Kind == MediaKind.IdFront
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let idBackUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == e.Id && m.Kind == MediaKind.IdBack
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let fingerUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == e.Id && m.Kind == MediaKind.FingerBitmap
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let work =
                    (from w in _workDal.QueryNoTracking()
                     where w.EmployeeId == e.Id
                     orderby (w.UpdatedAtUtc ?? w.CreatedAtUtc) descending
                     select new EmployeeWorkInfoDto
                     {
                         EmployeeId = w.EmployeeId,
                         JobTitle = w.JobTitle,
                         SalaryAmount = w.SalaryAmount,
                         SalaryCurrency = (Currency)w.SalaryCurrency,
                         PayDay = w.PayDay,
                         Interest = w.Interest,
                         Taxes = w.Taxes,
                         Notes = w.Notes,
                         IsActive = w.IsActive
                     }).FirstOrDefault()

                select new EmployeeProfileDto
                {
                    Id = e.Id.ToString(),
                    DisplayName = e.DisplayName,
                    FatherName = e.FatherName,
                    Gender = e.Gender,
                    NationalId = e.NationalId ?? "",
                    IsActive = e.IsActive,

                    PhonePrimary = phonePrimary ?? "",
                    PhoneSecondary = phoneSecondary ?? "",

                    Province = addr != null ? (addr.Province ?? "") : "",
                    District = addr != null ? (addr.District ?? "") : "",
                    AddressLine = addr != null ? (addr.Line1 ?? "") : "",
                    AddressFull =
                        (addr == null) ? "" :
                        ((addr.Province ?? "") +
                         ((addr.Province != null && addr.District != null) ? ", " : "") +
                         (addr.District ?? "") +
                         (((addr.Province != null || addr.District != null) && addr.Line1 != null) ? " - " : "") +
                         (addr.Line1 ?? "")),

                    AvatarUrl = avatarUrl ?? "",
                    IdFrontUrl = idFrontUrl ?? "",
                    IdBackUrl = idBackUrl ?? "",
                    FingerBitmapUrl = fingerUrl ?? "",

                    WorkInfo = work
                };

            var dto = q.FirstOrDefault();
            if (dto == null) return new ErrorDataResult<EmployeeProfileDto>(null, "یافت نشد.");
            return new SuccessDataResult<EmployeeProfileDto>(dto);
        }

        public IResult Add(Employee employee)
        {
            if (employee == null) return new ErrorResult("داده نامعتبر است.");
            if (employee.Id == Guid.Empty) employee.Id = Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(employee.DisplayName)) return new ErrorResult("نام الزامی است.");
            if (string.IsNullOrWhiteSpace(employee.FatherName)) employee.FatherName = "—";

            employee.NationalId ??= "";
            employee.IsActive = true;

            _employeeDal.Add(employee);
            return new SuccessResult("با موفقیت ایجاد شد.");
        }

        public IResult Update(Employee employee)
        {
            if (employee == null || employee.Id == Guid.Empty) return new ErrorResult("داده نامعتبر است.");

            var existing = _employeeDal.Get(x => x.Id == employee.Id);
            if (existing == null) return new ErrorResult("یافت نشد.");

            existing.DisplayName = employee.DisplayName;
            existing.FatherName = employee.FatherName;
            existing.Gender = employee.Gender;
            existing.NationalId = employee.NationalId ?? "";
            existing.IsActive = employee.IsActive;

            _employeeDal.Update(existing);
            return new SuccessResult("با موفقیت بروزرسانی شد.");
        }

        public IResult Delete(Guid id)
        {
            var existing = _employeeDal.Get(x => x.Id == id);
            if (existing == null) return new ErrorResult("یافت نشد.");
            _employeeDal.Delete(existing);
            return new SuccessResult("حذف شد.");
        }

        public IResult Activate(Guid id)
        {
            var e = _employeeDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            if (e.IsActive) return new SuccessResult("قبلاً فعال است.");
            e.IsActive = true;
            _employeeDal.Update(e);
            return new SuccessResult("فعال شد.");
        }

        public IResult Deactivate(Guid id)
        {
            var e = _employeeDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            if (!e.IsActive) return new SuccessResult("قبلاً غیرفعال است.");
            e.IsActive = false;
            _employeeDal.Update(e);
            return new SuccessResult("غیرفعال شد.");
        }

        public IDataResult<EmployeeWorkInfoDto> GetWorkInfo(Guid employeeId)
        {
            var w = _workDal.QueryNoTracking()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
                .Select(x => new EmployeeWorkInfoDto
                {
                    EmployeeId = x.EmployeeId,
                    JobTitle = x.JobTitle,
                    SalaryAmount = x.SalaryAmount,
                    SalaryCurrency = (Currency)x.SalaryCurrency,
                    PayDay = x.PayDay,
                    Interest = x.Interest,
                    Taxes = x.Taxes,
                    Notes = x.Notes,
                    IsActive = x.IsActive
                })
                .FirstOrDefault();

            return w == null
                ? new ErrorDataResult<EmployeeWorkInfoDto>(null, "ثبت نشده است.")
                : new SuccessDataResult<EmployeeWorkInfoDto>(w);
        }

        [CacheRemoveAspect(".*Employee.*Get.*")]
        public IResult UpsertWorkInfo(Guid employeeId, EmployeeWorkInfoUpsertDto dto, int userId)
        {
            if (employeeId == Guid.Empty) return new ErrorResult("شناسه کارمند نامعتبر است.");
            if (dto == null) return new ErrorResult("داده نامعتبر است.");

            // DTO safety: SalaryAmount is nullable on UI. In DB it is required.
            var salary = dto.SalaryAmount ?? 0m;
            if (salary < 0) salary = 0m;

            var payDay = dto.PayDay;
            if (payDay < 1) payDay = 1;
            if (payDay > 31) payDay = 31;

            var interest = dto.Interest;
            if (interest < 0) interest = 0m;

            var taxes = dto.Taxes;
            if (taxes < 0) taxes = 0m;

            var exists = _employeeDal.QueryNoTracking().Any(x => x.Id == employeeId);
            if (!exists) return new ErrorResult("کارمند یافت نشد.");

            var now = DateTime.UtcNow;

            var current = _workDal.Query().FirstOrDefault(x => x.EmployeeId == employeeId);
            if (current == null)
            {
                current = new EmployeeWorkInfo
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employeeId,
                    JobTitle = dto.JobTitle?.Trim() ?? "",
                    SalaryAmount = salary,
                    SalaryCurrency = (int)dto.SalaryCurrency,
                    PayDay = payDay,
                    Interest = interest,
                    Taxes = taxes,
                    Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                    IsActive = dto.IsActive,
                    CreatedByUserId = userId,
                    CreatedAtUtc = now,
                    UpdatedByUserId = null,
                    UpdatedAtUtc = null
                };
                _workDal.Add(current);
                return new SuccessResult("ثبت شد.");
            }

            current.JobTitle = dto.JobTitle?.Trim() ?? "";
            current.SalaryAmount = salary;
            current.SalaryCurrency = (int)dto.SalaryCurrency;
            current.PayDay = payDay;
            current.Interest = interest;
            current.Taxes = taxes;
            current.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
            current.IsActive = dto.IsActive;
            current.UpdatedByUserId = userId;
            current.UpdatedAtUtc = now;

            _workDal.Update(current);
            return new SuccessResult("بروزرسانی شد.");
        }

        [CacheRemoveAspect(".*Employee.*Get.*")]
        public async Task<IResult> UpsertFingerprintAsync(Guid employeeId, EmployeeFingerprintUpsertDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.TemplateBase64))
                return new ErrorResult("TemplateBase64 الزامی است.");

            var exists = await _employeeDal.QueryNoTracking().AnyAsync(x => x.Id == employeeId);
            if (!exists) return new ErrorResult("کارمند یافت نشد.");

            var templateBytes = DecodeBase64(dto.TemplateBase64);

            // Deactivate existing active fingerprints
            var actives = await _biometricDal.Query()
                .Where(x => x.OwnerType == MediaOwnerType.Employee
                         && x.OwnerId == employeeId
                         && x.Type == BiometricType.Fingerprint
                         && x.IsActive)
                .ToListAsync();

            foreach (var a in actives)
            {
                a.IsActive = false;
                _biometricDal.Update(a);
            }

            var bio = new Biometric
            {
                Id = Guid.NewGuid(),
                OwnerType = MediaOwnerType.Employee,
                OwnerId = employeeId,
                Type = BiometricType.Fingerprint,
                Template = templateBytes,
                RelatedMediaAssetId = dto.RelatedMediaAssetId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _biometricDal.Add(bio);
            return new SuccessResult("اثر انگشت بروزرسانی شد.");
        }

        private static byte[] DecodeBase64(string input)
        {
            var s = (input ?? "").Trim();
            var idx = s.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) s = s.Substring(idx + "base64,".Length);
            return Convert.FromBase64String(s);
        }
    }
}
