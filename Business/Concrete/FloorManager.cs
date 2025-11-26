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

        [CacheAspect(10)]
        public IDataResult<List<Floor>> GetAll(bool? onlyActive = null)
        {
            var q = _floorDal.QueryNoTracking();
            if (onlyActive == true) q = q.Where(f => f.Status);
            return new SuccessDataResult<List<Floor>>(q.ToList());
        }

        [CacheAspect(10)]
        public IDataResult<Floor> GetById(Guid id)
        {
            var e = _floorDal.QueryNoTracking().FirstOrDefault(f => f.Id == id);
            return e == null
                ? new ErrorDataResult<Floor>(null, "یافت نشد.")
                : new SuccessDataResult<Floor>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<FloorDetailDto>> GetDetails(QueryOptions opt, Guid? marketId = null)
        {
            opt ??= new QueryOptions();
            var q = _floorDal.QueryNoTracking();

            if (marketId.HasValue)
                q = q.Where(f => f.MarketId == marketId.Value);

            q = q.ApplyFilters(opt.Filters);
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(f => f.FloorName.Contains(key) || f.Market.MarketName.Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderBy(f => f.Level);

            var projected = q.Select(f => new FloorDetailDto
            {
                Id = f.Id,
                FloorName = f.FloorName,
                Level = f.Level,
                Status = f.Status,
                MarketId = f.MarketId,
                MarketName = f.Market.MarketName
            });

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<FloorDetailDto>>(new PagedResult<FloorDetailDto>
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
            var dto = _floorDal.QueryNoTracking()
                .Where(f => f.Id == id)
                .Select(f => new FloorDetailDto
                {
                    Id = f.Id,
                    FloorName = f.FloorName,
                    Level = f.Level,
                    Status = f.Status,
                    MarketId = f.MarketId,
                    MarketName = f.Market.MarketName
                }).FirstOrDefault();

            return dto == null
                ? new ErrorDataResult<FloorDetailDto>(null, "یافت نشد.")
                : new SuccessDataResult<FloorDetailDto>(dto);
        }

        // ---------- Write operations ----------

        [SecuredOperation("floor.write,admin")]
        [ValidationAspect(typeof(FloorValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFloorService.Get")]
        public IResult Add(Floor floor)
        {
            if (_marketDal.Get(m => m.Id == floor.MarketId) == null)
                return new ErrorResult("بازار یافت نشد.");

            _floorDal.Add(floor);
            return new SuccessResult("طبقه با موفقیت اضافه شد.");
        }

        [SecuredOperation("floor.write,admin")]
        [ValidationAspect(typeof(FloorValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFloorService.Get")]
        public IResult Update(Floor floor)
        {
            var current = _floorDal.Get(f => f.Id == floor.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            current.FloorName = floor.FloorName;
            current.Level = floor.Level;
            current.Status = floor.Status;
            current.MarketId = floor.MarketId;

            _floorDal.Update(current);
            return new SuccessResult("به‌روزرسانی انجام شد.");
        }

        [SecuredOperation("floor.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFloorService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _floorDal.Get(f => f.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _floorDal.Delete(e);
            return new SuccessResult("حذف انجام شد.");
        }

        [SecuredOperation("floor.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFloorService.Get")]
        public IResult Activate(Guid id)
        {
            var e = _floorDal.Get(f => f.Id == id);
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
            var e = _floorDal.Get(f => f.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.Status = false;
            _floorDal.Update(e);
            return new SuccessResult("غیرفعال شد.");
        }
    }
}
