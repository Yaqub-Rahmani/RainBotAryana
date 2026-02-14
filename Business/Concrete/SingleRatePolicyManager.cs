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
using Business.ValidationRules.FluentValidation.SingleRatePolicy;
using Core.Utilities.KabulDateTime;
using Entities.Dtos.SingleRatePolicy;

namespace Business.Concrete
{
    public class SingleRatePolicyManager : ISingleRatePolicyService
    {
        private readonly ISingleRatePolicyDal _dal;
        private readonly ITimeProvider _timeProvider;

        public SingleRatePolicyManager(ISingleRatePolicyDal dal, ITimeProvider timeProvider)
        {
            _dal = dal;
            _timeProvider = timeProvider;
        }

        [CacheAspect(10)]
        public IDataResult<List<SingleRatePolicyDto>> GetAll()
        {
            var list = _dal.QueryNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.Type)
                .Select(MapToDto)
                .ToList();

            return new SuccessDataResult<List<SingleRatePolicyDto>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<List<SingleRatePolicyDto>> GetByType(SingleRateType type)
        {
            var list = _dal.QueryNoTracking()
                .Where(x => x.Type == type)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(MapToDto)
                .ToList();

            return new SuccessDataResult<List<SingleRatePolicyDto>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<SingleRatePolicyDto> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (e == null)
                return new ErrorDataResult<SingleRatePolicyDto>(null, "ثبت نرخ یافت نشد.");

            return new SuccessDataResult<SingleRatePolicyDto>(MapToDto(e));
        }

        [CacheAspect(10)]
        public IDataResult<decimal> GetCurrentRate(SingleRateType type, DateTime? asOfUtc = null)
        {
            var asOfDate = DateOnly.FromDateTime(asOfUtc ?? _timeProvider.UtcNow());

            var e = _dal.QueryNoTracking()
                .Where(x => x.Type == type && x.CreatedAtUtc <= asOfDate)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.UpdatedAtUtc)
                .FirstOrDefault();

            if (e == null)
                return new ErrorDataResult<decimal>(0m, "برای این نوع، نرخ تعریف نشده است.");


            return new SuccessDataResult<decimal>(e.RatePercent);
        }

        [ValidationAspect(typeof(SingleRatePolicyCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("ISingleRatePolicyService.Get")]
        [SecuredOperation("admin")]
        public IResult Add(SingleRatePolicyCreateDto dto, int userId)
        {
            var nowUtc = _timeProvider.UtcNow();
            var createdDate = dto.CreatedAtUtc ?? DateOnly.FromDateTime(nowUtc);

            // 1) Aynı tipte daha önce aktif olan politikaları pasife çek
            var activePolicies = _dal.GetList(x => x.Type == dto.Type && x.IsActive);

            foreach (var policy in activePolicies)
            {
                policy.IsActive = false;
                policy.UpdatedAtUtc = nowUtc;
                policy.UpdatedByUserId = userId;

                _dal.Update(policy);
            }

            // 2) Yeni politikayı oluştur ve aktif yap
            var entity = new SingleRatePolicy
            {
                Id = Guid.NewGuid(),
                Type = dto.Type,
                RatePercent = dto.RatePercent,
                CreatedAtUtc = createdDate,
                CreatedByUserId = userId,
                Notes = dto.Notes,
                IsActive = true
            };

            _dal.Add(entity);

            return new SuccessResult("پالیسی نرخ ثبت شد.");
        }


        [ValidationAspect(typeof(SingleRatePolicyUpdateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("ISingleRatePolicyService.Get")]
        [SecuredOperation("admin")]
        public IResult Update(SingleRatePolicyUpdateDto dto, int userId)
        {
            var existing = _dal.Query().FirstOrDefault(x => x.Id == dto.Id);
            if (existing == null)
                return new ErrorResult("ثبت نرخ یافت نشد.");

            existing.RatePercent = dto.RatePercent;
            existing.Notes = dto.Notes;
            existing.UpdatedByUserId = userId;
            existing.UpdatedAtUtc = _timeProvider.UtcNow();

            _dal.Update(existing);
            return new SuccessResult("پالیسی نرخ ویرایش شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("ISingleRatePolicyService.Get")]
        [SecuredOperation("admin")]
        public IResult Delete(Guid id)
        {
            var existing = _dal.Query().FirstOrDefault(x => x.Id == id);
            if (existing == null)
                return new ErrorResult("ثبت نرخ یافت نشد.");

            _dal.Delete(existing);
            return new SuccessResult("پالیسی نرخ حذف شد.");
        }

        private static SingleRatePolicyDto MapToDto(SingleRatePolicy x)
        {
            return new SingleRatePolicyDto
            {
                Id = x.Id,
                Type = x.Type,
                RatePercent = x.RatePercent,
                CreatedAtUtc = x.CreatedAtUtc,
                CreatedByUserId = x.CreatedByUserId,
                UpdatedByUserId = x.UpdatedByUserId,
                UpdatedAtUtc = x.UpdatedAtUtc,
                Notes = x.Notes,
                IsActive = x.IsActive
            };
        }
    }
}
