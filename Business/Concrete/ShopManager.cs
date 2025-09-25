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
    public class ShopManager : IShopService
    {
        private readonly IShopDal _dal;

        public ShopManager(IShopDal dal) { _dal = dal; }

        private static ShopDetailDto Project(Shop s)
            => new ShopDetailDto
            {
                Id = s.Id,
                Name = s.Name,
                UnitNumber = s.UnitNumber,
                IsActive = s.IsActive,
                MarketId = s.MarketId,
                MarketName = s.Market?.MarketName ?? "",
                FloorId = s.FloorId,
                FloorName = s.Floor?.FloorName ?? "",
                FranchiseShareCount = s.FranchiseShares?.Count ?? 0,
                ContractCount = s.Contracts?.Count ?? 0
            };

        [CacheAspect(10)]
        public IDataResult<List<Shop>> GetAll(bool? onlyActive = null, Guid? marketId = null, Guid? floorId = null)
        {
            var q = _dal.QueryNoTracking();
            if (onlyActive == true) q = q.Where(x => x.IsActive);
            if (marketId.HasValue) q = q.Where(x => x.MarketId == marketId.Value);
            if (floorId.HasValue) q = q.Where(x => x.FloorId == floorId.Value);

            return new SuccessDataResult<List<Shop>>(q.Include(x => x.Market).Include(x => x.Floor).OrderBy(x => x.Name).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<Shop> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking()
                .Include(x => x.Market)
                .Include(x => x.Floor)
                .Include(x => x.FranchiseShares)
                .Include(x => x.Contracts)
                .FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<Shop>(null, "یافت نشد.") : new SuccessDataResult<Shop>(e);
        }


        [CacheAspect(10)]
        public IDataResult<PagedResult<ShopDetailDto>> GetDetails(QueryOptions opt, Guid? marketId = null, Guid? floorId = null, bool? onlyActive = null)
        {
            opt ??= new QueryOptions();

            // 1) Temel sorgu (Include YOK)
            var q = _dal.QueryNoTracking();

            if (onlyActive == true) q = q.Where(x => x.IsActive);
            if (marketId.HasValue) q = q.Where(x => x.MarketId == marketId.Value);
            if (floorId.HasValue) q = q.Where(x => x.FloorId == floorId.Value);

            q = q.ApplyFilters(opt.Filters);

            // 2) Arama (LIKE + null check)
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                var like = $"%{key}%";
                q = q.Where(x =>
                    (x.Name != null && EF.Functions.Like(x.Name, like)) ||
                    (x.UnitNumber != null && EF.Functions.Like(x.UnitNumber, like)) ||
                    EF.Functions.Like(x.Market.MarketName, like)
                );
            }

            // 3) Projeksiyon: DTO’yu doğrudan SQL’de kur
            var qDto = q.Select(s => new ShopDetailDto
            {
                Id = s.Id,
                Name = s.Name,
                UnitNumber = s.UnitNumber,
                IsActive = s.IsActive,
                MarketId = s.MarketId,
                MarketName = s.Market.MarketName,  // Include gerekmez
                FloorId = s.FloorId,
                FloorName = s.Floor.FloorName,     // Include gerekmez
                FranchiseShareCount = s.FranchiseShares.Count(), // SQL COUNT(*)
                ContractCount = s.Contracts.Count()
            });

            // 4) Sıralama (varsayılan ekle)
            qDto = qDto.ApplySort(opt.Sort);
            qDto = qDto.OrderBy(x => x.Name ?? x.UnitNumber); // her durumda deterministik

            // 5) Sayfalama + total
            var total = qDto.Count();
            var items = qDto
                .Skip((opt.Page - 1) * opt.PageSize)
                .Take(opt.PageSize)
                .ToList();

            return new SuccessDataResult<PagedResult<ShopDetailDto>>(new PagedResult<ShopDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            });
        }

        [CacheAspect(10)]
        public IDataResult<ShopDetailDto> GetDetailById(Guid id)
        {
            var e = _dal.QueryNoTracking()
                .Include(x => x.Market).Include(x => x.Floor)
                .Include(x => x.FranchiseShares).Include(x => x.Contracts)
                .FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<ShopDetailDto>(null, "یافت نشد.") : new SuccessDataResult<ShopDetailDto>(Project(e));
        }

        [SecuredOperation("shop.write,admin")]
        [ValidationAspect(typeof(ShopValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IShopService.Get")]
        public IResult Add(Shop entity)
        {
            _dal.Add(entity);
            return new SuccessResult("ثبت شد.");
        }

        [SecuredOperation("shop.write,admin")]
        [ValidationAspect(typeof(ShopValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IShopService.Get")]
        public IResult Update(Shop entity)
        {
            var e = _dal.Get(x => x.Id == entity.Id);
            if (e == null) return new ErrorResult("یافت نشد.");

            e.MarketId = entity.MarketId;
            e.FloorId = entity.FloorId;
            e.Name = entity.Name;
            e.UnitNumber = entity.UnitNumber;
            e.IsActive = entity.IsActive;
            _dal.Update(e);
            return new SuccessResult("بروز شد.");
        }

        [SecuredOperation("shop.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IShopService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
