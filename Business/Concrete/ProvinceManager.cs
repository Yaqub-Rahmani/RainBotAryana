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
    public class ProvinceManager : IProvinceService
    {
        private readonly IProvinceDal _dal;

        public ProvinceManager(IProvinceDal dal) { _dal = dal; }

        private static ProvinceDetailDto Project(Province p)
            => new ProvinceDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                IsActive = p.IsActive,
                DistrictCount = p.Districts?.Count ?? 0,
                AddressCount = p.Addresses?.Count ?? 0
            };

        [CacheAspect(10)]
        public IDataResult<List<Province>> GetAll(bool? onlyActive = null)
        {
            var q = _dal.QueryNoTracking();
            if (onlyActive == true) q = q.Where(x => x.IsActive);
            return new SuccessDataResult<List<Province>>(q.OrderBy(x => x.Name).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<Province> GetById(int id)
        {
            var e = _dal.QueryNoTracking().Include(x => x.Districts).Include(x => x.Addresses).FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<Province>(null, "یافت نشد.") : new SuccessDataResult<Province>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<ProvinceDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();
            var q = _dal.QueryNoTracking().Include(x => x.Districts).Include(x => x.Addresses).ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => x.Name.Contains(key) || (x.Code ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderBy(x => x.Name);

            var proj = q.ToList().Select(Project).AsQueryable();
            var total = proj.Count();
            var items = proj.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<ProvinceDetailDto>>(
                new PagedResult<ProvinceDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<ProvinceDetailDto> GetDetailById(int id)
        {
            var e = _dal.QueryNoTracking().Include(x => x.Districts).Include(x => x.Addresses).FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<ProvinceDetailDto>(null, "یافت نشد.") : new SuccessDataResult<ProvinceDetailDto>(Project(e));
        }

        [SecuredOperation("province.write,admin")]
        [ValidationAspect(typeof(ProvinceValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IProvinceService.Get")]
        public IResult Add(Province entity)
        {
            _dal.Add(entity);
            return new SuccessResult("ثبت شد.");
        }

        [SecuredOperation("province.write,admin")]
        [ValidationAspect(typeof(ProvinceValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IProvinceService.Get")]
        public IResult Update(Province entity)
        {
            var e = _dal.Get(x => x.Id == entity.Id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.Name = entity.Name;
            e.Code = entity.Code;
            e.IsActive = entity.IsActive;
            _dal.Update(e);
            return new SuccessResult("بروز شد.");
        }

        [SecuredOperation("province.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IProvinceService.Get")]
        public IResult Delete(int id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
