using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Validation;
using Core.Extensions;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class AccrualSettlementManager : IAccrualSettlementService
    {
        private readonly IAccrualSettlementDal _settlementDal;
        private readonly IAccrualDal _accrualDal;
        private readonly ICollectionAllocationDal _allocationDal; // varsayım: mevcut

        public AccrualSettlementManager(
            IAccrualSettlementDal settlementDal,
            IAccrualDal accrualDal,
            ICollectionAllocationDal allocationDal)
        {
            _settlementDal = settlementDal;
            _accrualDal = accrualDal;
            _allocationDal = allocationDal;
        }

        // ---------- Okuma ----------

        [CacheAspect(10)]
        public IDataResult<List<AccrualSettlement>> GetAll()
            => new SuccessDataResult<List<AccrualSettlement>>(
                _settlementDal.QueryNoTracking().OrderByDescending(x => x.SettledAtUtc).ToList()
            );

        [CacheAspect(10)]
        public IDataResult<AccrualSettlement> GetById(Guid id)
        {
            var e = _settlementDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<AccrualSettlement>(null, "یافت نشد.")
                : new SuccessDataResult<AccrualSettlement>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<AccrualSettlementDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();

            var q = _settlementDal.QueryNoTracking();

            // Filtreler
            q = q.ApplyFilters(opt.Filters);

            // Arama
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => EF.Functions.Like(x.Notes ?? "", $"%{key}%"));
            }

            // Sıralama
            var hasSort = opt.Sort != null && opt.Sort.Any();
            if (hasSort)
            {
                q = q.ApplySort(opt.Sort);
            }
            else
            {
                q = q.OrderByDescending(x => x.SettledAtUtc);
            }

            // Toplam kayıt
            var total = q.Count();

            // Projection + Paging
            var items = q
                .Select(s => new AccrualSettlementDetailDto
                {
                    Id = s.Id,
                    CollectionAllocationId = s.CollectionAllocationId,
                    AccrualId = s.AccrualId,
                    Amount = s.Amount,
                    SettledAtUtc = s.SettledAtUtc,
                    Notes = s.Notes,

                    // ReferenceNo kaldırıldı; okunabilir bir isim türetiyoruz
                    Allocation = new RefDto
                    {
                        Id = s.CollectionAllocationId,
                        Name =
                            s.CollectionAllocation != null
                                ? (
                                    s.CollectionAllocation.PeriodYear.HasValue && s.CollectionAllocation.PeriodMonth.HasValue
                                        ? $"{s.CollectionAllocation.Kind} {s.CollectionAllocation.PeriodYear}-{(s.CollectionAllocation.PeriodMonth ?? 0).ToString("00")}"
                                        : s.CollectionAllocation.Kind.ToString()
                                  )
                                : "",
                        Code = null
                    },

                    Accrual = new RefDto
                    {
                        Id = s.AccrualId,
                        Name =
                            s.Accrual != null
                                ? $"{s.Accrual.Kind} {s.Accrual.Year}-{s.Accrual.Month.ToString("00")}"
                                : "",
                        Code = null
                    }
                })
                .ApplyPaging(opt.Page, opt.PageSize)
                .ToList();

            var page = new PagedResult<AccrualSettlementDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            };

            return new SuccessDataResult<PagedResult<AccrualSettlementDetailDto>>(page);
        }

        [CacheAspect(10)]
        public IDataResult<AccrualSettlementDetailDto> GetDetailById(Guid id)
        {
            var dto = _settlementDal.QueryNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new AccrualSettlementDetailDto
                {
                    Id = s.Id,
                    CollectionAllocationId = s.CollectionAllocationId,
                    AccrualId = s.AccrualId,
                    Amount = s.Amount,
                    SettledAtUtc = s.SettledAtUtc,
                    Notes = s.Notes,

                    Allocation = new RefDto
                    {
                        Id = s.CollectionAllocationId,
                        Name =
                            s.CollectionAllocation != null
                                ? (
                                    s.CollectionAllocation.PeriodYear.HasValue && s.CollectionAllocation.PeriodMonth.HasValue
                                        ? $"{s.CollectionAllocation.Kind} {s.CollectionAllocation.PeriodYear}-{(s.CollectionAllocation.PeriodMonth ?? 0).ToString("00")}"
                                        : s.CollectionAllocation.Kind.ToString()
                                  )
                                : "",
                        Code = null
                    },

                    Accrual = new RefDto
                    {
                        Id = s.AccrualId,
                        Name =
                            s.Accrual != null
                                ? $"{s.Accrual.Kind} {s.Accrual.Year}-{s.Accrual.Month.ToString("00")}"
                                : "",
                        Code = null
                    }
                })
                .FirstOrDefault();

            return dto == null
                ? new ErrorDataResult<AccrualSettlementDetailDto>(null, "یافت نشد.")
                : new SuccessDataResult<AccrualSettlementDetailDto>(dto);
        }

        // ---------- Yazma ----------

        /// <summary>
        /// Settlement ekler ve aynı transaction içinde Accrual.OutstandingAmount'ı düşürür.
        /// Ayrıca Accrual.Status'ü günceller. 
        /// CollectionAllocation için "kalan" kontrolü varsa (örn. RemainingAmount) ona göre doğrulama yap.
        /// </summary>
        [SecuredOperation("accrual.settle,admin")]
        [ValidationAspect(typeof(AccrualSettlementValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IAccrualSettlementService.Get")]
        [CacheRemoveAspect("IAccrualService.Get")]
        public IResult Add(AccrualSettlement settlement)
        {
            if (settlement.Amount <= 0) return new ErrorResult("مبلغ باید بزرگتر از صفر باشد.");

            var accrual = _accrualDal.Get(x => x.Id == settlement.AccrualId);
            if (accrual == null) return new ErrorResult("بدهی یافت نشد.");
            if (accrual.OutstandingAmount <= 0) return new ErrorResult("این بدهی قبلاً تسویه شده است.");
            if (settlement.Amount > accrual.OutstandingAmount)
                return new ErrorResult("مبلغ تسویه از بدهی باقیمانده بیشتر است.");

            // (opsiyonel) Allocation kalan kontrolü
            var allocation = _allocationDal.Get(x => x.Id == settlement.CollectionAllocationId);
            if (allocation == null) return new ErrorResult("تخصیص گردآوری یافت نشد.");
            // Örn: allocation.RemainingAmount varsa:
            // if (allocation.RemainingAmount < settlement.Amount) return new ErrorResult("موجودی تخصیص کافی نیست.");

            // 1) Accrual’ı düşür
            accrual.ApplySettlement(settlement.Amount);
            _accrualDal.Update(accrual);

            // 2) Allocation’dan düş (varsa)
            // allocation.RemainingAmount -= settlement.Amount;
            // _allocationDal.Update(allocation);

            // 3) Settlement’ı kaydet
            _settlementDal.Add(settlement);

            return new SuccessResult("تسویه ثبت شد.");
        }

        /// <summary>
        /// Settlement siler ve accrual bakiyesini geri artırır (undo).
        /// </summary>
        [SecuredOperation("accrual.settle,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IAccrualSettlementService.Get")]
        [CacheRemoveAspect("IAccrualService.Get")]
        public IResult Delete(Guid id)
        {
            var entity = _settlementDal.Get(x => x.Id == id);
            if (entity == null) return new ErrorResult("یافت نشد.");

            var accrual = _accrualDal.Get(x => x.Id == entity.AccrualId);
            if (accrual == null) return new ErrorResult("بدهی یافت نشد.");

            // 1) Accrual bakiyesini geri artır
            accrual.OutstandingAmount = Math.Round(accrual.OutstandingAmount + entity.Amount, 2, MidpointRounding.AwayFromZero);
            accrual.Status = accrual.OutstandingAmount == 0 ? AccrualStatus.Settled :
                             (accrual.OutstandingAmount == accrual.Amount ? AccrualStatus.Open : AccrualStatus.PartiallySettled);
            _accrualDal.Update(accrual);

            // 2) Allocation geri yükleme (varsa)
            // allocation.RemainingAmount += entity.Amount; _allocationDal.Update(allocation);

            // 3) Settlement sil
            _settlementDal.Delete(entity);

            return new SuccessResult("حذف تسویه انجام شد.");
        }

        [SecuredOperation("accrual.settle,admin")]
        [CacheRemoveAspect("IAccrualSettlementService.Get")]
        public IResult UpdateNote(Guid id, string? notes)
        {
            var entity = _settlementDal.Get(x => x.Id == id);
            if (entity == null) return new ErrorResult("یافت نشد.");
            entity.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            _settlementDal.Update(entity);
            return new SuccessResult("یادداشت بروزرسانی شد.");
        }
    }
}
