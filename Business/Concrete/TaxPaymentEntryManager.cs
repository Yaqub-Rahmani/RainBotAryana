using Business.Abstract;
using Business.ValidationRules.FluentValidation.TaxPaymentEntry;
using Core.Utilities.Aspects.Autofac.Validation;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos.TaxPaymentEntry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class TaxPaymentEntryManager : ITaxPaymentEntryService
    {
        private readonly ITaxPaymentEntryDal _dal;

        public TaxPaymentEntryManager(ITaxPaymentEntryDal dal)
        {
            _dal = dal;
        }

        public IDataResult<List<TaxPaymentEntry>> GetAll()
        {
            var list = _dal.QueryNoTracking()
                .OrderByDescending(x => x.OccurredAtUtc)
                .ToList();

            return new SuccessDataResult<List<TaxPaymentEntry>>(list);
        }

        public IDataResult<List<TaxPaymentEntry>> GetAll(TaxPaymentEntryFilterDto filter)
        {
            filter ??= new TaxPaymentEntryFilterDto();
            var q = ApplyFilter(_dal.QueryNoTracking(), filter);
            var list = q.OrderByDescending(x => x.OccurredAtUtc).ToList();
            return new SuccessDataResult<List<TaxPaymentEntry>>(list);
        }

        public IDataResult<TaxPaymentEntry> GetById(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<TaxPaymentEntry>(null, "یافت نشد.")
                : new SuccessDataResult<TaxPaymentEntry>(e);
        }

        public IDataResult<TaxPaymentEntryDetailDto> GetDetail(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (e == null)
                return new ErrorDataResult<TaxPaymentEntryDetailDto>(null, "یافت نشد.");

            var dto = new TaxPaymentEntryDetailDto
            {
                Id = e.Id,
                Direction = e.Direction,
                Amount = e.Amount,
                OccurredAtUtc = e.OccurredAtUtc,
                Notes = e.Notes,
                CreatedByUserId = e.CreatedByUserId,
                CreatedAtUtc = e.CreatedAtUtc,
                UpdatedByUserId = e.UpdatedByUserId,
                UpdatedAtUtc = e.UpdatedAtUtc,
                ApprovedByUserId = e.ApprovedByUserId,
                ApprovedAtUtc = e.ApprovedAtUtc,
                PaymentMethod = e.PaymentMethod,
                PaymentReference = e.PaymentReference,
            };

            return new SuccessDataResult<TaxPaymentEntryDetailDto>(dto);
        }

        public IDataResult<TaxPaymentSummaryDto> GetSummary(TaxPaymentEntryFilterDto filter)
        {
            filter ??= new TaxPaymentEntryFilterDto();
            var q = ApplyFilter(_dal.QueryNoTracking(), filter);

            var totalIn = q.Where(x => x.Direction == BalanceDirection.In)
                           .Sum(x => (decimal?)x.Amount) ?? 0m;

            var totalOut = q.Where(x => x.Direction == BalanceDirection.Out)
                            .Sum(x => (decimal?)x.Amount) ?? 0m;

            var summary = new TaxPaymentSummaryDto
            {
                TotalIn = Math.Round(totalIn, 2, MidpointRounding.AwayFromZero),
                TotalOut = Math.Round(totalOut, 2, MidpointRounding.AwayFromZero)
            };

            return new SuccessDataResult<TaxPaymentSummaryDto>(summary);
        }

        [ValidationAspect(typeof(TaxPaymentEntryCreateValidator))]
        public IResult Add(TaxPaymentEntryCreateDto dto, int userId)
        {
            var entity = new TaxPaymentEntry
            {
                Id = Guid.NewGuid(),
                Direction = dto.Direction,
                Amount = Math.Round(dto.Amount, 2, MidpointRounding.AwayFromZero),
                OccurredAtUtc = dto.OccurredAtUtc,
                Notes = dto.Notes,
                PaymentMethod = dto.PaymentMethod,
                PaymentReference = dto.PaymentReference,
                OwnerDistributionId = dto.OwnerDistributionId,
                CollectionId = dto.CollectionId,
                ContractId = dto.ContractId,
                CreatedByUserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dal.Add(entity);

            var amountStr = entity.Amount.ToString(CultureInfo.InvariantCulture);
            return new SuccessResult($"پرداخت مالیات با مبلغ {amountStr} ثبت شد.");
        }

        [ValidationAspect(typeof(TaxPaymentEntryUpdateValidator))]
        public IResult Update(TaxPaymentEntryUpdateDto dto, int userId)
        {
            var e = _dal.Get(x => x.Id == dto.Id);
            if (e == null) return new ErrorResult("یافت نشد.");

            if (e.ApprovedAtUtc != null)
                return new ErrorResult("رکورد تایید شده قابل ویرایش نیست.");

            e.Direction = dto.Direction;
            e.Amount = Math.Round(dto.Amount, 2, MidpointRounding.AwayFromZero);
            e.OccurredAtUtc = dto.OccurredAtUtc;
            e.Notes = dto.Notes;
            e.PaymentMethod = dto.PaymentMethod;
            e.PaymentReference = dto.PaymentReference;
            e.OwnerDistributionId = dto.OwnerDistributionId;
            e.CollectionId = dto.CollectionId;
            e.ContractId = dto.ContractId;
            e.UpdatedByUserId = userId;
            e.UpdatedAtUtc = DateTime.UtcNow;

            _dal.Update(e);
            return new SuccessResult("بروز شد.");
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

            if (e.ApprovedAtUtc != null)
                return new ErrorResult("این رکورد قبلاً تایید شده است.");

            e.ApprovedByUserId = userId;
            e.ApprovedAtUtc = DateTime.UtcNow;

            _dal.Update(e);

            return new SuccessResult("پرداخت تایید شد.");
        }

        // Ortak filtre uygulanacak yer
        private IQueryable<TaxPaymentEntry> ApplyFilter(IQueryable<TaxPaymentEntry> q, TaxPaymentEntryFilterDto filter)
        {
            if (filter.FromOccurredAtUtc.HasValue)
                q = q.Where(x => x.OccurredAtUtc >= filter.FromOccurredAtUtc.Value);

            if (filter.ToOccurredAtUtc.HasValue)
                q = q.Where(x => x.OccurredAtUtc <= filter.ToOccurredAtUtc.Value);

            if (filter.FromCreatedAtUtc.HasValue)
                q = q.Where(x => x.CreatedAtUtc >= filter.FromCreatedAtUtc.Value);

            if (filter.ToCreatedAtUtc.HasValue)
                q = q.Where(x => x.CreatedAtUtc <= filter.ToCreatedAtUtc.Value);

            if (filter.Direction.HasValue)
                q = q.Where(x => x.Direction == filter.Direction.Value);

            if (filter.ApprovedOnly.HasValue)
            {
                if (filter.ApprovedOnly.Value)
                    q = q.Where(x => x.ApprovedAtUtc != null);
                else
                    q = q.Where(x => x.ApprovedAtUtc == null);
            }

            if (filter.MinAmount.HasValue)
                q = q.Where(x => x.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                q = q.Where(x => x.Amount <= filter.MaxAmount.Value);

            if (filter.OwnerDistributionId.HasValue)
                q = q.Where(x => x.OwnerDistributionId == filter.OwnerDistributionId.Value);

            if (filter.CollectionId.HasValue)
                q = q.Where(x => x.CollectionId == filter.CollectionId.Value);

            if (filter.ContractId.HasValue)
                q = q.Where(x => x.ContractId == filter.ContractId.Value);

            if (filter.CreatedByUserId.HasValue)
                q = q.Where(x => x.CreatedByUserId == filter.CreatedByUserId.Value);

            if (filter.ApprovedByUserId.HasValue)
                q = q.Where(x => x.ApprovedByUserId == filter.ApprovedByUserId.Value);

            return q;
        }
    }
}
