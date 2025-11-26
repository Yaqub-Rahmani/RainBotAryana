using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Business.ValidationRules.FluentValidation.CollectionAllocation;
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
using Entities.Dtos.CollectionAllocation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CollectionAllocationManager : ICollectionAllocationService
    {
        private readonly ICollectionAllocationDal _dal;

        public CollectionAllocationManager(ICollectionAllocationDal dal)
        {
            _dal = dal;
        }

        [CacheAspect(10)]
        public IDataResult<List<CollectionAllocation>> GetAll(Guid? collectionId = null, Guid? contractId = null)
        {
            var q = _dal.QueryNoTracking();

            if (collectionId.HasValue)
                q = q.Where(x => x.CollectionId == collectionId.Value);

            if (contractId.HasValue)
                q = q.Where(x => x.ContractId == contractId.Value);

            return new SuccessDataResult<List<CollectionAllocation>>(q.ToList());
        }

        [CacheAspect(10)]
        public IDataResult<CollectionAllocation> GetById(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<CollectionAllocation>(null, "یافت نشد.")
                : new SuccessDataResult<CollectionAllocation>(e);
        }

        [CacheAspect(10)]
        public IDataResult<CollectionAllocationDetailDto> GetDetail(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorDataResult<CollectionAllocationDetailDto>(null, "یافت نشد.");

            var dto = new CollectionAllocationDetailDto
            {
                Id = e.Id,
                CollectionId = e.CollectionId,
                Kind = e.Kind,
                Amount = e.Amount,
                ContractId = e.ContractId,
                ShopId = e.ShopId,
                TenantId = e.TenantId,
                MeterReadingId = e.MeterReadingId,
                PeriodYear = e.PeriodYear,
                PeriodMonth = e.PeriodMonth,
                Notes = e.Notes,
                CreatedByUserId = e.CreatedByUserId,
                CreatedAtUtc = e.CreatedAtUtc
            };

            return new SuccessDataResult<CollectionAllocationDetailDto>(dto);
        }

        [ValidationAspect(typeof(CollectionAllocationCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("ICollectionAllocationService.Get")]
        public IResult Add(CollectionAllocationCreateDto dto, int userId)
        {
            var e = new CollectionAllocation
            {
                Id = Guid.NewGuid(),
                CollectionId = dto.CollectionId,
                Kind = dto.Kind,
                Amount = dto.Amount,
                ContractId = dto.ContractId,
                ShopId = dto.ShopId,
                TenantId = dto.TenantId,
                MeterReadingId = dto.MeterReadingId,
                PeriodYear = dto.PeriodYear,
                PeriodMonth = dto.PeriodMonth,
                Notes = dto.Notes,
                CreatedByUserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dal.Add(e);
            return new SuccessResult("ثبت شد.");
        }

        [ValidationAspect(typeof(CollectionAllocationUpdateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("ICollectionAllocationService.Get")]
        public IResult Update(CollectionAllocationUpdateDto dto, int userId)
        {
            var e = _dal.Get(x => x.Id == dto.Id);
            if (e == null) return new ErrorResult("یافت نشد.");

            e.Kind = dto.Kind;
            e.Amount = dto.Amount;
            e.ContractId = dto.ContractId;
            e.ShopId = dto.ShopId;
            e.TenantId = dto.TenantId;
            e.MeterReadingId = dto.MeterReadingId;
            e.PeriodYear = dto.PeriodYear;
            e.PeriodMonth = dto.PeriodMonth;
            e.Notes = dto.Notes;

            _dal.Update(e);

            return new SuccessResult("بروز شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("ICollectionAllocationService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            if (e.AccrualSettlements.Any())
                return new ErrorResult("به علت وجود تسویه، حذف ممکن نیست.");

            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
