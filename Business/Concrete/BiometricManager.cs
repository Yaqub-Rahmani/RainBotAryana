using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class BiometricManager : IBiometricService
    {
        private readonly IBiometricDal _dal;
        public BiometricManager(IBiometricDal dal) { _dal = dal; }

        // ---------- Reads ----------
        [CacheAspect(10)]
        public IDataResult<List<Biometric>> GetAll()
            => new SuccessDataResult<List<Biometric>>(_dal.QueryNoTracking().OrderByDescending(x => x.CreatedAt).ToList());

        [CacheAspect(10)]
        public IDataResult<Biometric> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<Biometric>(null, "یافت نشد.")
                : new SuccessDataResult<Biometric>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<BiometricDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();
            var q = _dal.QueryNoTracking();

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => x.OwnerType.ToString().Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.CreatedAt);

            var projected = q.Select(p => new BiometricDetailDto
            {
                Id = p.Id,
                OwnerType = p.OwnerType.ToString(),
                OwnerId = p.OwnerId,
                Type = p.Type.ToString(),
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                RelatedMediaAssetId = p.RelatedMediaAssetId,
                TemplateSize = p.Template.Length
            });

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            var page = new PagedResult<BiometricDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            };

            return new SuccessDataResult<PagedResult<BiometricDetailDto>>(page);
        }

        [CacheAspect(10)]
        public IDataResult<BiometricDetailDto> GetDetailById(Guid id)
        {
            var dto = _dal.QueryNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new BiometricDetailDto
                {
                    Id = p.Id,
                    OwnerType = p.OwnerType.ToString(),
                    OwnerId = p.OwnerId,
                    Type = p.Type.ToString(),
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    RelatedMediaAssetId = p.RelatedMediaAssetId,
                    TemplateSize = p.Template.Length
                })
                .FirstOrDefault();

            return dto == null
                ? new ErrorDataResult<BiometricDetailDto>(null, "یافت نشد.")
                : new SuccessDataResult<BiometricDetailDto>(dto);
        }

        // ---------- Writes ----------
        [SecuredOperation("biometric.write,admin")]
        [ValidationAspect(typeof(BiometricValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IBiometricService.Get")]
        public IResult Add(Biometric biometric)
        {
            biometric.Id = Guid.NewGuid();
            biometric.CreatedAt = DateTime.UtcNow;
            _dal.Add(biometric);
            return new SuccessResult("ثبت بایومتریک انجام شد.");
        }

        [SecuredOperation("biometric.write,admin")]
        [ValidationAspect(typeof(BiometricValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IBiometricService.Get")]
        public IResult Update(Biometric biometric)
        {
            var current = _dal.Get(x => x.Id == biometric.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            current.OwnerType = biometric.OwnerType;
            current.OwnerId = biometric.OwnerId;
            current.Type = biometric.Type;
            current.Template = biometric.Template;
            current.RelatedMediaAssetId = biometric.RelatedMediaAssetId;
            current.IsActive = biometric.IsActive;

            _dal.Update(current);
            return new SuccessResult("بروزرسانی انجام شد.");
        }

        [SecuredOperation("biometric.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IBiometricService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
