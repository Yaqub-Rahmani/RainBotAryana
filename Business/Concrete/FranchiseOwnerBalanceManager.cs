using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Extensions;
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

namespace Business.Concrete
{
    public class FranchiseOwnerBalanceManager : IFranchiseOwnerBalanceService
    {
        private readonly IFranchiseOwnerBalanceDal _dal;

        public FranchiseOwnerBalanceManager(IFranchiseOwnerBalanceDal dal)
        {
            _dal = dal;
        }

        private static FranchiseOwnerBalanceDetailDto Project(FranchiseOwnerBalance e)
            => new FranchiseOwnerBalanceDetailDto
            {
                Id = e.Id,
                FranchiseOwnerId = e.FranchiseOwnerId,
                Balance = e.Balance,
                TotalIn = e.TotalIn,
                TotalOut = e.TotalOut,
                UpdatedAtUtc = e.UpdatedAtUtc,
                UpdatedAtBy = e.UpdatedAtBy,
                OwnerName = e.FranchiseOwner?.ToString() // İsteğe göre doldur
            };

        [CacheAspect(10)]
        public IDataResult<List<FranchiseOwnerBalance>> GetAll(bool? onlyActiveOwners = null)
        {
            IQueryable<FranchiseOwnerBalance> q = _dal.QueryNoTracking();
            q = q.Include(x => x.FranchiseOwner);               // OK: IIncludableQueryable -> IQueryable
                                                                // Alternatif güvenli yol: q = q.Include(...).AsQueryable();

            if (onlyActiveOwners == true)
                q = q.Where(x => x.FranchiseOwner != null /* && x.FranchiseOwner.IsActive */);

            return new SuccessDataResult<List<FranchiseOwnerBalance>>(q.ToList());
        }

        [CacheAspect(10)]
        public IDataResult<FranchiseOwnerBalance> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking()
                        .Include(x => x.FranchiseOwner)
                        .FirstOrDefault(x => x.Id == id);

            return e == null
                ? new ErrorDataResult<FranchiseOwnerBalance>(null, "یافت نشد.")
                : new SuccessDataResult<FranchiseOwnerBalance>(e);
        }

        [CacheAspect(10)]
        public IDataResult<FranchiseOwnerBalance> GetByOwnerId(Guid ownerId)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.FranchiseOwnerId == ownerId);
            return e == null
                ? new ErrorDataResult<FranchiseOwnerBalance>(null, "یافت نشد.")
                : new SuccessDataResult<FranchiseOwnerBalance>(e);
        }

        [CacheAspect(10)]
        public IDataResult<List<FranchiseOwnerBalance>> ListByOwnerId(Guid ownerId)
        {
            var list = _dal.QueryNoTracking()
                           .Where(x => x.FranchiseOwnerId == ownerId)
                           .ToList();
            return new SuccessDataResult<List<FranchiseOwnerBalance>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<FranchiseOwnerBalanceDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();

            var q = _dal.QueryNoTracking()
                        .Include(x => x.FranchiseOwner)
                        .ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.FranchiseOwnerId.ToString().Contains(key)) ||
                                 x.Balance.ToString().Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.UpdatedAtUtc);

            var proj = q.ToList().Select(Project).AsQueryable();
            var total = proj.Count();
            var items = proj.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<FranchiseOwnerBalanceDetailDto>>(
                new PagedResult<FranchiseOwnerBalanceDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<FranchiseOwnerBalanceDetailDto> GetDetailById(Guid id)
        {
            var e = _dal.QueryNoTracking().Include(x => x.FranchiseOwner).FirstOrDefault(x => x.Id == id);
            return e == null
                ? new ErrorDataResult<FranchiseOwnerBalanceDetailDto>(null, "یافت نشد.")
                : new SuccessDataResult<FranchiseOwnerBalanceDetailDto>(Project(e));
        }

        [SecuredOperation("franchise.balance.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseOwnerBalanceService.Get")]
        public IResult EnsureCreatedForOwner(Guid ownerId, Guid updatedBy)
        {
            var e = _dal.Get(x => x.FranchiseOwnerId == ownerId);
            if (e != null) return new SuccessResult("Kayıt mevcut.");

            e = new FranchiseOwnerBalance
            {
                Id = Guid.NewGuid(),
                FranchiseOwnerId = ownerId,
                UpdatedAtBy = updatedBy,
                Balance = 0m,
                TotalIn = 0m,
                TotalOut = 0m,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _dal.Add(e);
            return new SuccessResult("Oluşturuldu.");
        }

        [SecuredOperation("franchise.balance.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseOwnerBalanceService.Get")]
        public IResult UpdateIn(Guid ownerId, decimal amount, Guid updatedBy, DateTime? whenUtc = null, string? note = null)
        {
            if (amount <= 0) return new ErrorResult("Tutar pozitif olmalı.");

            var e = _dal.Get(x => x.FranchiseOwnerId == ownerId);
            if (e == null)
            {
                // idempotent: yoksa oluştur ve devam et
                e = new FranchiseOwnerBalance
                {
                    Id = Guid.NewGuid(),
                    FranchiseOwnerId = ownerId,
                    Balance = 0m,
                    TotalIn = 0m,
                    TotalOut = 0m
                };
                _dal.Add(e);
            }

            e.TotalIn += amount;
            e.Balance += amount; // IN: alacak artar
            e.UpdatedAtBy = updatedBy;
            e.UpdatedAtUtc = whenUtc ?? DateTime.UtcNow;

            _dal.Update(e);
            return new SuccessResult("Giriş işlendi.");
        }

        [SecuredOperation("franchise.balance.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseOwnerBalanceService.Get")]
        public IResult UpdateOut(Guid ownerId, decimal amount, Guid updatedBy, DateTime? whenUtc = null, string? note = null)
        {
            if (amount <= 0) return new ErrorResult("Tutar pozitif olmalı.");

            var e = _dal.Get(x => x.FranchiseOwnerId == ownerId);
            if (e == null)
                return new ErrorResult("Bakiye yok."); // İstersen otomatik oluştur

            e.TotalOut += amount;
            e.Balance -= amount; // OUT: ödeme, alacak azalır (hatta negatife inebilir)
            e.UpdatedAtBy = updatedBy;
            e.UpdatedAtUtc = whenUtc ?? DateTime.UtcNow;

            _dal.Update(e);
            return new SuccessResult("Çıkış işlendi.");
        }

        [SecuredOperation("franchise.balance.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFranchiseOwnerBalanceService.Get")]
        public IResult SetBalance(Guid ownerId, decimal balance, Guid updatedBy, DateTime? whenUtc = null)
        {
            var e = _dal.Get(x => x.FranchiseOwnerId == ownerId);
            if (e == null)
                return new ErrorResult("Bakiye yok.");

            e.Balance = balance; // manuel düzeltme
            e.UpdatedAtBy = updatedBy;
            e.UpdatedAtUtc = whenUtc ?? DateTime.UtcNow;
            _dal.Update(e);
            return new SuccessResult("Bakiye güncellendi.");
        }
    }
}
