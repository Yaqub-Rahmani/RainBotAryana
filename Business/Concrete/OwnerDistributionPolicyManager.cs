using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
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
using Core.Aspects.Autofac.Validation;
using Core.Utilities.KabulDateTime;
using Entities.Dtos.OwnerDistributionPolicy;
using Business.ValidationRules.FluentValidation.OwnerDistributionPolicy;

namespace Business.Concrete
{
    public class OwnerDistributionPolicyManager : IOwnerDistributionPolicyService
    {
        private readonly IOwnerDistributionPolicyDal _dal;
        private readonly ITimeProvider _timeProvider;

        public OwnerDistributionPolicyManager(
            IOwnerDistributionPolicyDal dal,
            ITimeProvider timeProvider)
        {
            _dal = dal;
            _timeProvider = timeProvider;
        }

        [CacheAspect(10)]
        public IDataResult<List<OwnerDistributionPolicyDto>> GetAll()
        {
            var list = _dal.QueryNoTracking()

                .OrderByDescending(x => x.IsActive)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.UpdatedAtUtc)
                .Select(MapToDto)
                .ToList();

            return new SuccessDataResult<List<OwnerDistributionPolicyDto>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<OwnerDistributionPolicyDto> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (e == null)
                return new ErrorDataResult<OwnerDistributionPolicyDto>(null,"پالیسی یافت نشد.");

            return new SuccessDataResult<OwnerDistributionPolicyDto>(MapToDto(e));
        }

        [CacheAspect(10)]
        public IDataResult<OwnerDistributionPolicyDto> GetCurrent()
        {
            var e = _dal.QueryNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ThenByDescending(x => x.CreatedAtUtc)
                .FirstOrDefault();

            if (e == null)
                return new ErrorDataResult<OwnerDistributionPolicyDto>(
                    null,
                    "هیچ پالیسی owner فعال نیست."
                );

            return new SuccessDataResult<OwnerDistributionPolicyDto>(MapToDto(e));
        }


        [ValidationAspect(typeof(OwnerDistributionPolicyCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IOwnerDistributionPolicyService.Get")]
        [SecuredOperation("admin")]
        public IResult Add(OwnerDistributionPolicyCreateDto dto, int userId)
        {
            var nowUtc = _timeProvider.UtcNow();
            var createdDate = dto.CreatedAtUtc ?? DateOnly.FromDateTime(nowUtc);

            // 1) Daha önce aktif olan politikaları pasife çek
            var activePolicies = _dal.GetList(x => x.IsActive); // ya da GetAll / Where, repo ne destekliyorsa

            foreach (var policy in activePolicies)
            {
                policy.IsActive = false;
                policy.UpdatedAtUtc = nowUtc;
                policy.UpdatedByUserId = userId;

                _dal.Update(policy);
            }

            // 2) Yeni politikayı oluştur ve aktif yap
            var entity = new OwnerDistributionPolicy
            {
                Id = Guid.NewGuid(),
                RateTax = dto.RateTax,
                RateZakat = dto.RateZakat,
                RateCharity = dto.RateCharity,
                RateOwnerReserve = dto.RateOwnerReserve,
                RateFamilyPool = dto.RateFamilyPool,
                CreatedAtUtc = createdDate,
                CreatedByUserId = userId,
                Notes = dto.Notes,
                IsActive = true
            };

            // Toplam %100 mü, emin olmak için:
            entity.ValidateOrThrow();

            _dal.Add(entity);

            return new SuccessResult("پالیسی توزیع owner ثبت شد.");
        }


        [ValidationAspect(typeof(OwnerDistributionPolicyUpdateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IOwnerDistributionPolicyService.Get")]
        [SecuredOperation("admin")]
        public IResult Update(OwnerDistributionPolicyUpdateDto dto, int userId)
        {
            var existing = _dal.Query().FirstOrDefault(x => x.Id == dto.Id);
            if (existing == null)
                return new ErrorResult("پالیسی یافت نشد.");

            existing.RateTax = dto.RateTax;
            existing.RateZakat = dto.RateZakat;
            existing.RateCharity = dto.RateCharity;
            existing.RateOwnerReserve = dto.RateOwnerReserve;
            existing.RateFamilyPool = dto.RateFamilyPool;
            existing.Notes = dto.Notes;
            existing.UpdatedByUserId = userId;
            existing.UpdatedAtUtc = _timeProvider.UtcNow();

            existing.ValidateOrThrow();

            _dal.Update(existing);
            return new SuccessResult("پالیسی توزیع owner ویرایش شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IOwnerDistributionPolicyService.Get")]
        [SecuredOperation("admin")]
        public IResult Delete(Guid id)
        {
            var existing = _dal.Query().FirstOrDefault(x => x.Id == id);
            if (existing == null)
                return new ErrorResult("پالیسی یافت نشد.");

            _dal.Delete(existing);
            return new SuccessResult("پالیسی حذف شد.");
        }

        private static OwnerDistributionPolicyDto MapToDto(OwnerDistributionPolicy x)
        {
            return new OwnerDistributionPolicyDto
            {
                Id = x.Id,
                RateTax = x.RateTax,
                RateZakat = x.RateZakat,
                RateCharity = x.RateCharity,
                RateOwnerReserve = x.RateOwnerReserve,
                RateFamilyPool = x.RateFamilyPool,
                CreatedAtUtc = x.CreatedAtUtc,
                CreatedByUserId = x.CreatedByUserId,
                UpdatedByUserId = x.UpdatedByUserId,
                UpdatedAtUtc = x.UpdatedAtUtc,
                Notes = x.Notes,
                IsActive = x.IsActive,
            };
        }
    }
}
