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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Business.ValidationRules.FluentValidation.Accrual;
using Entities.Dtos.Accrual;
using System.Globalization;

namespace Business.Concrete
{
    public class AccrualManager : IAccrualService
    {
        private readonly IAccrualDal _dal;
        private readonly IAccrualSettlementDal _settlementDal; // varsayılan
        public AccrualManager(IAccrualDal dal, IAccrualSettlementDal settlementDal)
        {
            _dal = dal;
            _settlementDal = settlementDal;
        }

        [CacheAspect(10)]
        public IDataResult<List<Accrual>> GetAll(Guid? shopId = null, Guid? tenantId = null, int? year = null, int? month = null, AccrualStatus? status = null)
        {
            var q = _dal.QueryNoTracking();

            if (shopId.HasValue) q = q.Where(x => x.ShopId == shopId.Value);
            if (tenantId.HasValue) q = q.Where(x => x.TenantId == tenantId.Value);
            if (year.HasValue) q = q.Where(x => x.Year == year.Value);
            if (month.HasValue) q = q.Where(x => x.Month == month.Value);
            if (status.HasValue) q = q.Where(x => x.Status == status.Value);

            return new SuccessDataResult<List<Accrual>>(q.OrderByDescending(x => x.CreatedAtUtc).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<Accrual> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking()
                        .Include(x => x.AccrualSettlements)
                        .FirstOrDefault(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<Accrual>(null, "یافت نشد.")
                : new SuccessDataResult<Accrual>(e);
        }

        [CacheAspect(10)]
        public IDataResult<AccrualDetailDto> GetDetail(Guid id)
        {
            var e = _dal.QueryNoTracking()
                        .Include(x => x.AccrualSettlements)
                        .FirstOrDefault(x => x.Id == id);
            if (e == null) return new ErrorDataResult<AccrualDetailDto>(null, "یافت نشد.");

            var settled = e.AccrualSettlements?.Sum(s => s.Amount) ?? 0m;
            return new SuccessDataResult<AccrualDetailDto>(new AccrualDetailDto
            {
                Id = e.Id,
                ShopId = e.ShopId,
                TenantId = e.TenantId,
                ContractId = e.ContractId,
                MeterReadingId = e.MeterReadingId,
                Kind = e.Kind,
                Year = e.Year,
                Month = e.Month,
                CreatedAtUtc = e.CreatedAtUtc,
                CreatedByUserId = e.CreatedByUserId,
                UpdatedByUserId = e.UpdatedByUserId,
                UpdatedAtUtc = e.UpdatedAtUtc,
                Status = e.Status,
                Amount = e.Amount,
                OutstandingAmount = e.OutstandingAmount,
                Notes = e.Notes,
                SettlementCount = e.AccrualSettlements?.Count ?? 0,
                SettledTotal = Math.Round(settled, 2, MidpointRounding.AwayFromZero)
            });
        }

        [ValidationAspect(typeof(AccrualCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IAccrualService.Get")]
        public IResult Add(AccrualCreateDto dto, int userId)
        {
            var e = new Accrual
            {
                Id = Guid.NewGuid(),
                ShopId = dto.ShopId,
                TenantId = dto.TenantId,
                ContractId = dto.ContractId,
                MeterReadingId = dto.MeterReadingId,
                Kind = dto.Kind,
                Year = dto.Year,
                Month = dto.Month,
                Amount = Math.Round(dto.Amount, 2, MidpointRounding.AwayFromZero),
                OutstandingAmount = Math.Round(dto.Amount, 2, MidpointRounding.AwayFromZero),
                Notes = dto.Notes,
                Status = AccrualStatus.Open,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            _dal.Add(e);
            return new SuccessResult("ثبت شد.");
        }

        [ValidationAspect(typeof(AccrualUpdateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IAccrualService.Get")]
        public IResult Update(AccrualUpdateDto dto, int userId)
        {
            var e = _dal.Get(x => x.Id == dto.Id);
            if (e == null) return new ErrorResult("یافت نشد.");

            e.ShopId = dto.ShopId;
            e.TenantId = dto.TenantId;
            e.ContractId = dto.ContractId;
            e.MeterReadingId = dto.MeterReadingId;
            e.Kind = dto.Kind;
            e.Year = dto.Year;
            e.Month = dto.Month;
            e.Amount = Math.Round(dto.Amount, 2, MidpointRounding.AwayFromZero);
            e.OutstandingAmount = Math.Round(dto.OutstandingAmount, 2, MidpointRounding.AwayFromZero); // dikkat: iş kuralı gereği sınırlanabilir
            e.Notes = dto.Notes;
            e.Status = dto.Status;
            e.UpdatedByUserId = userId;
            e.UpdatedAtUtc = DateTime.UtcNow;

            // Tutarlılık: Outstanding > Amount ise düzelt
            if (e.OutstandingAmount > e.Amount) e.OutstandingAmount = e.Amount;

            _dal.Update(e);
            return new SuccessResult("بروز شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IAccrualService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            try
            {
                _dal.Delete(e);
                return new SuccessResult("حذف شد.");
            }
            catch (DbUpdateException)
            {
                return new ErrorResult("به علت وابستگی داده‌ها حذف ممکن نیست.");
            }
        }

        [ValidationAspect(typeof(AccrualSettleValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IAccrualService.Get")]
        public IResult Settle(AccrualSettleDto dto, int userId)
        {
            var e = _dal.Get(x => x.Id == dto.AccrualId);
            if (e == null) return new ErrorResult("یافت نشد.");
            if (e.Status == AccrualStatus.Settled) return new ErrorResult("این بدهی قبلاً تسویه شده است.");

            // Kayıt altı: settlement entity varsayıyoruz
            var settlement = new AccrualSettlement
            {
                Id = Guid.NewGuid(),
                AccrualId = e.Id,
                Amount = Math.Round(dto.Amount, 2, MidpointRounding.AwayFromZero),
                Notes = dto.Note,
                SettledAtUtc = DateTime.UtcNow,
                SettledByUserId = userId
            };
            _settlementDal.Add(settlement);

            // Domain metodunu kullan
            e.ApplySettlement(settlement.Amount);
            e.UpdatedByUserId = userId;
            e.UpdatedAtUtc = DateTime.UtcNow;

            _dal.Update(e);

            // Rakamlar İngilizce: InvariantCulture ile biçimlendir
            var amountStr = settlement.Amount.ToString(CultureInfo.InvariantCulture);
            return new SuccessResult($"ثبت تسویه به مبلغ {amountStr} انجام شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IAccrualService.Get")]
        public IResult Reopen(Guid id, int userId)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            e.Status = AccrualStatus.Open;
            e.OutstandingAmount = e.Amount;
            e.UpdatedByUserId = userId;
            e.UpdatedAtUtc = DateTime.UtcNow;

            _dal.Update(e);
            return new SuccessResult("بدهی مجدداً باز شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IAccrualService.Get")]
        public IResult RecalculateOutstanding(Guid id)
        {
            var e = _dal.Query().Include(x => x.AccrualSettlements).FirstOrDefault(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            var settled = e.AccrualSettlements?.Sum(s => s.Amount) ?? 0m;
            e.OutstandingAmount = Math.Round(Math.Max(0m, e.Amount - settled), 2, MidpointRounding.AwayFromZero);
            e.Status = e.OutstandingAmount == 0 ? AccrualStatus.Settled : (settled == 0 ? AccrualStatus.Open : AccrualStatus.PartiallySettled);

            _dal.Update(e);
            return new SuccessResult("محاسبه‌ی بدهی باقی‌مانده بروز شد.");
        }
    }
}
