using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Business.ValidationRules.FluentValidation.AccrualSettlement;
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
using Entities.Dtos.AccrualSettlement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class AccrualSettlementManager : IAccrualSettlementService
    {
        private readonly IAccrualSettlementDal _dal;
        private readonly IAccrualDal _accrualDal;
        private readonly ICollectionAllocationDal _allocDal; // varsayıyorum

        public AccrualSettlementManager(IAccrualSettlementDal dal, IAccrualDal accrualDal, ICollectionAllocationDal allocDal)
        {
            _dal = dal;
            _accrualDal = accrualDal;
            _allocDal = allocDal;
        }

        [CacheAspect(10)]
        public IDataResult<List<AccrualSettlement>> GetAll(Guid? accrualId = null)
        {
            var q = _dal.QueryNoTracking();
            if (accrualId.HasValue) q = q.Where(x => x.AccrualId == accrualId.Value);
            return new SuccessDataResult<List<AccrualSettlement>>(q.OrderByDescending(x => x.SettledAtUtc).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<AccrualSettlement> GetById(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<AccrualSettlement>(null, "یافت نشد.")
                : new SuccessDataResult<AccrualSettlement>(e);
        }

        [CacheAspect(10)]
        public IDataResult<AccrualSettlementDetailDto> GetDetail(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (e == null) return new ErrorDataResult<AccrualSettlementDetailDto>(null, "یافت نشد.");

            var a = _accrualDal.QueryNoTracking().FirstOrDefault(x => x.Id == e.AccrualId);
            if (a == null) return new ErrorDataResult<AccrualSettlementDetailDto>(null, "یافت نشد.");

            return new SuccessDataResult<AccrualSettlementDetailDto>(new AccrualSettlementDetailDto
            {
                Id = e.Id,
                CollectionAllocationId = e.CollectionAllocationId,
                AccrualId = e.AccrualId,
                Amount = e.Amount,
                SettledByUserId = e.SettledByUserId,
                SettledAtUtc = e.SettledAtUtc,
                Notes = e.Notes,
                AccrualAmount = a.Amount,
                AccrualOutstanding = a.OutstandingAmount,
                AccrualYear = a.Year,
                AccrualMonth = a.Month
            });
        }

        [ValidationAspect(typeof(AccrualSettlementCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IAccrualSettlementService.Get")]
        public IResult Add(AccrualSettlementCreateDto dto, int userId)
        {
            var accrual = _accrualDal.Get(x => x.Id == dto.AccrualId);
            if (accrual == null) return new ErrorResult("بدهی یافت نشد.");

            var alloc = _allocDal.Get(x => x.Id == dto.CollectionAllocationId);
            if (alloc == null) return new ErrorResult("تخصیص یافت نشد.");

            // İş kuralı: tutar, kalan borcu aşamaz
            if (dto.Amount <= 0) return new ErrorResult("مبلغ باید بزرگتر از 0 باشد.");
            if (dto.Amount > accrual.OutstandingAmount)
            {
                var maxStr = accrual.OutstandingAmount.ToString(CultureInfo.InvariantCulture);
                return new ErrorResult($"مبلغ نمی‌تواند از بدهی باقی‌مانده ({maxStr}) بیشتر باشد.");
            }

            // Domain update
            accrual.ApplySettlement(Math.Round(dto.Amount, 2, MidpointRounding.AwayFromZero));
            accrual.UpdatedByUserId = userId;
            accrual.UpdatedAtUtc = DateTime.UtcNow;
            _accrualDal.Update(accrual);

            var entity = new AccrualSettlement
            {
                Id = Guid.NewGuid(),
                CollectionAllocationId = dto.CollectionAllocationId,
                AccrualId = dto.AccrualId,
                Amount = Math.Round(dto.Amount, 2, MidpointRounding.AwayFromZero),
                SettledByUserId = userId,
                SettledAtUtc = DateTime.UtcNow,
                Notes = dto.Notes
            };
            _dal.Add(entity);

            var amtStr = entity.Amount.ToString(CultureInfo.InvariantCulture);
            return new SuccessResult($"تسویه با مبلغ {amtStr} ثبت شد.");
        }

        [ValidationAspect(typeof(AccrualSettlementUpdateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IAccrualSettlementService.Get")]
        public IResult Update(AccrualSettlementUpdateDto dto, int userId)
        {
            var e = _dal.Get(x => x.Id == dto.Id);
            if (e == null) return new ErrorResult("یافت نشد.");

            // İş kuralı: varsayılan olarak sadece Notlar güncellenir
            e.Notes = dto.Notes;
            _dal.Update(e);
            return new SuccessResult("بروز شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IAccrualSettlementService.Get")]
        public IResult Delete(Guid id, int userId)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            var accrual = _accrualDal.Get(x => x.Id == e.AccrualId);
            if (accrual == null) return new ErrorResult("بدهی یافت نشد.");

            // Reversal: tahakkuk tutarlarını geri sar
            accrual.OutstandingAmount = Math.Round(accrual.OutstandingAmount + e.Amount, 2, MidpointRounding.AwayFromZero);
            accrual.Status = accrual.OutstandingAmount == 0 ? AccrualStatus.Settled
                               : (accrual.OutstandingAmount == accrual.Amount ? AccrualStatus.Open : AccrualStatus.PartiallySettled);
            accrual.UpdatedByUserId = userId;
            accrual.UpdatedAtUtc = DateTime.UtcNow;
            _accrualDal.Update(accrual);

            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }

}
