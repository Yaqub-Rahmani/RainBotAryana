using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation.DollarRate;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Utilities.Aspects.Autofac.Validation;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos.DollarRate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class DollarRateManager : IDollarRateService
    {
        private readonly IDollarRateDal _dal;
        private readonly ITimeProvider _timeProvider;

        public DollarRateManager(IDollarRateDal dal, ITimeProvider timeProvider)
        {
            _dal = dal;
            _timeProvider = timeProvider;
        }

        [CacheAspect(10)]
        public IDataResult<List<DollarRateDto>> GetAll()
        {
            var list = _dal.QueryNoTracking()
                           .OrderByDescending(x => x.EffectiveFromUtc)
                           .Select(x => new DollarRateDto
                           {
                               Id = x.Id,
                               EffectiveFromUtc = x.EffectiveFromUtc,
                               AfnPerUsd = x.AfnPerUsd,
                               CreatedAtUtc = x.CreatedAtUtc,
                               CreatedByUserId = x.CreatedByUserId
                           })
                           .ToList();

            return new SuccessDataResult<List<DollarRateDto>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<DollarRateDto> GetById(int id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (e == null)
                return new ErrorDataResult<DollarRateDto>(null,"نرخ یافت نشد.");

            var dto = new DollarRateDto
            {
                Id = e.Id,
                EffectiveFromUtc = e.EffectiveFromUtc,
                AfnPerUsd = e.AfnPerUsd,
                CreatedAtUtc = e.CreatedAtUtc,
                CreatedByUserId = e.CreatedByUserId
            };

            return new SuccessDataResult<DollarRateDto>(dto);
        }

        [CacheAspect(10)]
        public IDataResult<DollarRateDto> GetLatest()
        {
            var e = _dal.QueryNoTracking()
                        .OrderByDescending(x => x.EffectiveFromUtc)
                        .ThenByDescending(x => x.Id)
                        .FirstOrDefault();

            if (e == null)
                return new ErrorDataResult<DollarRateDto>(null,"هنوز هیچ نرخ ثبت نشده است.");

            var dto = new DollarRateDto
            {
                Id = e.Id,
                EffectiveFromUtc = e.EffectiveFromUtc,
                AfnPerUsd = e.AfnPerUsd,
                CreatedAtUtc = e.CreatedAtUtc,
                CreatedByUserId = e.CreatedByUserId
            };

            return new SuccessDataResult<DollarRateDto>(dto);
        }

        [ValidationAspect(typeof(DollarRateCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IDollarRateService.Get"), CacheRemoveAspect("IDashboardService.GetFinanceDashboard")]
        [SecuredOperation("admin")] // rol ismini senin sistemine göre ayarlarsın
        public IResult Add(DollarRateCreateDto dto, int userId)
        {
            var nowUtc = _timeProvider.UtcNow();

            var entity = new DollarRate
            {
                EffectiveFromUtc = dto.EffectiveFromUtc,
                AfnPerUsd = dto.AfnPerUsd,
                CreatedByUserId = userId,
                CreatedAtUtc = nowUtc
            };

            _dal.Add(entity);
            return new SuccessResult("نرخ دالر ثبت شد.");
        }

        [ValidationAspect(typeof(DollarRateUpdateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IDollarRateService.Get"), CacheRemoveAspect("IDashboardService.GetFinanceDashboard")]
        [SecuredOperation("admin")]
        public IResult Update(DollarRateUpdateDto dto, int userId)
        {
            var existing = _dal.Query().FirstOrDefault(x => x.Id == dto.Id);
            if (existing == null)
                return new ErrorResult("نرخ یافت نشد.");

            existing.EffectiveFromUtc = dto.EffectiveFromUtc;
            existing.AfnPerUsd = dto.AfnPerUsd;
            // CreatedAtUtc ve CreatedByUserId değişmesin

            _dal.Update(existing);
            return new SuccessResult("نرخ دالر ویرایش شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IDollarRateService.Get"), CacheRemoveAspect("IDashboardService.GetFinanceDashboard")]
        [SecuredOperation("admin")]
        public IResult Delete(int id)
        {
            var existing = _dal.Query().FirstOrDefault(x => x.Id == id);
            if (existing == null)
                return new ErrorResult("نرخ یافت نشد.");

            _dal.Delete(existing);
            return new SuccessResult("نرخ دالر حذف شد.");
        }
    }
}
