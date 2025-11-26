using Business.Abstract;
using Business.ValidationRules.FluentValidation.ZakatPayment;
using Core.Utilities.Aspects.Autofac.Validation;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos.ZakatPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class ZakatPaymentEntryManager : IZakatPaymentEntryService
    {
        private readonly IZakatPaymentEntryDal _dal;

        public ZakatPaymentEntryManager(IZakatPaymentEntryDal dal)
        {
            _dal = dal;
        }

        public IDataResult<List<ZakatPaymentEntry>> GetAll()
        {
            return new SuccessDataResult<List<ZakatPaymentEntry>>(
                _dal.QueryNoTracking().OrderByDescending(x => x.OccurredAtUtc).ToList());
        }

        public IDataResult<List<ZakatPaymentEntry>> GetAll(ZakatPaymentEntryFilterDto filter)
        {
            var q = _dal.QueryNoTracking();

            if (filter.FromOccurredAtUtc.HasValue)
                q = q.Where(x => x.OccurredAtUtc >= filter.FromOccurredAtUtc);

            if (filter.ToOccurredAtUtc.HasValue)
                q = q.Where(x => x.OccurredAtUtc <= filter.ToOccurredAtUtc);

            if (filter.Direction.HasValue)
                q = q.Where(x => x.Direction == filter.Direction);

            if (filter.ApprovedOnly.HasValue)
                q = filter.ApprovedOnly.Value
                    ? q.Where(x => x.ApprovedAtUtc != null)
                    : q.Where(x => x.ApprovedAtUtc == null);

            return new SuccessDataResult<List<ZakatPaymentEntry>>(q.ToList());
        }

        [ValidationAspect(typeof(ZakatPaymentEntryCreateValidator))]
        public IResult Add(ZakatPaymentEntryCreateDto dto, int userId)
        {
            var entity = new ZakatPaymentEntry
            {
                Id = Guid.NewGuid(),
                Direction = dto.Direction,
                Amount = Math.Round(dto.Amount, 2),
                OccurredAtUtc = dto.OccurredAtUtc,
                Notes = dto.Notes,
                PaymentMethod = dto.PaymentMethod,
                PaymentReference = dto.PaymentReference,
                OwnerDistributionId = dto.OwnerDistributionId,
                CollectionId = dto.CollectionId,
                ContractId = dto.ContractId,
                CreatedByUserId = userId
            };

            _dal.Add(entity);
            return new SuccessResult($"پرداخت زکات با مبلغ {entity.Amount} ثبت شد.");
        }

        [ValidationAspect(typeof(ZakatPaymentEntryUpdateValidator))]
        public IResult Update(ZakatPaymentEntryUpdateDto dto, int userId)
        {
            var e = _dal.Get(x => x.Id == dto.Id);
            if (e == null) return new ErrorResult("یافت نشد.");

            e.Amount = dto.Amount;
            e.Notes = dto.Notes;
            e.PaymentMethod = dto.PaymentMethod;
            e.PaymentReference = dto.PaymentReference;
            e.UpdatedByUserId = userId;
            e.UpdatedAtUtc = DateTime.UtcNow;

            _dal.Update(e);
            return new SuccessResult("بروزرسانی شد.");
        }

        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        public IResult Approve(Guid id, int userId)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            e.ApprovedByUserId = userId;
            e.ApprovedAtUtc = DateTime.UtcNow;
            _dal.Update(e);

            return new SuccessResult("پرداخت تایید شد.");
        }

        public IDataResult<ZakatPaymentEntryDetailDto> GetDetail(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorDataResult<ZakatPaymentEntryDetailDto>(null, "یافت نشد.");

            return new SuccessDataResult<ZakatPaymentEntryDetailDto>(new ZakatPaymentEntryDetailDto
            {
                Id = e.Id,
                Amount = e.Amount,
                Notes = e.Notes,
                Direction = e.Direction,
                PaymentMethod = e.PaymentMethod,
                PaymentReference = e.PaymentReference,
                OccurredAtUtc = e.OccurredAtUtc
            });
        }
    }

}
