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
    public class FamilyPayoutManager : IFamilyPayoutService
    {
        private readonly IFamilyPayoutDal _payoutDal;
        private readonly IOwnerDistributionDal _ownerDistributionDal; // varsayım: mevcut
        // İleri adım: IFamilyPayoutLineDal, IFamilyBalanceEntryDal eklenecek

        public FamilyPayoutManager(IFamilyPayoutDal payoutDal, IOwnerDistributionDal ownerDistributionDal)
        {
            _payoutDal = payoutDal;
            _ownerDistributionDal = ownerDistributionDal;
        }

        // ------ Okumalar ------

        [CacheAspect(10)]
        public IDataResult<List<FamilyPayout>> GetAll()
            => new SuccessDataResult<List<FamilyPayout>>(
                _payoutDal.QueryNoTracking().OrderByDescending(x => x.OccurredAtUtc).ToList()
            );

        [CacheAspect(10)]
        public IDataResult<FamilyPayout> GetById(Guid id)
        {
            var e = _payoutDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<FamilyPayout>(null, "یافت نشد.")
                             : new SuccessDataResult<FamilyPayout>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<FamilyPayoutDetailDto>> GetDetails(QueryOptions opt)
        {
            opt ??= new QueryOptions();
            var q = _payoutDal.QueryNoTracking();

            // Basit filtreler: ownerDistributionId=..., from/to tarihleri Filters ile verilebilir
            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.RuleSummary ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.OccurredAtUtc);

            // Henüz satır DAL’ı verilmediği için LinesCount/LinesTotal doğrudan nav’dan okunur
            var projected = q.Select(p => new FamilyPayoutDetailDto
            {
                Id = p.Id,
                OwnerDistributionId = p.OwnerDistributionId,
                TotalFamilyAmount = p.TotalFamilyAmount,
                RuleSummary = p.RuleSummary,
                OccurredAtUtc = p.OccurredAtUtc,
                LineCount = p.FamilyPayoutLines.Count,                              // EF nav (NoTracking ile de çalışır)
                LinesTotal = p.FamilyPayoutLines.Sum(l => (decimal?)l.Amount) ?? 0  // FamilyPayoutLine.Amount varsayımı
            });

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            var page = new PagedResult<FamilyPayoutDetailDto>
            {
                Items = items,
                Page = opt.Page,
                PageSize = opt.PageSize,
                TotalCount = total
            };
            return new SuccessDataResult<PagedResult<FamilyPayoutDetailDto>>(page);
        }

        [CacheAspect(10)]
        public IDataResult<FamilyPayoutDetailDto> GetDetailById(Guid id)
        {
            var dto = _payoutDal.QueryNoTracking().Where(p => p.Id == id)
                .Select(p => new FamilyPayoutDetailDto
                {
                    Id = p.Id,
                    OwnerDistributionId = p.OwnerDistributionId,
                    TotalFamilyAmount = p.TotalFamilyAmount,
                    RuleSummary = p.RuleSummary,
                    OccurredAtUtc = p.OccurredAtUtc,
                    LineCount = p.FamilyPayoutLines.Count,
                    LinesTotal = p.FamilyPayoutLines.Sum(l => (decimal?)l.Amount) ?? 0
                }).FirstOrDefault();

            return dto == null ? new ErrorDataResult<FamilyPayoutDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<FamilyPayoutDetailDto>(dto);
        }

        // ------ Yazmalar ------

        [SecuredOperation("familypayout.write,admin")]
        [ValidationAspect(typeof(FamilyPayoutValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyPayoutService.Get")]
        public IResult Add(FamilyPayout payout)
        {
            if (_ownerDistributionDal.Get(x => x.Id == payout.OwnerDistributionId) == null)
                return new ErrorResult("توزیع مالک یافت نشد.");

            // (İleri adım) Lines ile gelirse: satır toplamı TotalFamilyAmount ile eşleşiyor mu kontrolü yapılacak.

            _payoutDal.Add(payout);
            return new SuccessResult("توزیع خانوادگی ثبت شد.");
        }

        [SecuredOperation("familypayout.write,admin")]
        [ValidationAspect(typeof(FamilyPayoutValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyPayoutService.Get")]
        public IResult Update(FamilyPayout payout)
        {
            var current = _payoutDal.Get(x => x.Id == payout.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            if (_ownerDistributionDal.Get(x => x.Id == payout.OwnerDistributionId) == null)
                return new ErrorResult("توزیع مالک یافت نشد.");

            current.OwnerDistributionId = payout.OwnerDistributionId;
            current.TotalFamilyAmount = payout.TotalFamilyAmount;
            current.RuleSummary = payout.RuleSummary;
            current.OccurredAtUtc = payout.OccurredAtUtc;

            _payoutDal.Update(current);
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("familypayout.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyPayoutService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _payoutDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");

            // (İleri adım) Eğer FamilyPayoutLine ve FamilyBalanceEntry kayıtları üretildiyse,
            // burada ters hareket (undo) yapılmalı.

            _payoutDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
