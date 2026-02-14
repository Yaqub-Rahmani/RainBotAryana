using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Business.ValidationRules.FluentValidation.Collection;
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
using Entities.Dtos.Collection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CollectionManager : ICollectionService
    {

        private readonly ICollectionDal _collectionDal;
        private readonly ICollectionAllocationDal _collectionAllocationDal;
        private readonly IMoneyAccountDal _moneyAccountDal;
        private readonly IMoneyEntryDal _moneyEntryDal;
        private readonly IDollarRateDal _dollarRateDal;
        private readonly ITimeProvider _timeProvider;
        private readonly IAccrualDal _accrualDal;
        private readonly IAccrualSettlementDal _accrualSettlementDal;
        private readonly IContractDal _contractDal;

        private readonly IReserveAccountDal _reserveAccountDal;
        private readonly IReserveEntryDal _reserveEntryDal;
        private readonly ISingleRatePolicyDal _singleRatePolicyDal;

        private readonly IShopFranchiseShareDal _shopFranchiseShareDal;
        private readonly IFranchiseBalanceEntryDal _franchiseBalanceEntryDal;

        private readonly IOwnerDistributionPolicyDal _ownerDistributionPolicyDal;
        private readonly IOwnerDistributionDal _ownerDistributionDal;
        private readonly IOwnerDistributionLineDal _ownerDistributionLineDal;

        private readonly IFamilyMemberDal _familyMemberDal;
        private readonly IFamilyPayoutDal _familyPayoutDal;
        private readonly IFamilyPayoutLineDal _familyPayoutLineDal;
        private readonly IFamilyBalanceEntryDal _familyBalanceEntryDal;

        private readonly ITaxPaymentEntryDal _taxPaymentEntryDal;
        private readonly IZakatPaymentEntryDal _zakatPaymentEntryDal;
        private readonly ICharityPaymentEntryDal _charityPaymentEntryDal;

        private readonly IShopDal _shopDal;
        private readonly IMarketDal _marketDal;
        private readonly IFloorDal _floorDal;

        public CollectionManager(
            ICollectionDal collectionDal,
            IMoneyAccountDal moneyAccountDal,
            IMoneyEntryDal moneyEntryDal,
            IDollarRateDal dollarRateDal,
            ITimeProvider timeProvider,
            IAccrualDal accrualDal,
            IAccrualSettlementDal accrualSettlementDal,
            IContractDal contractDal,
            ICollectionAllocationDal collectionAllocationDal,
            IReserveAccountDal reserveAccountDal,
            IReserveEntryDal reserveEntryDal,
            ISingleRatePolicyDal singleRatePolicyDal,
            IShopFranchiseShareDal shopFranchiseShareDal,
            IFranchiseBalanceEntryDal franchiseBalanceEntryDal,
            IOwnerDistributionPolicyDal ownerDistributionPolicyDal,
            IOwnerDistributionDal ownerDistributionDal,
            IOwnerDistributionLineDal ownerDistributionLineDal,
            IFamilyMemberDal familyMemberDal,
            IFamilyPayoutDal familyPayoutDal,
            IFamilyPayoutLineDal familyPayoutLineDal,
            IFamilyBalanceEntryDal familyBalanceEntryDal,
            ITaxPaymentEntryDal taxPaymentEntryDal,
            IZakatPaymentEntryDal zakatPaymentEntryDal,
            ICharityPaymentEntryDal charityPaymentEntryDal,
            IShopDal shopDal,
            IMarketDal marketDal,
            IFloorDal floorDal)
        {
            _collectionDal = collectionDal;
            _moneyAccountDal = moneyAccountDal;
            _moneyEntryDal = moneyEntryDal;
            _dollarRateDal = dollarRateDal;
            _timeProvider = timeProvider;
            _accrualDal = accrualDal;
            _accrualSettlementDal = accrualSettlementDal;
            _contractDal = contractDal;
            _collectionAllocationDal = collectionAllocationDal;

            _reserveAccountDal = reserveAccountDal;
            _reserveEntryDal = reserveEntryDal;
            _singleRatePolicyDal = singleRatePolicyDal;

            _shopFranchiseShareDal = shopFranchiseShareDal;
            _franchiseBalanceEntryDal = franchiseBalanceEntryDal;

            _ownerDistributionPolicyDal = ownerDistributionPolicyDal;
            _ownerDistributionDal = ownerDistributionDal;
            _ownerDistributionLineDal = ownerDistributionLineDal;

            _familyMemberDal = familyMemberDal;
            _familyPayoutDal = familyPayoutDal;
            _familyPayoutLineDal = familyPayoutLineDal;
            _familyBalanceEntryDal = familyBalanceEntryDal;

            _taxPaymentEntryDal = taxPaymentEntryDal;
            _zakatPaymentEntryDal = zakatPaymentEntryDal;
            _charityPaymentEntryDal = charityPaymentEntryDal;

            _shopDal = shopDal;
            _marketDal = marketDal;
            _floorDal = floorDal;
        }


        [CacheAspect(10)]
        public IDataResult<CollectionDto> GetById(Guid id)
        {
            var e = _collectionDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (e == null)
                return new ErrorDataResult<CollectionDto>(null,"پرداخت یافت نشد.");

            return new SuccessDataResult<CollectionDto>(MapToDto(e));
        }

        [CacheAspect(10)]
        public IDataResult<List<CollectionDto>> GetByTenant(Guid tenantId)
        {
            var list = _collectionDal.QueryNoTracking()
                .Where(x => x.TenantId == tenantId)
                .OrderByDescending(x => x.PaidAtUtc)
                .ThenByDescending(x => x.Id)
                .Select(x => MapToDto(x))
                .ToList();

            return new SuccessDataResult<List<CollectionDto>>(list);
        }

        [ValidationAspect(typeof(CollectionCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("ICollectionService.Get"), CacheRemoveAspect("IDashboardService.GetFinanceDashboard")]
        [SecuredOperation("admin,collection.add")]
        public IResult Add(CollectionCreateDto dto, int userId)
        {
            // 1) Zaman
            var paidAt = dto.PaidAtUtc ?? _timeProvider.UtcNow();

            // 2) Hesabı bul (para nereye girdi?)
            var account = _moneyAccountDal.QueryNoTracking()
                .FirstOrDefault(x => x.Id == dto.MoneyAccountId);

            if (account == null)
                return new ErrorResult("حساب مقصد یافت نشد.");

            if (dto.AmountReceived <= 0)
                return new ErrorResult("مبلغ باید بزرگتر از صفر باشد.");

            // 3) Kur (DollarRate)
            var rate = _dollarRateDal.QueryNoTracking()
                .Where(r => r.EffectiveFromUtc <= paidAt)
                .OrderByDescending(r => r.EffectiveFromUtc)
                .ThenByDescending(r => r.Id)
                .FirstOrDefault();

            if (rate == null)
                return new ErrorResult("هنوز هیچ نرخ دالر ثبت نشده است.");

            if (account.Currency == Currency.Afn && rate.AfnPerUsd <= 0)
                return new ErrorResult("نرخ دالر نامعتبر است.");

            // 4) USD karşılık
            decimal usdEquivalent;
            if (account.Currency == Currency.Afn)
                usdEquivalent = Math.Round(dto.AmountReceived / rate.AfnPerUsd, 4);
            else // Currency.Usd
                usdEquivalent = dto.AmountReceived;

            // 5) Collection ve MoneyEntry Id’leri
            var collectionId = Guid.NewGuid();
            var moneyEntryId = Guid.NewGuid();
            var nowUtc = _timeProvider.UtcNow();

            // 6) MoneyEntry (dolar izi burada kalıyor)
            var moneyEntry = new MoneyEntry
            {
                Id = moneyEntryId,
                MoneyAccountId = account.Id,
                Direction = BalanceDirection.In,
                Amount = dto.AmountReceived,
                Currency = account.Currency,
                AfnPerUsdAtThatTime = rate.AfnPerUsd,
                UsdEquivalent = usdEquivalent,
                OccurredAtUtc = paidAt,
                CreatedByUserId = userId,
                CreatedAtUtc = nowUtc,
                CollectionId = collectionId,
                ExpenseId = null
            };

            // 7) Collection kaydı
            var entity = new Collection
            {
                Id = collectionId,
                TenantId = dto.TenantId,
                ShopId = dto.ShopId,
                ContractId = dto.ContractId,
                PaidAtUtc = paidAt,
                CreatedByUserId = userId,
                UpdatedByUserId = null,
                UpdatedAtUtc = null,
                ApprovedByUserId = null,
                ApprovedAtUtc = null,
                IsApproved = false,

                AmountReceived = dto.AmountReceived,
                Method = dto.Method,
                ReferenceNo = dto.ReferenceNo,

                MoneyEntryId = moneyEntryId,

                TargetYear = dto.TargetYear,
                TargetMonth = dto.TargetMonth,
                Notes = dto.Notes
            };

            // 8) DB işlemleri (transaction scope içerisinde)
            _collectionDal.Add(entity);
            _moneyEntryDal.Add(moneyEntry);

            return new SuccessResult("پرداخت ثبت شد و در حساب مالی درج گردید.");
        }

        private static CollectionDto MapToDto(Collection e)
        {
            return new CollectionDto
            {
                Id = e.Id,
                TenantId = e.TenantId,
                ShopId = e.ShopId,
                ContractId = e.ContractId,
                PaidAtUtc = e.PaidAtUtc,
                AmountReceived = e.AmountReceived,
                Method = e.Method,
                ReferenceNo = e.ReferenceNo,
                MoneyEntryId = e.MoneyEntryId,
                TargetYear = e.TargetYear,
                TargetMonth = e.TargetMonth,
                IsApproved = e.IsApproved,
                CreatedByUserId = e.CreatedByUserId,
                ApprovedByUserId = e.ApprovedByUserId,
                ApprovedAtUtc = e.ApprovedAtUtc,
                Notes = e.Notes
            };
        }

        [TransactionScopeAspect]
        [SecuredOperation("admin,collection.add")]
        public IResult AddContractDebtPayment(ContractDebtPaymentRequestDto dto, int userId)
        {
            var nowUtc = _timeProvider.UtcNow();

            // 1) Sözleşme kontrolü
            var contract = _contractDal.QueryNoTracking()
                .FirstOrDefault(c => c.Id == dto.ContractId);

            if (contract == null)
                return new ErrorResult("قرارداد یافت نشد.");

            if (dto.Payment == null)
                return new ErrorResult("اطلاعات پرداخت نامعتبر است.");

            if (dto.Allocations == null || dto.Allocations.Count == 0)
                return new ErrorResult("هیچ توزیعی برای بدهی‌ها ارسال نشده است.");

            if (dto.Payment.Amount <= 0)
                return new ErrorResult("مبلغ پرداخت باید بزرگتر از صفر باشد.");

            // 2) Toplam dağıtım tutarı ile ödeme tutarı eşit mi?
            var totalAlloc = dto.Allocations.Sum(x => x.Amount);
            if (totalAlloc != dto.Payment.Amount)
                return new ErrorResult("مبلغ توزیع شده با مبلغ پرداخت برابر نیست.");

            // 3) İlgili Accrual kayıtlarını çek
            var accrualIds = dto.Allocations.Select(x => x.DebtId).Distinct().ToList();

            var accruals = _accrualDal.Query()
                .Where(a => accrualIds.Contains(a.Id))
                .ToList();

            if (accruals.Count != accrualIds.Count)
                return new ErrorResult("برخی از بدهی‌های ارسال شده در سیستم یافت نشد.");

            // Hepsi aynı sözleşmeye mi bağlı?
            if (accruals.Any(a => a.ContractId != dto.ContractId))
                return new ErrorResult("بدهی‌های ارسال شده مربوط به قرارداد دیگری است.");

            // 4) Her Accrual için toplam dağıtım, kalan borcu aşmamalı
            var allocByAccrual = dto.Allocations
                .GroupBy(x => x.DebtId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

            foreach (var kv in allocByAccrual)
            {
                var accrual = accruals.First(a => a.Id == kv.Key);
                var sumForThisAccrual = kv.Value;

                if (sumForThisAccrual <= 0)
                    return new ErrorResult("مبلغ توزیع برای یک بدهی کوچکتر یا مساوی صفر است.");

                if (sumForThisAccrual > accrual.OutstandingAmount)
                    return new ErrorResult("مبلغ توزیعی از باقیمانده بدهی بیشتر است.");
            }

            // 5) Collection kaydı (şimdilik sadece başlık / taslak)
            var paidAt = dto.Payment.PaidAtUtc ?? nowUtc;
            var collectionId = Guid.NewGuid();

            var method = MapPaymentMethod(dto.Payment.Method); // "bank", "cash" vs. string → enum

            var collection = new Collection
            {
                Id = collectionId,
                TenantId = contract.TenantId,
                ShopId = contract.ShopId,
                ContractId = contract.Id,
                PaidAtUtc = paidAt,
                CreatedByUserId = userId,
                AmountReceived = dto.Payment.Amount,
                Method = method,
                ReferenceNo = dto.Payment.Reference,
                MoneyEntryId = null,            // 💡 şimdilik yok, onayda eklenecek
                TargetYear = null,
                TargetMonth = null,
                Notes = dto.Payment.Description,
                IsApproved = false,
                ApprovedByUserId = null,
                ApprovedAtUtc = null
            };

            _collectionDal.Add(collection);

            // 6) Allocation + Settlement + Accrual güncelleme
            foreach (var line in dto.Allocations)
            {
                var accrual = accruals.First(a => a.Id == line.DebtId);

                if (line.Amount <= 0)
                    throw new Exception("Dağıtım satırında tutar sıfır veya negatif olamaz.");

                if (line.Amount > accrual.OutstandingAmount)
                    throw new Exception("Dağıtım tutarı kalan borçtan büyük olamaz.");

                // CollectionAllocation kaydı
                var allocation = new CollectionAllocation
                {
                    Id = Guid.NewGuid(),
                    CollectionId = collectionId,
                    Kind = (AllocationKind)accrual.Kind,
                    Amount = line.Amount,
                    ContractId = accrual.ContractId,
                    ShopId = accrual.ShopId,
                    TenantId = accrual.TenantId,
                    MeterReadingId = accrual.MeterReadingId,
                    PeriodYear = accrual.Year,
                    PeriodMonth = accrual.Month,
                    Notes = dto.Payment.Description,
                    CreatedByUserId = userId,
                    CreatedAtUtc = nowUtc
                };
                _collectionAllocationDal.Add(allocation);

                // AccrualSettlement kaydı
                var settlement = new AccrualSettlement
                {
                    Id = Guid.NewGuid(),
                    CollectionAllocationId = allocation.Id,
                    AccrualId = accrual.Id,
                    Amount = line.Amount,
                    SettledByUserId = userId,
                    SettledAtUtc = paidAt,
                    Notes = dto.Payment.Description
                };
                _accrualSettlementDal.Add(settlement);

                // Accrual borcunu düş
                accrual.ApplySettlement(line.Amount);   // OutstandingAmount & Status güncelleniyor
                accrual.UpdatedByUserId = userId;
                accrual.UpdatedAtUtc = nowUtc;

                _accrualDal.Update(accrual);
            }

            return new SuccessResult("پرداخت و توزیع بدهی ثبت شد (در انتظار تایید).");
        }


        private PaymentMethod MapPaymentMethod(string method)
        {
            return method?.ToLowerInvariant() switch
            {
                "cash" => PaymentMethod.Cash,
                "bank" => PaymentMethod.BankTransfer,
                "pos" => PaymentMethod.Pos,
                "cheque" => PaymentMethod.Cheque,
                _ => PaymentMethod.Unknown
            };
        }


        // Diğer ICollectionService metotları (GetById, GetByTenant, Add vs.) burada olacak;
        // şu an odak sadece Approve üzerinde.

        [ValidationAspect(typeof(CollectionApproveValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ICollectionService.Get"), CacheRemoveAspect("IDashboardService.GetFinanceDashboard")]
        [SecuredOperation("admin,collection.approve")]

        public IResult Approve(CollectionApproveDto dto, int userId)
        {
            var collection = _collectionDal.Query().FirstOrDefault(x => x.Id == dto.Id);
            if (collection == null)
                return new ErrorResult("پرداخت یافت نشد.");

            if (collection.IsApproved)
                return new ErrorResult("این پرداخت قبلاً تایید شده است.");

            if (collection.AmountReceived <= 0)
                return new ErrorResult("مبلغ پرداخت باید بزرگتر از صفر باشد.");

            var nowUtc = _timeProvider.UtcNow();

            // 1) Allocation toplamı ile tahsilat uyumu
            var allocations = _collectionAllocationDal.QueryNoTracking()
                .Where(a => a.CollectionId == collection.Id)
                .ToList();

            if (!allocations.Any())
                return new ErrorResult("برای این پرداخت هیچ توزیعی (Allocation) ثبت نشده است.");

            var totalAllocated = allocations.Sum(a => a.Amount);
            var diff = Math.Abs(totalAllocated - collection.AmountReceived);

            if (diff > 0.01m)
                return new ErrorResult("مجموع توزیع‌ها با مبلغ پرداخت شده برابر نیست.");

            // 2) MoneyAccount: her zaman CashLocal + AFN
            var moneyAccount = _moneyAccountDal.QueryNoTracking()
                .FirstOrDefault(a =>
                    a.AccountType == MoneyAccountType.CashLocal &&
                    a.Currency == Currency.Afn &&
                    a.IsActive);

            if (moneyAccount == null)
                return new ErrorResult("حساب نقدی افغانی (CashLocal / AFN) تعریف نشده است.");

            // 3) Dolar kuru – tahsilat tarihi için
            var paidAt = collection.PaidAtUtc;
            var rate = _dollarRateDal.QueryNoTracking()
                .OrderByDescending(r => r.EffectiveFromUtc)
                .ThenByDescending(r => r.Id)
                .FirstOrDefault();

            if (rate == null)
                return new ErrorResult("برای این پرداخت هیچ نرخ دالر ثبت نشده است.");

            if (rate.AfnPerUsd <= 0)
                return new ErrorResult("نرخ دالر نامعتبر است.");

            var amountAfn = collection.AmountReceived;
            var usdEquivalent = Math.Round(amountAfn / rate.AfnPerUsd, 4);

            var moneyEntryId = Guid.NewGuid();

            // allocations zaten elinde
            var p = allocations.FirstOrDefault(a => a.PeriodYear.HasValue && a.PeriodMonth.HasValue);
            var period = p != null ? $"{p.PeriodYear:0000}-{p.PeriodMonth:00}" : "---- --";

            // Shop -> Floor -> Market (shopId ile)
            var shopId = _contractDal.QueryNoTracking()
                .Where(c => c.Id == collection.ContractId)
                .Select(c => c.ShopId)
                .FirstOrDefault();

            var shopInfo = (from s in _shopDal.QueryNoTracking().Where(x => x.Id == shopId)
                            join f in _floorDal.QueryNoTracking() on s.FloorId equals f.Id
                            join m in _marketDal.QueryNoTracking() on f.MarketId equals m.Id
                            select new { ShopName = s.Name, MarketName = m.MarketName, FloorName = f.FloorName })
                           .FirstOrDefault();

            if (shopInfo == null)
                return new ErrorResult("دوکان/مارکیت یافت نشد.");

            var baseNote = $"{period} | دوکان {shopInfo.ShopName}، {shopInfo.MarketName}، {shopInfo.FloorName}";


            var moneyEntry = new MoneyEntry
            {
                Id = moneyEntryId,
                MoneyAccountId = moneyAccount.Id,
                Direction = BalanceDirection.In,
                Amount = amountAfn,
                Currency = Currency.Afn,
                AfnPerUsdAtThatTime = rate.AfnPerUsd,
                UsdEquivalent = usdEquivalent,
                OccurredAtUtc = paidAt,
                CreatedAtUtc = nowUtc,
                CreatedByUserId = userId,
                CollectionId = collection.Id,
                ExpenseId = null
            };

            _moneyEntryDal.Add(moneyEntry);

            // 5) Allocation türlerine göre dağıtım

            // 5.1 Electricity
            ProcessElectricityAllocations(collection, allocations, userId, nowUtc);

            // 5.2 Market
            ProcessMarketAllocations(collection, allocations, userId, nowUtc);

            // 5.3 Franchise (imtiyaz sahibi payı)
            ProcessFranchiseAllocations(collection, allocations, userId, nowUtc, baseNote);

            // 5.4 Owner (market sahibi payı)
            ProcessOwnerAllocation(collection, allocations, userId, nowUtc, baseNote);

            // 6) Collection üzerinde onay bilgileri
            collection.IsApproved = true;
            collection.ApprovedByUserId = userId;
            collection.ApprovedAtUtc = nowUtc;
            collection.UpdatedByUserId = userId;
            collection.UpdatedAtUtc = nowUtc;
            collection.MoneyEntryId = moneyEntryId;

            _collectionDal.Update(collection);

            return new SuccessResult("پرداخت تایید شد، در حساب نقدی افغانی ثبت گردید و به حساب‌های مربوطه توزیع شد.");
        }

        private void ProcessElectricityAllocations(Collection collection, List<CollectionAllocation> allocations, int userId, DateTime nowUtc)
        {
            var electricityAllocations = allocations
                .Where(a => a.Kind == AllocationKind.Electricity)
                .ToList();

            if (!electricityAllocations.Any())
                return;

            var policy = _singleRatePolicyDal.QueryNoTracking()
                .FirstOrDefault(p => p.Type == SingleRateType.ElectricityReserve && p.IsActive);

            if (policy == null)
                throw new Exception("برای برق هیچ سیاست ذخیره (SingleRatePolicy.ElectricityReserve) تعریف نشده است.");

            var reserveAccount = _reserveAccountDal.QueryNoTracking()
                .FirstOrDefault(r => r.Type == ReserveAccountType.ElectricityReserve && r.IsActive);

            var mainAccount = _reserveAccountDal.QueryNoTracking()
                .FirstOrDefault(r => r.Type == ReserveAccountType.ElectricityMain && r.IsActive);

            if (reserveAccount == null || mainAccount == null)
                throw new Exception("حساب‌های ذخیره یا اصلی برق (ElectricityReserve / ElectricityMain) تعریف نشده است.");

            foreach (var a in electricityAllocations)
            {
                var reserveAmount = Math.Round(a.Amount * policy.RatePercent / 100m, 2);
                if (reserveAmount < 0) reserveAmount = 0;
                var mainAmount = a.Amount - reserveAmount;

                if (reserveAmount > 0)
                {
                    var re = new ReserveEntry
                    {
                        Id = Guid.NewGuid(),
                        ReserveAccountId = reserveAccount.Id,
                        Direction = BalanceDirection.In,
                        Amount = reserveAmount,
                        CollectionId = collection.Id,
                        CollectionAllocationId = a.Id,
                        ContractId = a.ContractId ?? collection.ContractId,
                        //ShopId = a.ShopId ?? collection.ShopId,
                        //PeriodYear = a.PeriodYear,
                        //PeriodMonth = a.PeriodMonth,
                        OccurredAtUtc = collection.PaidAtUtc,
                        CreatedAtUtc = nowUtc,
                        CreatedByUserId = userId,
                        //Notes = "Electricity reserve from collection"
                        PaymentMethod = collection.Method,
                        Description = collection.Notes,
                        PaymentReference = collection.ReferenceNo,
                        
                    };
                    _reserveEntryDal.Add(re);
                }

                if (mainAmount > 0)
                {
                    var reMain = new ReserveEntry
                    {
                        Id = Guid.NewGuid(),
                        ReserveAccountId = mainAccount.Id,
                        Direction = BalanceDirection.In,
                        Amount = mainAmount,
                        CollectionId = collection.Id,
                        CollectionAllocationId = a.Id,
                        ContractId = a.ContractId ?? collection.ContractId,
                        //ShopId = a.ShopId ?? collection.ShopId,
                        //PeriodYear = a.PeriodYear,
                        //PeriodMonth = a.PeriodMonth,
                        OccurredAtUtc = collection.PaidAtUtc,
                        CreatedAtUtc = nowUtc,
                        CreatedByUserId = userId,
                        //Notes = "Electricity main share from collection"

                        PaymentMethod = collection.Method,
                        Description = collection.Notes,
                        PaymentReference = collection.ReferenceNo,

                    };
                    _reserveEntryDal.Add(reMain);
                }
            }
        }

        private void ProcessMarketAllocations(Collection collection, List<CollectionAllocation> allocations, int userId, DateTime nowUtc)
        {
            var marketAllocations = allocations
                .Where(a => a.Kind == AllocationKind.Market)
                .ToList();

            if (!marketAllocations.Any())
                return;

            var policy = _singleRatePolicyDal.QueryNoTracking()
                .FirstOrDefault(p => p.Type == SingleRateType.MarketReserve && p.IsActive);

            if (policy == null)
                throw new Exception("برای مارکیت هیچ سیاست ذخیره (SingleRatePolicy.MarketReserve) تعریف نشده است.");

            var reserveAccount = _reserveAccountDal.QueryNoTracking()
                .FirstOrDefault(r => r.Type == ReserveAccountType.MarketReserve && r.IsActive);

            var mainAccount = _reserveAccountDal.QueryNoTracking()
                .FirstOrDefault(r => r.Type == ReserveAccountType.MarketMain && r.IsActive);

            if (reserveAccount == null || mainAccount == null)
                throw new Exception("حساب‌های ذخیره یا اصلی مارکیت (MarketReserve / MarketMain) تعریف نشده است.");

            foreach (var a in marketAllocations)
            {
                var reserveAmount = Math.Round(a.Amount * policy.RatePercent / 100m, 2);
                if (reserveAmount < 0) reserveAmount = 0;
                var mainAmount = a.Amount - reserveAmount;

                if (reserveAmount > 0)
                {
                    var re = new ReserveEntry
                    {
                        Id = Guid.NewGuid(),
                        ReserveAccountId = reserveAccount.Id,
                        Direction = BalanceDirection.In,
                        Amount = reserveAmount,
                        CollectionId = collection.Id,
                        CollectionAllocationId = a.Id,
                        ContractId = a.ContractId ?? collection.ContractId,
                        //ShopId = a.ShopId ?? collection.ShopId,
                        //PeriodYear = a.PeriodYear,
                        //PeriodMonth = a.PeriodMonth,
                        OccurredAtUtc = collection.PaidAtUtc,
                        CreatedAtUtc = nowUtc,
                        CreatedByUserId = userId,
                        //Notes = "Market reserve from collection"

                        PaymentMethod = collection.Method,
                        Description = collection.Notes,
                        PaymentReference = collection.ReferenceNo,

                        
                    };
                    _reserveEntryDal.Add(re);
                }

                if (mainAmount > 0)
                {
                    var reMain = new ReserveEntry
                    {
                        Id = Guid.NewGuid(),
                        ReserveAccountId = mainAccount.Id,
                        Direction = BalanceDirection.In,
                        Amount = mainAmount,
                        CollectionId = collection.Id,
                        CollectionAllocationId = a.Id,
                        ContractId = a.ContractId ?? collection.ContractId,
                        //ShopId = a.ShopId ?? collection.ShopId,
                        //PeriodYear = a.PeriodYear,
                        //PeriodMonth = a.PeriodMonth,
                        OccurredAtUtc = collection.PaidAtUtc,
                        CreatedAtUtc = nowUtc,
                        CreatedByUserId = userId,
                        //Notes = "Market main share from collection"

                        PaymentMethod = collection.Method,
                        Description = collection.Notes,
                        PaymentReference = collection.ReferenceNo,
                    };
                    _reserveEntryDal.Add(reMain);
                }
            }
        }

        private void ProcessFranchiseAllocations(Collection collection, List<CollectionAllocation> allocations, int userId, DateTime nowUtc, string baseNote)
        {
            var franchiseAllocations = allocations
                .Where(a => a.Kind == AllocationKind.Franchise)
                .ToList();

            if (!franchiseAllocations.Any())
                return;

            var contract = _contractDal.QueryNoTracking()
                .FirstOrDefault(c => c.Id == collection.ContractId);

            if (contract == null)
                throw new Exception("قرارداد مرتبط با این پرداخت یافت نشد.");

            var shopId = contract.ShopId;

            foreach (var a in franchiseAllocations)
            {
                var paidDate = DateOnly.FromDateTime(collection.PaidAtUtc);

                var shares = _shopFranchiseShareDal.QueryNoTracking()
                    .Where(s => s.ShopId == shopId &&
                                (s.EffectiveFrom == null || s.EffectiveFrom <= paidDate) &&
                                (s.EffectiveTo == null || s.EffectiveTo >= paidDate))
                    .ToList();


                if (!shares.Any())
                    continue;

                var totalPercent = shares.Sum(s => s.SharePercent);
                if (totalPercent > 100m + 0.0001m)
                    throw new Exception("مجموع درصد سهام‌داران امتیاز از 100% بیشتر است.");

                foreach (var s in shares)
                {
                    var ownerAmount = Math.Round(a.Amount * s.SharePercent / 100m, 2);
                    if (ownerAmount <= 0) continue;

                    var fb = new FranchiseBalanceEntry
                    {
                        Id = Guid.NewGuid(),
                        FranchiseOwnerId = s.FranchiseOwnerId,
                        Direction = BalanceDirection.In,
                        Amount = ownerAmount,
                        CreatedByUserId = userId,
                        CreatedAtUtc = nowUtc,
                        OccurredAtUtc = collection.PaidAtUtc,
                        CollectionId = collection.Id,
                        CollectionAllocationId = a.Id,
                        ContractId = collection.ContractId,
                        ShopId = shopId,
                        PeriodYear = a.PeriodYear,
                        PeriodMonth = a.PeriodMonth,
                        Notes = baseNote,
                        PaymentMethod = collection.Method,
                        PaymentReference = collection.ReferenceNo,
                        
                    };

                    _franchiseBalanceEntryDal.Add(fb);
                }
            }
        }
        private void ProcessOwnerAllocation(Collection collection, List<CollectionAllocation> allocations, int userId, DateTime nowUtc, string baseNote)
        {
            var ownerAllocations = allocations
                .Where(a => a.Kind == AllocationKind.Owner)
                .ToList();

            if (!ownerAllocations.Any())
                return;

            var ownerTotal = ownerAllocations.Sum(a => a.Amount);
            if (ownerTotal <= 0) return;

            var policy = _ownerDistributionPolicyDal.QueryNoTracking()
                .FirstOrDefault(p => p.IsActive);

            if (policy == null)
                throw new Exception("هیچ سیاست توزیع مالک (OwnerDistributionPolicy) فعال نیست.");

            var taxAmount = Math.Round(ownerTotal * policy.RateTax / 100m, 2);
            var zakatAmount = Math.Round(ownerTotal * policy.RateZakat / 100m, 2);
            var charityAmount = Math.Round(ownerTotal * policy.RateCharity / 100m, 2);
            var reserveAmount = Math.Round(ownerTotal * policy.RateOwnerReserve / 100m, 2);

            var cutsTotal = taxAmount + zakatAmount + charityAmount + reserveAmount;
            var familyBase = ownerTotal - cutsTotal; // %60 kısım – familyBase

            if (familyBase < 0) familyBase = 0;

            // OwnerDistribution kaydı
            var firstAlloc = ownerAllocations.First();

            var ownerDistId = Guid.NewGuid();

            // Family havuz oranı: ya policy'den gelir ya da 100 - diğerleri
            var rateFamilyPool = 100m
                                 - policy.RateTax
                                 - policy.RateZakat
                                 - policy.RateCharity
                                 - policy.RateOwnerReserve;

            if (rateFamilyPool < 0)
                rateFamilyPool = 0; // güvenlik

            var ownerDist = new OwnerDistribution
            {
                Id = ownerDistId,

                ContractId = collection.ContractId,
                CollectionId = collection.Id,
                CollectionAllocationId = firstAlloc.Id,

                // Girişte kullanılan toplam owner tutarı
                OwnerAmountInput = ownerTotal,

                // O an kullanılan oranların snapshot'ı
                RateTax = policy.RateTax,
                RateZakat = policy.RateZakat,
                RateCharity = policy.RateCharity,
                RateOwnerReserve = policy.RateOwnerReserve,
                RateFamilyPool = rateFamilyPool,

                // Hesaplanan tutarların snapshot'ı
                AmountTax = taxAmount,
                AmountZakat = zakatAmount,
                AmountCharity = charityAmount,
                AmountOwnerReserve = reserveAmount,
                AmountFamilyPool = familyBase,

                OccurredAtUtc = collection.PaidAtUtc,
                Notes = "Owner distribution from collection"
            };

            _ownerDistributionDal.Add(ownerDist);

            // ownerTotal, taxAmount, zakatAmount, charityAmount, reserveAmount, familyBase, policy, ownerDistId
            // hepsi yukarıda hesaplanmış olsun.

            decimal familyRate = 0m;
            if (ownerTotal > 0)
                familyRate = Math.Round(familyBase * 100m / ownerTotal, 2); // فیصدی حقی خانواده

            // مالیه (Tax)
            if (taxAmount > 0)
            {
                var line = new OwnerDistributionLine
                {
                    Id = Guid.NewGuid(),
                    OwnerDistributionId = ownerDistId,
                    Part = DistributionPart.Tax,
                    RateUsed = policy.RateTax,
                    Amount = taxAmount,
                    Notes = baseNote,
                    
                };
                _ownerDistributionLineDal.Add(line);
            }

            // زکات
            if (zakatAmount > 0)
            {
                var line = new OwnerDistributionLine
                {
                    Id = Guid.NewGuid(),
                    OwnerDistributionId = ownerDistId,
                    Part = DistributionPart.Zakat,
                    RateUsed = policy.RateZakat,
                    Amount = zakatAmount,
                    Notes = baseNote
                };
                _ownerDistributionLineDal.Add(line);
            }

            // صدقه / خیریه
            if (charityAmount > 0)
            {
                var line = new OwnerDistributionLine
                {
                    Id = Guid.NewGuid(),
                    OwnerDistributionId = ownerDistId,
                    Part = DistributionPart.Charity,
                    RateUsed = policy.RateCharity,
                    Amount = charityAmount,
                    Notes = baseNote
                };
                _ownerDistributionLineDal.Add(line);
            }

            // ذخیره مالک (OwnerReserve)
            if (reserveAmount > 0)
            {
                var line = new OwnerDistributionLine
                {
                    Id = Guid.NewGuid(),
                    OwnerDistributionId = ownerDistId,
                    Part = DistributionPart.OwnerReserve,
                    RateUsed = policy.RateOwnerReserve,
                    Amount = reserveAmount,
                    Notes = baseNote
                };
                _ownerDistributionLineDal.Add(line);
            }

            // حوض خانواده (FamilyPool)
            if (familyBase > 0)
            {
                var line = new OwnerDistributionLine
                {
                    Id = Guid.NewGuid(),
                    OwnerDistributionId = ownerDistId,
                    Part = DistributionPart.FamilyPool,
                    RateUsed = familyRate,
                    Amount = familyBase,
                    Notes =baseNote
                };
                _ownerDistributionLineDal.Add(line);
            }


            // Owner Reserve -> ReserveEntry(In)
            if (reserveAmount > 0)
            {
                var ownerReserveAccount = _reserveAccountDal.QueryNoTracking()
                    .FirstOrDefault(r => r.Type == ReserveAccountType.OwnerReserve && r.IsActive);

                if (ownerReserveAccount == null)
                    throw new Exception("حساب ذخیره مالک (OwnerReserve) تعریف نشده است.");

                var re = new ReserveEntry
                {
                    Id = Guid.NewGuid(),
                    ReserveAccountId = ownerReserveAccount.Id,
                    Direction = BalanceDirection.In,
                    Amount = reserveAmount,
                    CollectionId = collection.Id,
                    CollectionAllocationId = firstAlloc.Id,
                    ContractId = collection.ContractId,                    
                    OccurredAtUtc = collection.PaidAtUtc,
                    CreatedAtUtc = nowUtc,
                    CreatedByUserId = userId,

                    PaymentMethod = collection.Method,
                    Description = collection.Notes,
                    PaymentReference = collection.ReferenceNo,

                };
                _reserveEntryDal.Add(re);
            }

            // Tax/Zakat/Charity – IN kayıtları (borç/ayrılan hak)
            if (taxAmount > 0)
            {
                var t = new TaxPaymentEntry
                {
                    Id = Guid.NewGuid(),
                    Direction = BalanceDirection.In,
                    Amount = taxAmount,
                    CollectionId = collection.Id,
                    OwnerDistributionId = ownerDistId,
                    MoneyEntryId = null,
                    CreatedByUserId = userId,
                    CreatedAtUtc = nowUtc,
                    Notes = baseNote,
                    PaymentMethod = collection.Method,
                    PaymentReference = collection.ReferenceNo,

                    ApprovedByUserId = userId,
                    ApprovedAtUtc = nowUtc,
                    ContractId = collection.ContractId,
                    OccurredAtUtc = nowUtc,
                };
                _taxPaymentEntryDal.Add(t);
            }

            if (zakatAmount > 0)
            {
                var z = new ZakatPaymentEntry
                {
                    Id = Guid.NewGuid(),
                    Direction = BalanceDirection.In,
                    Amount = zakatAmount,
                    CollectionId = collection.Id,
                    OwnerDistributionId = ownerDistId,
                    MoneyEntryId = null,
                    CreatedByUserId = userId,
                    CreatedAtUtc = nowUtc,
                    Notes = baseNote,
                    PaymentMethod = collection.Method,
                    PaymentReference = collection.ReferenceNo,
                    ApprovedByUserId = userId,
                    ApprovedAtUtc = nowUtc,
                    ContractId = collection.ContractId,
                    OccurredAtUtc = nowUtc,
                    
                };
                _zakatPaymentEntryDal.Add(z);
            }

            if (charityAmount > 0)
            {
                var c = new CharityPaymentEntry
                {
                    Id = Guid.NewGuid(),
                    Direction = BalanceDirection.In,
                    Amount = charityAmount,
                    CollectionId = collection.Id,
                    OwnerDistributionId = ownerDistId,
                    MoneyEntryId = null,
                    CreatedByUserId = userId,
                    CreatedAtUtc = nowUtc,
                    Notes = baseNote,
                    ApprovedByUserId = userId,
                    ApprovedAtUtc = nowUtc,
                    ContractId = collection.ContractId,
                    OccurredAtUtc = nowUtc,
                    PaymentMethod = collection.Method,
                    PaymentReference = collection.ReferenceNo,

                };
                _charityPaymentEntryDal.Add(c);
            }

            // Aile dağıtımı – familyBase üzerinden
            if (familyBase <= 0)
                return;

            var familyMembers = _familyMemberDal.QueryNoTracking()
                .Where(f => f.IsActive && f.IncludeInShare)
                .ToList();

            if (!familyMembers.Any())
                return;

            var parents = familyMembers.Where(f => f.IsParent).ToList();
            var children = familyMembers.Where(f => !f.IsParent).ToList();

            var parentCount = parents.Count;
            var parentShareEach = Math.Round(familyBase * 8m / 100m, 2); // %8 familyBase’ten
            var parentTotal = parentShareEach * parentCount;

            var childrenPool = familyBase - parentTotal;
            if (childrenPool < 0) childrenPool = 0;

            var femaleChildren = children.Where(c => c.Gender == Gender.Female).ToList();
            var maleChildren = children.Where(c => c.Gender == Gender.Male).ToList();

            var totalShares = femaleChildren.Count * 1 + maleChildren.Count * 2;
            decimal perShare = 0;
            if (totalShares > 0)
                perShare = Math.Round(childrenPool / totalShares, 2);

            var payoutId = Guid.NewGuid();

            var payout = new FamilyPayout
            {
                Id = payoutId,
                OwnerDistributionId = ownerDistId,
                TotalFamilyAmount = familyBase,
                RuleSummary = "والدین: هر کدام ۸٪ از مبلغ حوض خانواده؛ اطفال: دختر=۱ سهم، پسر=۲ سهم.",
                OccurredAtUtc = collection.PaidAtUtc
            };

            _familyPayoutDal.Add(payout);



            // Ebeveynler
            foreach (var p in parents)
            {
                var amount = parentShareEach;
                if (amount <= 0) continue;

                var lineId = Guid.NewGuid();
                var line = new FamilyPayoutLine
                {
                    Id = lineId,
                    FamilyPayoutId = payoutId,
                    FamilyMemberId = p.Id,
                    Amount = amount
                };
                _familyPayoutLineDal.Add(line);

                var fb = new FamilyBalanceEntry
                {
                    Id = Guid.NewGuid(),
                    FamilyMemberId = p.Id,
                    Direction = BalanceDirection.In,
                    Amount = amount,
                    CollectionId = collection.Id,
                    OwnerDistributionId = ownerDistId,
                    FamilyPayoutId = payoutId,
                    PeriodYear = firstAlloc.PeriodYear,
                    PeriodMonth = firstAlloc.PeriodMonth,
                    OccurredAtUtc = collection.PaidAtUtc,
                    CreatedAtUtc = nowUtc,
                    CreatedByUserId = userId,
                    Notes = baseNote,
                    ContractId = collection.ContractId,
                    PaymentMethod = collection.Method,
                    PaymentReference = collection.ReferenceNo,
                };
                _familyBalanceEntryDal.Add(fb);
            }

            // Çocuklar
            foreach (var child in femaleChildren)
            {
                var amount = perShare;
                if (amount <= 0) continue;

                var lineId = Guid.NewGuid();
                var line = new FamilyPayoutLine
                {
                    Id = lineId,
                    FamilyPayoutId = payoutId,
                    FamilyMemberId = child.Id,
                    Amount = amount
                };
                _familyPayoutLineDal.Add(line);

                var fb = new FamilyBalanceEntry
                {
                    Id = Guid.NewGuid(),
                    FamilyMemberId = child.Id,
                    Direction = BalanceDirection.In,
                    Amount = amount,
                    CollectionId = collection.Id,
                    OwnerDistributionId = ownerDistId,
                    FamilyPayoutId = payoutId,
                    //PeriodYear = firstAlloc.PeriodYear,
                    //PeriodMonth = firstAlloc.PeriodMonth,
                    //OccurredAtUtc = collection.PaidAtUtc,
                    //CreatedAtUtc = nowUtc,
                    //CreatedByUserId = userId,
                    Notes = baseNote
                };
                _familyBalanceEntryDal.Add(fb);
            }

            foreach (var child in maleChildren)
            {
                var amount = perShare * 2;
                if (amount <= 0) continue;

                var lineId = Guid.NewGuid();
                var line = new FamilyPayoutLine
                {
                    Id = lineId,
                    FamilyPayoutId = payoutId,
                    FamilyMemberId = child.Id,
                    Amount = amount
                };
                _familyPayoutLineDal.Add(line);

                var fb = new FamilyBalanceEntry
                {
                    Id = Guid.NewGuid(),
                    FamilyMemberId = child.Id,
                    Direction = BalanceDirection.In,
                    Amount = amount,
                    CollectionId = collection.Id,
                    OwnerDistributionId = ownerDistId,
                    FamilyPayoutId = payoutId,
                    //PeriodYear = firstAlloc.PeriodYear,
                    //PeriodMonth = firstAlloc.PeriodMonth,
                    //OccurredAtUtc = collection.PaidAtUtc,
                    //CreatedAtUtc = nowUtc,
                    //CreatedByUserId = userId,
                    Notes = baseNote,
                };
                _familyBalanceEntryDal.Add(fb);
            }
        }
    }
}
