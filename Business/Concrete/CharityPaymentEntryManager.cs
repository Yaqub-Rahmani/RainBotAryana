using Business.Abstract;
using Business.ValidationRules.FluentValidation.CharityPayment;
using Core.Utilities.Aspects.Autofac.Validation;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos.CharityPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CharityPaymentEntryManager : ICharityPaymentEntryService
    {
        private readonly ICharityPaymentEntryDal _dal;

        public CharityPaymentEntryManager(ICharityPaymentEntryDal dal)
        {
            _dal = dal;
        }

        public IDataResult<List<CharityPaymentEntry>> GetAll()
        {
            var list = _dal.QueryNoTracking()
                           .OrderByDescending(x => x.OccurredAtUtc)
                           .ToList();

            return new SuccessDataResult<List<CharityPaymentEntry>>(list);
        }

        public IDataResult<List<CharityPaymentEntry>> GetAll(CharityPaymentEntryFilterDto filter)
        {
            filter ??= new CharityPaymentEntryFilterDto();
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

            if (filter.MinAmount.HasValue)
                q = q.Where(x => x.Amount >= filter.MinAmount);

            if (filter.MaxAmount.HasValue)
                q = q.Where(x => x.Amount <= filter.MaxAmount);

            if (filter.OwnerDistributionId.HasValue)
                q = q.Where(x => x.OwnerDistributionId == filter.OwnerDistributionId);

            if (filter.CollectionId.HasValue)
                q = q.Where(x => x.CollectionId == filter.CollectionId);

            if (filter.ContractId.HasValue)
                q = q.Where(x => x.ContractId == filter.ContractId);

            return new SuccessDataResult<List<CharityPaymentEntry>>(q.ToList());
        }

        public IDataResult<CharityPaymentEntryDetailDto> GetDetail(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null)
                return new ErrorDataResult<CharityPaymentEntryDetailDto>(null,"یافت نشد.");

            return new SuccessDataResult<CharityPaymentEntryDetailDto>(new CharityPaymentEntryDetailDto
            {
                Id = e.Id,
                Direction = e.Direction,
                Amount = e.Amount,
                OccurredAtUtc = e.OccurredAtUtc,
                Notes = e.Notes,
                PaymentMethod = e.PaymentMethod,
                PaymentReference = e.PaymentReference,
                CreatedByUserId = e.CreatedByUserId,
                CreatedAtUtc = e.CreatedAtUtc,
                UpdatedByUserId = e.UpdatedByUserId,
                UpdatedAtUtc = e.UpdatedAtUtc,
                ApprovedByUserId = e.ApprovedByUserId,
                ApprovedAtUtc = e.ApprovedAtUtc
            });
        }

        [ValidationAspect(typeof(CharityPaymentEntryCreateValidator))]
        public IResult Add(CharityPaymentEntryCreateDto dto, int userId)
        {
            var entry = new CharityPaymentEntry
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

            _dal.Add(entry);
            return new SuccessResult($"پرداخت صدقه با مبلغ {entry.Amount} ثبت شد.");
        }

        [ValidationAspect(typeof(CharityPaymentEntryUpdateValidator))]
        public IResult Update(CharityPaymentEntryUpdateDto dto, int userId)
        {
            var e = _dal.Get(x => x.Id == dto.Id);
            if (e == null)
                return new ErrorResult("یافت نشد.");

            e.Amount = dto.Amount;
            e.Notes = dto.Notes;
            e.PaymentMethod = dto.PaymentMethod;
            e.PaymentReference = dto.PaymentReference;
            e.UpdatedByUserId = userId;
            e.UpdatedAtUtc = DateTime.UtcNow;

            _dal.Update(e);
            return new SuccessResult("بروزرسانی انجام شد.");
        }

        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null)
                return new ErrorResult("یافت نشد.");

            _dal.Delete(e);
            return new SuccessResult("حذف انجام شد.");
        }

        public IResult Approve(Guid id, int userId)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null)
                return new ErrorResult("یافت نشد.");

            e.ApprovedByUserId = userId;
            e.ApprovedAtUtc = DateTime.UtcNow;
            _dal.Update(e);

            return new SuccessResult("پرداخت تایید شد.");
        }
    }
}
