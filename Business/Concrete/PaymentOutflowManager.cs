using Business.Abstract;
using Core.Aspects.Autofac.Transaction;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete;
using Entities.Dtos.FranchiseOwner.entry;
using Entities.Dtos.MoneyEntry;
using Entities.Dtos.PaymentOutflow;

namespace Business.Concrete
{
    public class PaymentOutflowManager : IPaymentOutflowService
    {
        private readonly IImageStorageService _imageStorage;
        private readonly IMediaAssetService _mediaAssetService;
        private readonly IBiometricService _biometricService;

        private readonly IMoneyEntryService _moneyEntryService;
        private readonly IFranchiseBalanceEntryService _franchiseBalanceEntryService;
        private readonly ITimeProvider _timeProvider;
        private readonly IMoneyAccountDal _moneyAccountDal;
        private readonly IFamilyBalanceEntryDal _familyBalanceEntryDal;
        private readonly IFamilyBalanceEntryService _familyBalanceEntryService;
        private readonly IExpenseDal _expenseDal;
        private readonly IEmployeeTransactionDal _employeeTransactionDal;


        public PaymentOutflowManager(
            IImageStorageService imageStorage,
            IMediaAssetService mediaAssetService,
            IBiometricService biometricService,
            IMoneyEntryService moneyEntryService,
            IFranchiseBalanceEntryService franchiseBalanceEntryService,
            ITimeProvider timeProvider,
            IMoneyAccountDal moneyAccountDal,
            IFamilyBalanceEntryService familyBalanceEntryService,
            IExpenseDal expenseDal,
            IEmployeeTransactionDal employeeTransactionDal

            )
        {
            _imageStorage = imageStorage;
            _mediaAssetService = mediaAssetService;
            _biometricService = biometricService;
            _moneyEntryService = moneyEntryService;
            _franchiseBalanceEntryService = franchiseBalanceEntryService;
            _timeProvider = timeProvider;
            _moneyAccountDal = moneyAccountDal;
            _familyBalanceEntryService = familyBalanceEntryService;
            _expenseDal = expenseDal;
            _employeeTransactionDal = employeeTransactionDal;
        }

        [TransactionScopeAspect]
        public async Task<IDataResult<PaymentOutflowCreateResultDto>> CreateAsync(
            PaymentOutflowCreateFromUiDto dto,
            int userId,
            CancellationToken ct)
                {
                    // ---- 0) اعتبارسنجی ----
                    if (dto.OwnerId == Guid.Empty)
                        return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, "شناسه صاحب امتیاز الزامی است.");

                    var amount = dto.Amount ?? 0m;
                    if (amount <= 0m)
                        return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, "مبلغ نامعتبر است.");

                    var now = _timeProvider.UtcNow();

                    // moneyAccountId resolve (AccountType=1, Currency=dto.Currency, IsActive=true)
                    var account = _moneyAccountDal.QueryNoTracking()
                        .Where(x => x.IsActive && (int)x.AccountType == 1 && x.Currency == dto.Currency)
                        .Select(x => new { x.Id })
                        .FirstOrDefault();

                    if (account == null || account.Id == Guid.Empty)
                        return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, "حساب پیش‌فرض برای این ارز یافت نشد.");

                    var moneyAccountId = account.Id;

                    // PaymentId
                    var paymentId = dto.EntryId == Guid.Empty ? Guid.NewGuid() : dto.EntryId;
                    var moneyEntryId = Guid.NewGuid();
                    var domainEntryId = Guid.NewGuid();

                    // Fingerprint policy
                    var hasAnyFp =
                        !string.IsNullOrWhiteSpace(dto.FingerprintBitmapPngBase64) ||
                        !string.IsNullOrWhiteSpace(dto.FingerprintTemplateBase64);

                    if (hasAnyFp && (string.IsNullOrWhiteSpace(dto.FingerprintBitmapPngBase64) ||
                                     string.IsNullOrWhiteSpace(dto.FingerprintTemplateBase64)))
                    {
                        return new ErrorDataResult<PaymentOutflowCreateResultDto>(
                            null,
                            "اگر اثرانگشت ارسال می‌شود، هم تصویر و هم قالب (Template) الزامی است.");
                    }

                    // occurredAt
                    var occurredAt = now;

                    // ---- 1) اثرانگشت (اگر وجود دارد) ----
                    Guid? fingerMediaId = null;
                    Guid? biometricId = null;
                    string? fingerBmpPath = null;

                    var fingerFolderUrl = $"/storage/payments/{paymentId}/fingers";

                    if (hasAnyFp)
                    {
                        var save = await _imageStorage.SaveIfDataUrlAsync(
                            photoFolderUrl: fingerFolderUrl,
                            baseFile: dto.FingerprintBitmapPngBase64!,
                            options: null,
                            ct: ct);

                        if (!save.Saved || string.IsNullOrWhiteSpace(save.RelativePath))
                            return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, $"ذخیره تصویر اثرانگشت ناموفق بود: {save.Error}");

                        fingerBmpPath = save.RelativePath;

                        fingerMediaId = Guid.NewGuid();
                        var media = new MediaAsset
                        {
                            Id = fingerMediaId.Value,
                            OwnerType = MediaOwnerType.Payment,
                            OwnerId = paymentId,
                            Kind = MediaKind.FingerBitmap,
                            StorageUrl = fingerBmpPath,
                            HashChecksum = "",
                            IsPrimary = false,
                            CreatedAt = now
                        };

                        var mr = _mediaAssetService.Add(media);
                        if (!mr.Success)
                            return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, mr.Message);

                        byte[] templateBytes;
                        try
                        {
                            templateBytes = Convert.FromBase64String(dto.FingerprintTemplateBase64!.Trim());
                        }
                        catch
                        {
                            return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, "قالب اثرانگشت (Base64) نامعتبر است.");
                        }

                        var bio = new Biometric
                        {
                            OwnerType = MediaOwnerType.Payment,
                            OwnerId = paymentId,
                            Type = BiometricType.Fingerprint,
                            Template = templateBytes,
                            RelatedMediaAssetId = fingerMediaId,
                            IsActive = true
                        };

                        var br = _biometricService.Add(bio);
                        if (!br.Success)
                            return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, br.Message);

                        biometricId = bio.Id;
                    }

                    // ---- 2) MoneyEntry ----
                    var moneyDto = new MoneyEntryCreateDto
                    {
                        Id = moneyEntryId,
                        MoneyAccountId = moneyAccountId,
                        Direction = BalanceDirection.Out,
                        Amount = amount,
                        Currency = dto.Currency,
                        ExpenseId = null,
                        CollectionId = null
                    };

                    var mer = _moneyEntryService.Add(moneyDto, userId);
                    if (!mer.Success)
                        return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, mer.Message);

                    // ---- 3) Domain entry ----
                    switch (dto.OutflowType)
                    {
                        case OutflowType.FranchiseOwnerPayout:
                            {
                                var fbeDto = new FranchiseBalanceEntryCreateDto
                                {
                                    Id = domainEntryId,
                                    FranchiseOwnerId = dto.OwnerId,
                                    Direction = BalanceDirection.Out,
                                    Amount = amount,
                                    OccurredAtUtc = occurredAt,
                                    Notes = dto.Description,
                                    PaymentMethod = dto.Method,
                                    PaymentReference = dto.Reference,
                                    MoneyEntryId = moneyEntryId
                                };

                                var fr = _franchiseBalanceEntryService.Add(fbeDto, userId);
                                if (!fr.Success)
                                    return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, fr.Message);

                                break;
                            }

                        case OutflowType.FamilyMemberPayout:
                            {
                                var entry = new FamilyBalanceEntry
                                {
                                    Id = domainEntryId,
                                    FamilyMemberId = dto.OwnerId,          // ✅ UI ownerId alanını bu tipte FamilyMemberId gibi kullanıyoruz
                                    Direction = BalanceDirection.Out,
                                    Amount = amount,
                                    OccurredAtUtc = occurredAt,

                                    PaymentMethod = dto.Method,
                                    PaymentReference = dto.Reference,
                                    Notes = dto.Description,

                                    CreatedByUserId = userId,
                                    CreatedAtUtc = now,
                                    IsActive = true
                                };

                                var fr = _familyBalanceEntryService.Add(entry);
                                if (!fr.Success)
                                    return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, fr.Message);

                                break;
                            }

                        case OutflowType.Expense:
                            {
                                // For employee salary payments we use Expense(Category=Salary, EmployeeId=dto.OwnerId)
                                var expense = new Expense
                                {
                                    Id = domainEntryId,
                                    DisplayName = string.IsNullOrWhiteSpace(dto.Description) ? "معاش کارمند" : dto.Description!.Trim(),
                                    Amount = amount,
                                    MoneyEntryId = null, // set after money entry created
                                    Category = ExpenseCategory.Salary,
                                    Source = ExpenseSource.Unknown,
                                    EmployeeId = dto.OwnerId,
                                    OccurredAtUtc = occurredAt,
                                    Notes = dto.Reference,
                                    PaymentMethod = dto.Method,
                                    CreatedByUserId = userId,
                                    CreatedAtUtc = now,
                                    IsActive = true
                                };

                                _expenseDal.Add(expense);

                                // MoneyEntry after (needs Expense row for FK)
                                var imoneyDto = new MoneyEntryCreateDto
                                {
                                    Id = moneyEntryId,
                                    MoneyAccountId = moneyAccountId,
                                    Direction = BalanceDirection.Out,
                                    Amount = amount,
                                    Currency = dto.Currency,
                                    ExpenseId = expense.Id,
                                    CollectionId = null
                                };

                                var imer = _moneyEntryService.Add(imoneyDto, userId);
                                if (!imer.Success)
                                    return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, imer.Message);

                                expense.MoneyEntryId = moneyEntryId;
                                _expenseDal.Update(expense);
                                break;
                            }

                case OutflowType.EmployeeSalary:
                    {
                        // 1) MoneyEntry (para çıkışı)
                        var emoneyDto = new MoneyEntryCreateDto
                        {
                            Id = moneyEntryId,
                            MoneyAccountId = moneyAccountId,
                            Direction = BalanceDirection.Out,
                            Amount = amount,
                            Currency = dto.Currency,
                            ExpenseId = null,
                            CollectionId = null
                        };

                        var emer = _moneyEntryService.Add(emoneyDto, userId);
                        if (!emer.Success)
                            return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, emer.Message);

                        // 2) EmployeeTransaction (çalışan ledger)
                        // EmployeeTransaction entity'nizde Year/Month 1300-1500 aralığı var -> Persian year/month kullanılmalı.
                        var (y, m, d) = _timeProvider.TodayPersian();

                        var et = new EmployeeTransaction
                        {
                            Id = domainEntryId,          // domain entry id burada kullanılabilir
                            EmployeeId = dto.OwnerId,
                            Direction = BalanceDirection.Out,
                            Amount = amount,
                            MoneyEntryId = moneyEntryId,
                            Year = y,
                            Month = m,
                            PaidAtUtc = occurredAt,
                            CreatedByUserId = userId
                        };

                        _employeeTransactionDal.Add(et);

                        break;
                    }


                default:
                                    return new ErrorDataResult<PaymentOutflowCreateResultDto>(null, "این نوع پرداخت هنوز پیاده‌سازی نشده است.");
                            }

                    return new SuccessDataResult<PaymentOutflowCreateResultDto>(new PaymentOutflowCreateResultDto
                    {
                        PaymentId = paymentId,
                        MoneyEntryId = moneyEntryId,
                        DomainEntryId = domainEntryId,
                        FingerprintFolderUrl = hasAnyFp ? fingerFolderUrl : null,
                        FingerprintBitmapPath = fingerBmpPath,
                        FingerprintMediaAssetId = fingerMediaId,
                        BiometricId = biometricId
                    }, "پرداخت با موفقیت ثبت شد.");
                }

            }
}
