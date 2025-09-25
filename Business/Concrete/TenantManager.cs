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
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Extensions;

namespace Business.Concrete
{
    public class TenantManager : ITenantService
    {
        private readonly ITenantDal _dal;

        public TenantManager(ITenantDal dal) { _dal = dal; }

        private static TenantDetailDto Project(Tenant t) => new()
        {
            Id = t.Id,
            DisplayName = t.DisplayName,
            Phone = t.Phone,
            Email = t.Email,
            TaxOrIdNumber = t.TaxOrIdNumber,
            IsActive = t.IsActive,
            ContractId = t.Contract?.Id,
            ContractStart = t.Contract?.StartDate,
            ContractEnd = t.Contract?.EndDate,
            ContractVersion = t.Contract?.Version,
            ContractStatus = t.Contract?.Status.ToString()
        };

        // ------- Reads -------
        [CacheAspect(10)]
        public IDataResult<List<Tenant>> GetAll(bool? onlyActive = null)
        {
            var q = _dal.QueryNoTracking();
            if (onlyActive == true) q = q.Where(x => x.IsActive);
            return new SuccessDataResult<List<Tenant>>(q.OrderBy(x => x.DisplayName).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<Tenant> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking()
                        .Include(x => x.Contract)
                        .FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<Tenant>(null, "یافت نشد.")
                             : new SuccessDataResult<Tenant>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<TenantDetailDto>> GetDetails(QueryOptions opt, bool? onlyActive = null)
        {
            opt ??= new QueryOptions();

            var q = _dal.QueryNoTracking();

            if (onlyActive == true)
                q = q.Where(x => x.IsActive);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                var like = $"%{key}%";

                q = q.Where(x =>
                    EF.Functions.Like(x.DisplayName, like) ||
                    (x.Phone != null && EF.Functions.Like(x.Phone, like)) ||
                    (x.Email != null && EF.Functions.Like(x.Email, like)) ||
                    (x.TaxOrIdNumber != null && EF.Functions.Like(x.TaxOrIdNumber, like)));
            }

            // DTO'ya projeksiyon (Include gerekmez)
            var qDto = q.Select(x => new TenantDetailDto
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                Phone = x.Phone,
                Email = x.Email,
                TaxOrIdNumber = x.TaxOrIdNumber,
                IsActive = x.IsActive,

                // Tekil navigation ise doğrudan erişim left join olarak çevrilir
                ContractId = x.Contract != null ? (Guid?)x.Contract.Id : null,
                ContractStart = x.Contract != null ? (DateOnly?)x.Contract.StartDate : null,
                ContractEnd = x.Contract != null ? (DateOnly?)x.Contract.EndDate : null,
                ContractVersion = x.Contract != null ? x.Contract.Version : null,
                ContractStatus = x.Contract != null ? x.Contract.Status.ToString() : null
            });

            // Sıralama – bir sort gelmediyse varsayılan uygula
            qDto = qDto.ApplySort(opt.Sort);
            if (!qDto.Expression.ToString()!.Contains("OrderBy"))
                qDto = qDto.OrderBy(x => x.DisplayName);

            var total = qDto.Count();
            var items = qDto
                .Skip((opt.Page - 1) * opt.PageSize)
                .Take(opt.PageSize)
                .ToList();

            return new SuccessDataResult<PagedResult<TenantDetailDto>>(new PagedResult<TenantDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            });
        }

        [CacheAspect(10)]
        public IDataResult<TenantDetailDto> GetDetailById(Guid id)
        {
            var e = _dal.QueryNoTracking().Include(x => x.Contract).FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<TenantDetailDto>(null, "یافت نشد.")
                             : new SuccessDataResult<TenantDetailDto>(Project(e));
        }

        // ------- Writes -------
        [SecuredOperation("tenant.write,admin")]
        [ValidationAspect(typeof(TenantValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ITenantService.Get")]
        public IResult Add(Tenant entity)
        {
            // Basit: isim uniq değilse de kabul (isterseniz uniq kuralı eklenir)
            _dal.Add(entity);
            return new SuccessResult("مستأجر ثبت شد.");
        }

        [SecuredOperation("tenant.write,admin")]
        [ValidationAspect(typeof(TenantValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ITenantService.Get")]
        public IResult Update(Tenant entity)
        {
            var e = _dal.Get(x => x.Id == entity.Id);
            if (e == null) return new ErrorResult("یافت نشد.");

            e.DisplayName = entity.DisplayName;
            e.Phone = entity.Phone;
            e.Email = entity.Email;
            e.TaxOrIdNumber = entity.TaxOrIdNumber;
            e.IsActive = entity.IsActive;

            _dal.Update(e);
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("tenant.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ITenantService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        // ------- Activate / Deactivate -------
        [SecuredOperation("tenant.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ITenantService.Get")]
        public IResult Activate(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.IsActive = true;
            _dal.Update(e);
            return new SuccessResult("فعال شد.");
        }

        [SecuredOperation("tenant.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("ITenantService.Get")]
        public IResult Deactivate(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.IsActive = false;
            _dal.Update(e);
            return new SuccessResult("غیرفعال شد.");
        }
    }
}
