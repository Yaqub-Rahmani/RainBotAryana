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
using Business.ValidationRules.FluentValidation.FamilySharePolicy;
using Core.Utilities.KabulDateTime;
using Entities.Dtos.FamilySharePolicy;

namespace Business.Concrete
{
    public class FamilySharePolicyManager : IFamilySharePolicyService
    {
        private readonly IFamilySharePolicyDal _dal;
        private readonly ITimeProvider _timeProvider;

        public FamilySharePolicyManager(
            IFamilySharePolicyDal dal,
            ITimeProvider timeProvider)
        {
            _dal = dal;
            _timeProvider = timeProvider;
        }

        [CacheAspect(10)]
        public IDataResult<List<FamilySharePolicyDto>> GetAll()
        {
            var list = _dal.QueryNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.UpdatedAtUtc)
                .Select(MapToDto)
                .ToList();

            return new SuccessDataResult<List<FamilySharePolicyDto>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<FamilySharePolicyDto> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (e == null)
                return new ErrorDataResult<FamilySharePolicyDto>(null, "پالیسی خانواده یافت نشد.");

            return new SuccessDataResult<FamilySharePolicyDto>(MapToDto(e));
        }

        [CacheAspect(10)]
        public IDataResult<FamilySharePolicyDto> GetCurrent(DateTime? asOfUtc = null)
        {
            var asOfDate = DateOnly.FromDateTime(asOfUtc ?? _timeProvider.UtcNow());

            var e = _dal.QueryNoTracking()
                .Where(x => x.CreatedAtUtc <= asOfDate)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.UpdatedAtUtc)
                .FirstOrDefault();

            if (e == null)
                return new ErrorDataResult<FamilySharePolicyDto>(null, "هیچ پالیسی خانواده تعریف نشده است.");

            return new SuccessDataResult<FamilySharePolicyDto>(MapToDto(e));
        }

        [ValidationAspect(typeof(FamilySharePolicyCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IFamilySharePolicyService.Get")]
        [SecuredOperation("admin")]
        public IResult Add(FamilySharePolicyCreateDto dto, int userId)
        {
            var createdDate = dto.CreatedAtUtc ?? DateOnly.FromDateTime(_timeProvider.UtcNow());

            var entity = new FamilySharePolicy
            {
                Id = Guid.NewGuid(),
                ParentPercent = dto.ParentPercent,
                FemaleWeight = dto.FemaleWeight,
                MaleWeight = dto.MaleWeight,
                Notes = dto.Notes,
                CreatedAtUtc = createdDate,
                CreatedByUserId = userId
            };

            _dal.Add(entity);
            return new SuccessResult("پالیسی تقسیم خانواده ثبت شد.");
        }

        [ValidationAspect(typeof(FamilySharePolicyUpdateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IFamilySharePolicyService.Get")]
        [SecuredOperation("admin")]
        public IResult Update(FamilySharePolicyUpdateDto dto, int userId)
        {
            var existing = _dal.Query().FirstOrDefault(x => x.Id == dto.Id);
            if (existing == null)
                return new ErrorResult("پالیسی خانواده یافت نشد.");

            existing.ParentPercent = dto.ParentPercent;
            existing.FemaleWeight = dto.FemaleWeight;
            existing.MaleWeight = dto.MaleWeight;
            existing.Notes = dto.Notes;
            existing.UpdatedByUserId = userId;
            existing.UpdatedAtUtc = _timeProvider.UtcNow();

            _dal.Update(existing);
            return new SuccessResult("پالیسی تقسیم خانواده ویرایش شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IFamilySharePolicyService.Get")]
        [SecuredOperation("admin")]
        public IResult Delete(Guid id)
        {
            var existing = _dal.Query().FirstOrDefault(x => x.Id == id);
            if (existing == null)
                return new ErrorResult("پالیسی خانواده یافت نشد.");

            _dal.Delete(existing);
            return new SuccessResult("پالیسی خانواده حذف شد.");
        }

        private static FamilySharePolicyDto MapToDto(FamilySharePolicy x)
        {
            return new FamilySharePolicyDto
            {
                Id = x.Id,
                ParentPercent = x.ParentPercent,
                FemaleWeight = x.FemaleWeight,
                MaleWeight = x.MaleWeight,
                Notes = x.Notes,
                CreatedAtUtc = x.CreatedAtUtc,
                CreatedByUserId = x.CreatedByUserId,
                UpdatedByUserId = x.UpdatedByUserId,
                UpdatedAtUtc = x.UpdatedAtUtc
            };
        }
    }
}
