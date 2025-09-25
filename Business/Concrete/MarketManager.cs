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
    public class MarketManager : IMarketService
    {
        private readonly IMarketDal _marketDal;
        private readonly IFloorDal _floorDal;   // ilişkiler
        private readonly IShopDal _shopDal;

        public MarketManager(IMarketDal marketDal, IFloorDal floorDal, IShopDal shopDal)
        {
            _marketDal = marketDal;
            _floorDal = floorDal;
            _shopDal = shopDal;
        }

        [CacheAspect(10)]
        public IDataResult<List<Market>> GetAll(bool? status = null)
        {
            var q = _marketDal.QueryNoTracking();
            if (status.HasValue) q = q.Where(x => x.Status == status.Value);
            return new SuccessDataResult<List<Market>>(q.OrderBy(x => x.MarketName).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<Market> GetById(Guid id)
        {
            var e = _marketDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<Market>(null, "بازار یافت نشد.")
                : new SuccessDataResult<Market>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<MarketDetailDto>> GetDetails(QueryOptions opt, bool? status = null)
        {
            opt ??= new QueryOptions();
            var q = _marketDal.QueryNoTracking();
            if (status.HasValue) q = q.Where(x => x.Status == status.Value);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => x.MarketName.Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderBy(x => x.MarketName);

            var projected =
                from m in q
                select new MarketDetailDto
                {
                    Id = m.Id,
                    MarketName = m.MarketName,
                    Status = m.Status,
                    FloorCount = _floorDal.QueryNoTracking().Count(f => f.MarketId == m.Id),
                    ActiveFloorCount = _floorDal.QueryNoTracking().Count(f => f.MarketId == m.Id && f.Status),
                    ShopCount = _shopDal.QueryNoTracking().Count(s => s.MarketId == m.Id)
                };

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<MarketDetailDto>>(
                new PagedResult<MarketDetailDto> { Items = items, Page = opt.Page, PageSize = opt.PageSize, TotalCount = total });
        }

        [CacheAspect(10)]
        public IDataResult<MarketDetailDto> GetDetailById(Guid id)
        {
            var dto = _marketDal.QueryNoTracking().Where(x => x.Id == id)
                .Select(m => new MarketDetailDto
                {
                    Id = m.Id,
                    MarketName = m.MarketName,
                    Status = m.Status,
                    FloorCount = _floorDal.QueryNoTracking().Count(f => f.MarketId == m.Id),
                    ActiveFloorCount = _floorDal.QueryNoTracking().Count(f => f.MarketId == m.Id && f.Status),
                    ShopCount = _shopDal.QueryNoTracking().Count(s => s.MarketId == m.Id)
                }).FirstOrDefault();

            return dto == null
                ? new ErrorDataResult<MarketDetailDto>(null, "بازار یافت نشد.")
                : new SuccessDataResult<MarketDetailDto>(dto);
        }

        [SecuredOperation("market.write,admin")]
        [ValidationAspect(typeof(MarketValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMarketService.Get")]
        public IResult Add(Market market)
        {
            var dup = _marketDal.Get(x => x.MarketName.ToLower() == market.MarketName.ToLower());
            if (dup != null) return new ErrorResult("بازار با همین نام وجود دارد.");
            _marketDal.Add(market);
            return new SuccessResult("بازار افزوده شد.");
        }

        [SecuredOperation("market.write,admin")]
        [ValidationAspect(typeof(MarketValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMarketService.Get")]
        public IResult Update(Market market)
        {
            var current = _marketDal.Get(x => x.Id == market.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            current.MarketName = market.MarketName;
            current.Status = market.Status;
            _marketDal.Update(current);

            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("market.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMarketService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _marketDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _marketDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        [SecuredOperation("market.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMarketService.Get")]
        public IResult Activate(Guid id)
        {
            var e = _marketDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.Status = true;
            _marketDal.Update(e);
            return new SuccessResult("فعال شد.");
        }

        [SecuredOperation("market.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMarketService.Get")]
        public IResult Deactivate(Guid id)
        {
            var e = _marketDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            e.Status = false;
            _marketDal.Update(e);
            return new SuccessResult("غیرفعال شد.");
        }
    }
}
