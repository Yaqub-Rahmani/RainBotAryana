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

namespace Business.Concrete
{
    public class FloorManager : IFloorService
    {
        private readonly IFloorDal _floorDal;
        private readonly IMarketDal _marketDal;

        public FloorManager(IFloorDal floorDal, IMarketDal marketDal)
        {
            _floorDal = floorDal;
            _marketDal = marketDal;
        }

        // ---------- Okumalar ----------
        [CacheAspect(10)]
        public IDataResult<List<Floor>> GetAll(Guid? marketId = null, bool? status = null)
        {
            var q = _floorDal.QueryNoTracking();
            if (marketId.HasValue && marketId.Value != Guid.Empty)
                q = q.Where(x => x.MarketId == marketId.Value);
            if (status.HasValue)
                q = q.Where(x => x.Status == status.Value);

            var list = q.OrderByDescending(x => x.Status).ThenBy(x => x.Level).ThenBy(x => x.FloorName).ToList();
            return new SuccessDataResult<List<Floor>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<Floor> GetById(Guid id)
        {
            var e = _floorDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<Floor>(null, "یافت نشد.") : new SuccessDataResult<Floor>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<FloorDetailDto>> GetDetails(QueryOptions opt, Guid? marketId = null, bool? status = null)
        {
            opt ??= new QueryOptions();
            var q = _floorDal.QueryNoTracking();

            if (marketId.HasValue && marketId.Value != Guid.Empty)
                q = q.Where(x => x.MarketId == marketId.Value);
            if (status.HasValue)
                q = q.Where(x => x.Status == status.Value);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => x.FloorName.Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.Status).ThenBy(x => x.Level);

            // Market adını getir
            var markets = _marketDal.QueryNoTracking();
            var projected = from f in q
                            join m in markets on f.MarketId equals m.Id
                            select new FloorDetailDto
                            {
                                Id = f.Id,
                                MarketId = f.MarketId,
                                MarketName = m.MarketName,
                                FloorName = f.FloorName,
                                Level = f.Level,
                                Status = f.Status
                            };

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<FloorDetailDto>>(
                new PagedResult<FloorDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<FloorDetailDto> GetDetailById(Guid id)
        {
            var dto = (from f in _floorDal.QueryNoTracking().Where(x => x.Id == id)
                       join m in _marketDal.QueryNoTracking() on f.MarketId equals m.Id
                       select new FloorDetailDto
                       {
                           Id = f.Id,
                           MarketId = f.MarketId,
                           MarketName = m.MarketName,
                           FloorName = f.FloorName,
                           Level = f.Level,
                           Status = f.Status
                       }).FirstOrDefault();

            return dto == null ? new ErrorDataResult<FloorDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<FloorDetailDto>(dto);
        }

        // ---------- Yazmalar ----------
        [SecuredOperation("floor.write,admin")]
        [ValidationAspect(typeof(FloorValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFloorService.Get")]
        public IResult Add(Floor floor)
        {
            if (_marketDal.Get(x => x.Id == floor.MarketId) == null)
                return new ErrorResult("بازار یافت نشد.");

            // Aynı markette aynı kat adı var mı?
            var nameDup = _floorDal.Get(x => x.MarketId == floor.MarketId &&
                                             x.FloorName.ToLower() == floor.FloorName.ToLower());
            if (nameDup != null) return new ErrorResult("نام طبقه در این بازار تکراری است.");

            // Aynı markette aynı seviye var mı?
            var levelDup = _floorDal.Get(x => x.MarketId == floor.MarketId && x.Level == floor.Level);
            if (levelDup != null) return new ErrorResult("سطح طبقه در این بازار تکراری است.");

            _floorDal.Add(floor);
            return new SuccessResult("طبقه اضافه شد.");
        }

        [SecuredOperation("floor.write,admin")]
        [ValidationAspect(typeof(FloorValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFloorService.Get")]
        public IResult Update(Floor floor)
        {
            var current = _floorDal.Get(x => x.Id == floor.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            if (_marketDal.Get(x => x.Id == floor.MarketId) == null)
                return new ErrorResult("بازار یافت نشد.");

            // Çakışma kontrolleri (kendisi hariç)
            var nameDup = _floorDal.Get(x => x.Id != floor.Id &&
                                             x.MarketId == floor.MarketId &&
                                             x.FloorName.ToLower() == floor.FloorName.ToLower());
            if (nameDup != null) return new ErrorResult("نام طبقه در این بازار تکراری است.");

            var levelDup = _floorDal.Get(x => x.Id != floor.Id &&
                                              x.MarketId == floor.MarketId &&
                                              x.Level == floor.Level);
            if (levelDup != null) return new ErrorResult("سطح طبقه در این بازار تکراری است.");

            current.MarketId = floor.MarketId;
            current.FloorName = floor.FloorName;
            current.Level = floor.Level;
            current.Status = floor.Status;

            _floorDal.Update(current);
            return new SuccessResult("طبقه بروزرسانی شد.");
        }

        [SecuredOperation("floor.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFloorService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _floorDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _floorDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        [SecuredOperation("floor.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFloorService.Get")]
        public IResult Activate(Guid id)
        {
            var e = _floorDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.Status = true;
            _floorDal.Update(e);
            return new SuccessResult("فعال شد.");
        }

        [SecuredOperation("floor.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFloorService.Get")]
        public IResult Deactivate(Guid id)
        {
            var e = _floorDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.Status = false;
            _floorDal.Update(e);
            return new SuccessResult("غیرفعال شد.");
        }
    }
}
