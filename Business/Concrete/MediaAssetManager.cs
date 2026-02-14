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
    public class MediaAssetManager : IMediaAssetService
    {
        private readonly IMediaAssetDal _dal;
        public MediaAssetManager(IMediaAssetDal dal) { _dal = dal; }

        // -------- Reads --------
        [CacheAspect(10)]
        public IDataResult<List<MediaAsset>> GetAll(Guid? ownerId = null, MediaOwnerType? ownerType = null)
        {
            var q = _dal.QueryNoTracking();
            if (ownerId.HasValue && ownerId.Value != Guid.Empty) q = q.Where(x => x.OwnerId == ownerId);
            if (ownerType.HasValue) q = q.Where(x => x.OwnerType == ownerType);
            return new SuccessDataResult<List<MediaAsset>>(q.OrderByDescending(x => x.CreatedAt).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<MediaAsset> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<MediaAsset>(null, "یافت نشد.") : new SuccessDataResult<MediaAsset>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<MediaAssetDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();
            var q = _dal.QueryNoTracking();
            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => x.StorageUrl.Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.CreatedAt);

            var projected = q.Select(p => new MediaAssetDetailDto
            {
                Id = p.Id,
                OwnerType = p.OwnerType.ToString(),
                OwnerId = p.OwnerId,
                Kind = p.Kind.ToString(),
                StorageUrl = p.StorageUrl,
                HashChecksum = p.HashChecksum,
                IsPrimary = p.IsPrimary,
                CreatedAt = p.CreatedAt
            });

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<MediaAssetDetailDto>>(
                new PagedResult<MediaAssetDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<MediaAssetDetailDto> GetDetailById(Guid id)
        {
            var dto = _dal.QueryNoTracking().Where(x => x.Id == id)
                .Select(p => new MediaAssetDetailDto
                {
                    Id = p.Id,
                    OwnerType = p.OwnerType.ToString(),
                    OwnerId = p.OwnerId,
                    Kind = p.Kind.ToString(),
                    StorageUrl = p.StorageUrl,
                    HashChecksum = p.HashChecksum,
                    IsPrimary = p.IsPrimary,
                    CreatedAt = p.CreatedAt
                }).FirstOrDefault();

            return dto == null
                ? new ErrorDataResult<MediaAssetDetailDto>(null, "یافت نشد.")
                : new SuccessDataResult<MediaAssetDetailDto>(dto);
        }

        // -------- Writes --------
        [SecuredOperation("mediaasset.write,admin")]
        [ValidationAspect(typeof(MediaAssetValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMediaAssetService.Get")]
        [CacheRemoveAspect(".*FamilyMember.*Get.*")]
        [CacheRemoveAspect(".*FranchiseOwner.*Get.*")]
        [CacheRemoveAspect(".*Employee.*Get.*")]
        public IResult Add(MediaAsset asset)
        {
            asset.CreatedAt = DateTime.UtcNow;

            if (asset.IsPrimary)
            {
                // Aynı owner+kind için mevcut primary varsa kapat
                var currentPrimary = _dal.Get(x => x.OwnerType == asset.OwnerType && x.OwnerId == asset.OwnerId && x.Kind == asset.Kind && x.IsPrimary);
                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                    _dal.Update(currentPrimary);
                }
            }

            _dal.Add(asset);
            return new SuccessResult("رسانه ثبت شد.");
        }

        [SecuredOperation("mediaasset.write,admin")]
        [ValidationAspect(typeof(MediaAssetValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMediaAssetService.Get")]
        [CacheRemoveAspect(".*FamilyMember.*Get.*")]
        [CacheRemoveAspect(".*FranchiseOwner.*Get.*")]
        public IResult Update(MediaAsset asset)
        {
            var current = _dal.Get(x => x.Id == asset.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            if (asset.IsPrimary && !current.IsPrimary)
            {
                // Diğer primary'i kapat
                var oldPrimary = _dal.Get(x => x.OwnerType == asset.OwnerType && x.OwnerId == asset.OwnerId && x.Kind == asset.Kind && x.IsPrimary);
                if (oldPrimary != null && oldPrimary.Id != asset.Id)
                {
                    oldPrimary.IsPrimary = false;
                    _dal.Update(oldPrimary);
                }
            }

            current.OwnerType = asset.OwnerType;
            current.OwnerId = asset.OwnerId;
            current.Kind = asset.Kind;
            current.StorageUrl = asset.StorageUrl;
            current.HashChecksum = asset.HashChecksum;
            current.IsPrimary = asset.IsPrimary;

            _dal.Update(current);
            return new SuccessResult("بروزرسانی انجام شد.");
        }

        [SecuredOperation("mediaasset.write,admin")]
        [ValidationAspect(typeof(MediaAssetValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMediaAssetService.Get")]
        [CacheRemoveAspect(".*FamilyMember.*Get.*")]
        [CacheRemoveAspect(".*FranchiseOwner.*Get.*")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        // -------- Primary setter --------
        [SecuredOperation("mediaasset.write,admin")]
        [ValidationAspect(typeof(MediaAssetValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IMediaAssetService.Get")]
        [CacheRemoveAspect(".*FamilyMember.*Get.*")]
        [CacheRemoveAspect(".*FranchiseOwner.*Get.*")]
        public IResult SetPrimary(Guid id)
        {
            var target = _dal.Get(x => x.Id == id);
            if (target == null) return new ErrorResult("یافت نشد.");

            var existing = _dal.Get(x => x.OwnerType == target.OwnerType && x.OwnerId == target.OwnerId && x.Kind == target.Kind && x.IsPrimary);
            if (existing != null && existing.Id != target.Id)
            {
                existing.IsPrimary = false;
                _dal.Update(existing);
            }

            target.IsPrimary = true;
            _dal.Update(target);
            return new SuccessResult("به عنوان اصلی تنظیم شد.");
        }
    }
}
