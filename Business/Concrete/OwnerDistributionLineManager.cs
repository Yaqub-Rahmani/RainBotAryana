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
    public class OwnerDistributionLineManager : IOwnerDistributionLineService
    {
        private readonly IOwnerDistributionLineDal _lineDal;
        private readonly IOwnerDistributionDal _headerDal;

        public OwnerDistributionLineManager(IOwnerDistributionLineDal lineDal, IOwnerDistributionDal headerDal)
        {
            _lineDal = lineDal;
            _headerDal = headerDal;
        }

        // ------- Helpers -------
        private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        private bool ExistsPart(Guid headerId, DistributionPart part, Guid? excludeId = null)
        {
            var q = _lineDal.QueryNoTracking().Where(x => x.OwnerDistributionId == headerId && x.Part == part);
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);
            return q.Any();
        }

        private OwnerDistributionLineDetailDto Project(OwnerDistributionLine l, OwnerDistribution? h = null)
        {
            h ??= _headerDal.QueryNoTracking().FirstOrDefault(x => x.Id == l.OwnerDistributionId);
            return new OwnerDistributionLineDetailDto
            {
                Id = l.Id,
                OwnerDistributionId = l.OwnerDistributionId,
                Part = l.Part.ToString(),
                RateUsed = l.RateUsed,
                Amount = l.Amount,
                Notes = l.Notes,
                OwnerAmountInput = h?.OwnerAmountInput,
                OccurredAtUtc = h?.OccurredAtUtc
            };
        }

        private (decimal rate, decimal amount) ExpectedFromHeader(OwnerDistribution h, DistributionPart part)
        {
            decimal rate = part switch
            {
                DistributionPart.Tax => h.RateTax,
                DistributionPart.Zakat => h.RateZakat,
                DistributionPart.Charity => h.RateCharity,
                DistributionPart.OwnerReserve => h.RateOwnerReserve,
                DistributionPart.FamilyPool => h.RateFamilyPool,
                _ => 0
            };
            decimal amount = Round2(h.OwnerAmountInput * rate / 100m);
            return (rate, amount);
        }

        // ------- Reads -------
        [CacheAspect(10)]
        public IDataResult<List<OwnerDistributionLine>> GetAll(Guid? distributionId = null)
        {
            var q = _lineDal.QueryNoTracking();
            if (distributionId.HasValue && distributionId.Value != Guid.Empty)
                q = q.Where(x => x.OwnerDistributionId == distributionId.Value);
            return new SuccessDataResult<List<OwnerDistributionLine>>(q.OrderBy(x => x.Part).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<OwnerDistributionLine> GetById(Guid id)
        {
            var e = _lineDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<OwnerDistributionLine>(null, "یافت نشد.")
                             : new SuccessDataResult<OwnerDistributionLine>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<OwnerDistributionLineDetailDto>> GetDetails(QueryOptions opt, Guid? distributionId = null)
        {
            opt ??= new QueryOptions();
            var q = _lineDal.QueryNoTracking();
            if (distributionId.HasValue && distributionId.Value != Guid.Empty)
                q = q.Where(x => x.OwnerDistributionId == distributionId.Value);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.Notes ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderBy(x => x.Part);

            // Header join (hafif şekilde tek tek projekte ediyoruz)
            var items = q.ToList().Select(l => Project(l)).AsQueryable();

            var total = items.Count();
            var pageItems = items.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<OwnerDistributionLineDetailDto>>(
                new PagedResult<OwnerDistributionLineDetailDto>
                {
                    Items = pageItems,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<OwnerDistributionLineDetailDto> GetDetailById(Guid id)
        {
            var l = _lineDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (l == null) return new ErrorDataResult<OwnerDistributionLineDetailDto>(null, "یافت نشد.");
            return new SuccessDataResult<OwnerDistributionLineDetailDto>(Project(l));
        }

        // ------- Writes -------
        [SecuredOperation("ownerdistributionline.write,admin")]
        [ValidationAspect(typeof(OwnerDistributionLineValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IOwnerDistributionLineService.Get")]
        public IResult Add(OwnerDistributionLine line, bool alignWithHeader = true)
        {
            var header = _headerDal.Get(x => x.Id == line.OwnerDistributionId);
            if (header == null) return new ErrorResult("هدر توزیع مالک یافت نشد.");

            if (ExistsPart(line.OwnerDistributionId, line.Part))
                return new ErrorResult("این بخش قبلاً برای این توزیع ثبت شده است.");

            if (alignWithHeader)
            {
                var (expRate, expAmount) = ExpectedFromHeader(header, line.Part);
                line.RateUsed = expRate;
                line.Amount = expAmount;
            }

            _lineDal.Add(line);

            // (İsteğe bağlı) FamilyPool satırı için burada FamilyPayout tetikleme kancası olabilir.

            return new SuccessResult("سطر توزیع ثبت شد.");
        }

        [SecuredOperation("ownerdistributionline.write,admin")]
        [ValidationAspect(typeof(OwnerDistributionLineValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IOwnerDistributionLineService.Get")]
        public IResult Update(OwnerDistributionLine line, bool alignWithHeader = true)
        {
            var current = _lineDal.Get(x => x.Id == line.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            var header = _headerDal.Get(x => x.Id == current.OwnerDistributionId);
            if (header == null) return new ErrorResult("هدر توزیع مالک یافت نشد.");

            if (line.OwnerDistributionId != current.OwnerDistributionId)
                return new ErrorResult("تغییر شناسه هدر مجاز نیست.");

            if (ExistsPart(line.OwnerDistributionId, line.Part, line.Id))
                return new ErrorResult("این بخش قبلاً برای این توزیع ثبت شده است.");

            current.Part = line.Part;
            current.Notes = line.Notes;

            if (alignWithHeader)
            {
                var (expRate, expAmount) = ExpectedFromHeader(header, line.Part);
                current.RateUsed = expRate;
                current.Amount = expAmount;
            }
            else
            {
                current.RateUsed = line.RateUsed;
                current.Amount = Round2(line.Amount);
            }

            _lineDal.Update(current);
            return new SuccessResult("سطر بروزرسانی شد.");
        }

        [SecuredOperation("ownerdistributionline.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IOwnerDistributionLineService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _lineDal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _lineDal.Delete(e);
            return new SuccessResult("حذف شد.");
        }

        // ------- Recalc -------
        [SecuredOperation("ownerdistributionline.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IOwnerDistributionLineService.Get")]
        public IResult RecalcFromHeader(Guid distributionId)
        {
            var header = _headerDal.Get(x => x.Id == distributionId);
            if (header == null) return new ErrorResult("هدر توزیع مالک یافت نشد.");

            var lines = _lineDal.Query().Where(x => x.OwnerDistributionId == distributionId).ToList();
            foreach (var l in lines)
            {
                var (expRate, expAmount) = ExpectedFromHeader(header, l.Part);
                l.RateUsed = expRate;
                l.Amount = expAmount;
                _lineDal.Update(l);
            }
            return new SuccessResult("سطرها بر اساس هدر مجدداً محاسبه شد.");
        }
    }
}
