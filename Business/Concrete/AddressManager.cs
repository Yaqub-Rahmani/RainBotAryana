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
    public class AddressManager : IAddressService
    {
        private readonly IAddressDal _addressDal;
        private readonly IProvinceDal _provinceDal;
        private readonly IDistrictDal _districtDal;

        public AddressManager(IAddressDal addressDal, IProvinceDal provinceDal, IDistrictDal districtDal)
        {
            _addressDal = addressDal;
            _provinceDal = provinceDal;
            _districtDal = districtDal;
        }

        // ------------ READS ------------

        [CacheAspect(10)]
        public IDataResult<List<Address>> GetAll()
            => new SuccessDataResult<List<Address>>(
                _addressDal.QueryNoTracking().OrderByDescending(x => x.IsPrimary).ThenBy(x => x.Id).ToList()
            );

        [CacheAspect(10)]
        public IDataResult<Address> GetById(Guid id)
        {
            var e = _addressDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<Address>(null, "یافت نشد.") : new SuccessDataResult<Address>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<AddressDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();
            var q = _addressDal.QueryNoTracking();

            q = q.ApplyFilters(opt.Filters);
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(a =>
                    (a.Neighborhood ?? "").Contains(key) ||
                    (a.Street ?? "").Contains(key) ||
                    (a.Notes ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(a => a.IsPrimary).ThenBy(a => a.Id);

            var projected = from a in q
                            join p in _provinceDal.QueryNoTracking() on a.ProvinceId equals p.Id
                            join d in _districtDal.QueryNoTracking() on a.DistrictId equals d.Id
                            select new AddressDetailDto
                            {
                                Id = a.Id,
                                ProvinceId = a.ProvinceId,
                                ProvinceName = p.Name,
                                DistrictId = a.DistrictId,
                                DistrictName = d.Name,
                                Neighborhood = a.Neighborhood,
                                Street = a.Street,
                                BuildingNo = a.BuildingNo,
                                UnitNo = a.UnitNo,
                                PostalCode = a.PostalCode,
                                Notes = a.Notes,
                                IsPrimary = a.IsPrimary,
                                OwnerType = a.OwnerType.ToString(),
                                OwnerId = a.OwnerId
                            };

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            var page = new PagedResult<AddressDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            };
            return new SuccessDataResult<PagedResult<AddressDetailDto>>(page);
        }

        [CacheAspect(10)]
        public IDataResult<List<AddressDetailDto>> GetByOwner(MediaOwnerType ownerType, Guid ownerId)
        {
            var q = _addressDal.QueryNoTracking().Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId);

            var list = (from a in q
                        join p in _provinceDal.QueryNoTracking() on a.ProvinceId equals p.Id
                        join d in _districtDal.QueryNoTracking() on a.DistrictId equals d.Id
                        orderby a.IsPrimary descending, a.Id
                        select new AddressDetailDto
                        {
                            Id = a.Id,
                            ProvinceId = a.ProvinceId,
                            ProvinceName = p.Name,
                            DistrictId = a.DistrictId,
                            DistrictName = d.Name,
                            Neighborhood = a.Neighborhood,
                            Street = a.Street,
                            BuildingNo = a.BuildingNo,
                            UnitNo = a.UnitNo,
                            PostalCode = a.PostalCode,
                            Notes = a.Notes,
                            IsPrimary = a.IsPrimary,
                            OwnerType = a.OwnerType.ToString(),
                            OwnerId = a.OwnerId
                        }).ToList();

            return new SuccessDataResult<List<AddressDetailDto>>(list);
        }

        // ------------ WRITES ------------

        [SecuredOperation("address.write,admin")]
        [ValidationAspect(typeof(AddressValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IAddressService.Get")]
        public IResult Add(Address address)
        {
            // Province/District var mı?
            if (_provinceDal.Get(x => x.Id == address.ProvinceId) == null)
                return new ErrorResult("ولایت یافت نشد.");
            if (_districtDal.Get(x => x.Id == address.DistrictId) == null)
                return new ErrorResult("ناحیه یافت نشد.");

            // Eğer IsPrimary=true ise aynı owner’daki diğerlerini false yap
            if (address.IsPrimary)
            {
                var others = _addressDal.GetList(x => x.OwnerType == address.OwnerType && x.OwnerId == address.OwnerId && x.IsPrimary);
                foreach (var o in others) { o.IsPrimary = false; _addressDal.Update(o); }
            }
            else
            {
                // Bu owner’da hiç birincil yoksa, ilkini birincil yap
                var hasPrimary = _addressDal.Get(x => x.OwnerType == address.OwnerType && x.OwnerId == address.OwnerId && x.IsPrimary) != null;
                if (!hasPrimary) address.IsPrimary = true;
            }

            _addressDal.Add(address);
            return new SuccessResult("آدرس ثبت شد.");
        }

        [SecuredOperation("address.write,admin")]
        [ValidationAspect(typeof(AddressValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IAddressService.Get")]
        public IResult Update(Address address)
        {
            var current = _addressDal.Get(x => x.Id == address.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            if (_provinceDal.Get(x => x.Id == address.ProvinceId) == null)
                return new ErrorResult("ولایت یافت نشد.");
            if (_districtDal.Get(x => x.Id == address.DistrictId) == null)
                return new ErrorResult("ناحیه یافت نشد.");

            // Alanlar
            current.OwnerType = address.OwnerType;
            current.OwnerId = address.OwnerId;
            current.ProvinceId = address.ProvinceId;
            current.DistrictId = address.DistrictId;
            current.Neighborhood = address.Neighborhood;
            current.Street = address.Street;
            current.BuildingNo = address.BuildingNo;
            current.UnitNo = address.UnitNo;
            current.PostalCode = address.PostalCode;
            current.Notes = address.Notes;

            // Birincil mantığı
            if (address.IsPrimary && !current.IsPrimary)
            {
                var others = _addressDal.GetList(x => x.OwnerType == current.OwnerType && x.OwnerId == current.OwnerId && x.IsPrimary);
                foreach (var o in others) { o.IsPrimary = false; _addressDal.Update(o); }
                current.IsPrimary = true;
            }
            else
            {
                // Eğer bu adres primary'den çıkarılıyorsa ve owner’da başka primary kalmayacaksa engelle
                if (current.IsPrimary && !address.IsPrimary)
                {
                    var anotherPrimary = _addressDal.GetList(x => x.OwnerType == current.OwnerType && x.OwnerId == current.OwnerId && x.Id != current.Id && x.IsPrimary).Any();
                    if (!anotherPrimary) return new ErrorResult("حداقل یک آدرس به‌عنوان پیش‌فرض باید باقی بماند.");
                    current.IsPrimary = false;
                }
            }

            _addressDal.Update(current);
            return new SuccessResult("آدرس بروزرسانی شد.");
        }

        [SecuredOperation("address.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IAddressService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _addressDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            _addressDal.Delete(e);

            // Eğer birincil silindiyse, aynı owner’daki en eski kaydı birincil yap (varsa)
            if (e.IsPrimary)
            {
                var fallback = _addressDal.GetList(x => x.OwnerType == e.OwnerType && x.OwnerId == e.OwnerId)
                                          .OrderBy(x => x.Id).FirstOrDefault();
                if (fallback != null)
                {
                    fallback.IsPrimary = true;
                    _addressDal.Update(fallback);
                }
            }

            return new SuccessResult("حذف انجام شد.");
        }

        [SecuredOperation("address.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IAddressService.Get")]
        public IResult SetPrimary(Guid id)
        {
            var e = _addressDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            var others = _addressDal.GetList(x => x.OwnerType == e.OwnerType && x.OwnerId == e.OwnerId && x.IsPrimary && x.Id != id);
            foreach (var o in others) { o.IsPrimary = false; _addressDal.Update(o); }

            e.IsPrimary = true;
            _addressDal.Update(e);

            return new SuccessResult("به‌عنوان آدرس پیش‌فرض تنظیم شد.");
        }
    }
}
