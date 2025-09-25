using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
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
    public class DistrictManager : IDistrictService
    {
        private readonly IDistrictDal _districtDal;
        private readonly IProvinceDal _provinceDal;
        private readonly IAddressDal _addressDal;

        public DistrictManager(IDistrictDal districtDal, IProvinceDal provinceDal, IAddressDal addressDal)
        {
            _districtDal = districtDal;
            _provinceDal = provinceDal;
            _addressDal = addressDal;
        }

        [CacheAspect(10)]
        public IDataResult<List<District>> GetAll()
            => new SuccessDataResult<List<District>>(
                _districtDal.QueryNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToList()
            );

        [CacheAspect(10)]
        public IDataResult<District> GetById(int id)
        {
            var e = _districtDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<District>(null, "یافت نشد.") : new SuccessDataResult<District>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<DistrictDetailDto>> GetDetails(QueryOptions opt, int? provinceId = null)
        {
            opt ??= new QueryOptions();
            var q = _districtDal.QueryNoTracking().Where(x => x.IsActive);

            if (provinceId.HasValue && provinceId.Value > 0)
                q = q.Where(x => x.ProvinceId == provinceId.Value);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => x.Name.Contains(key) || (x.Code != null && x.Code.Contains(key)));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderBy(x => x.Name);

            var projected = from d in q
                            join p in _provinceDal.QueryNoTracking() on d.ProvinceId equals p.Id
                            select new DistrictDetailDto
                            {
                                Id = d.Id,
                                ProvinceId = d.ProvinceId,
                                ProvinceName = p.Name,
                                Name = d.Name,
                                Code = d.Code,
                                IsActive = d.IsActive,
                                AddressCount = _addressDal.QueryNoTracking().Count(a => a.DistrictId == d.Id)
                            };

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<DistrictDetailDto>>(
                new PagedResult<DistrictDetailDto> { Items = items, Page = opt.Page, PageSize = opt.PageSize, TotalCount = total }
            );
        }

        [CacheAspect(10)]
        public IDataResult<DistrictDetailDto> GetDetailById(int id)
        {
            var dto = (from d in _districtDal.QueryNoTracking().Where(x => x.Id == id)
                       join p in _provinceDal.QueryNoTracking() on d.ProvinceId equals p.Id
                       select new DistrictDetailDto
                       {
                           Id = d.Id,
                           ProvinceId = d.ProvinceId,
                           ProvinceName = p.Name,
                           Name = d.Name,
                           Code = d.Code,
                           IsActive = d.IsActive,
                           AddressCount = _addressDal.QueryNoTracking().Count(a => a.DistrictId == d.Id)
                       }).FirstOrDefault();

            return dto == null
                ? new ErrorDataResult<DistrictDetailDto>(null, "یافت نشد.")
                : new SuccessDataResult<DistrictDetailDto>(dto);
        }

        [SecuredOperation("district.write,admin")]
        [ValidationAspect(typeof(DistrictValidator))]
        [CacheRemoveAspect("IDistrictService.Get")]
        public IResult Add(District district)
        {
            if (_provinceDal.Get(x => x.Id == district.ProvinceId) == null)
                return new ErrorResult("ولایت یافت نشد.");

            var exists = _districtDal.Get(x => x.ProvinceId == district.ProvinceId &&
                                               x.Name.ToLower() == district.Name.ToLower());
            if (exists != null) return new ErrorResult("نام ناحیه در این ولایت تکراری است.");

            _districtDal.Add(district);
            return new SuccessResult("ناحیه اضافه شد.");
        }

        [SecuredOperation("district.write,admin")]
        [ValidationAspect(typeof(DistrictValidator))]
        [CacheRemoveAspect("IDistrictService.Get")]
        public IResult Update(District district)
        {
            var current = _districtDal.Get(x => x.Id == district.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            if (_provinceDal.Get(x => x.Id == district.ProvinceId) == null)
                return new ErrorResult("ولایت یافت نشد.");

            var conflict = _districtDal.Get(x => x.Id != district.Id &&
                                                 x.ProvinceId == district.ProvinceId &&
                                                 x.Name.ToLower() == district.Name.ToLower());
            if (conflict != null) return new ErrorResult("نام ناحیه در این ولایت تکراری است.");

            current.ProvinceId = district.ProvinceId;
            current.Name = district.Name;
            current.Code = district.Code;
            current.IsActive = district.IsActive;

            _districtDal.Update(current);
            return new SuccessResult("ناحیه بروزرسانی شد.");
        }

        [SecuredOperation("district.write,admin")]
        [CacheRemoveAspect("IDistrictService.Get")]
        public IResult Delete(int id)
        {
            var e = _districtDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _districtDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
