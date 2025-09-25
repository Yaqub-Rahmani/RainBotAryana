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
    public class FamilyPayoutLineManager : IFamilyPayoutLineService
    {
        private readonly IFamilyPayoutLineDal _lineDal;
        private readonly IFamilyPayoutDal _payoutDal;
        private readonly IFamilyMemberDal _memberDal;
        private readonly IFamilyBalanceEntryDal _balanceDal;

        public FamilyPayoutLineManager(
            IFamilyPayoutLineDal lineDal,
            IFamilyPayoutDal payoutDal,
            IFamilyMemberDal memberDal,
            IFamilyBalanceEntryDal balanceDal)
        {
            _lineDal = lineDal;
            _payoutDal = payoutDal;
            _memberDal = memberDal;
            _balanceDal = balanceDal;
        }

        // ---------- Reads ----------

        [CacheAspect(10)]
        public IDataResult<List<FamilyPayoutLine>> GetAll(Guid? payoutId = null)
        {
            var q = _lineDal.QueryNoTracking();
            if (payoutId.HasValue && payoutId.Value != Guid.Empty)
                q = q.Where(x => x.FamilyPayoutId == payoutId.Value);
            return new SuccessDataResult<List<FamilyPayoutLine>>(q.OrderBy(x => x.Id).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<FamilyPayoutLine> GetById(Guid id)
        {
            var e = _lineDal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<FamilyPayoutLine>(null, "یافت نشد.") : new SuccessDataResult<FamilyPayoutLine>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<FamilyPayoutLineDetailDto>> GetDetails(QueryOptions opt, Guid? payoutId = null)
        {
            opt ??= new QueryOptions();
            var q = _lineDal.QueryNoTracking();

            if (payoutId.HasValue && payoutId.Value != Guid.Empty)
                q = q.Where(x => x.FamilyPayoutId == payoutId.Value);

            q = q.ApplyFilters(opt.Filters);
            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => (x.Notes ?? "").Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderBy(x => x.Id);

            var members = _memberDal.QueryNoTracking();
            var projected = from l in q
                            join m in members on l.FamilyMemberId equals m.Id
                            select new FamilyPayoutLineDetailDto
                            {
                                Id = l.Id,
                                FamilyPayoutId = l.FamilyPayoutId,
                                FamilyMemberId = l.FamilyMemberId,
                                FamilyMemberName = m.DisplayName,
                                Amount = l.Amount,
                                Notes = l.Notes
                            };

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<FamilyPayoutLineDetailDto>>(
                new PagedResult<FamilyPayoutLineDetailDto>
                {
                    Items = items,
                    Page = opt.Page,
                    PageSize = opt.PageSize,
                    TotalCount = total
                });
        }

        [CacheAspect(10)]
        public IDataResult<FamilyPayoutLineDetailDto> GetDetailById(Guid id)
        {
            var dto = (from l in _lineDal.QueryNoTracking().Where(x => x.Id == id)
                       join m in _memberDal.QueryNoTracking() on l.FamilyMemberId equals m.Id
                       select new FamilyPayoutLineDetailDto
                       {
                           Id = l.Id,
                           FamilyPayoutId = l.FamilyPayoutId,
                           FamilyMemberId = l.FamilyMemberId,
                           FamilyMemberName = m.DisplayName,
                           Amount = l.Amount,
                           Notes = l.Notes
                       }).FirstOrDefault();

            return dto == null ? new ErrorDataResult<FamilyPayoutLineDetailDto>(null, "یافت نشد.")
                               : new SuccessDataResult<FamilyPayoutLineDetailDto>(dto);
        }

        // ---------- Writes (Transaction + Balance sync + Header total fix) ----------

        [SecuredOperation("familypayoutline.write,admin")]
        [ValidationAspect(typeof(FamilyPayoutLineValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyPayoutLineService.Get")]
        [CacheRemoveAspect("IFamilyPayoutService.Get")]
        [CacheRemoveAspect("IFamilyBalanceEntryService.Get")]
        public IResult Add(FamilyPayoutLine line)
        {
            var payout = _payoutDal.Get(x => x.Id == line.FamilyPayoutId);
            if (payout == null) return new ErrorResult("توزیع خانوادگی یافت نشد.");

            if (_memberDal.Get(x => x.Id == line.FamilyMemberId) == null)
                return new ErrorResult("عضو خانواده یافت نشد.");

            // 1) Satırı kaydet
            _lineDal.Add(line);

            // 2) FamilyBalanceEntry (In) oluştur (benzersiz eşleştirme: Member + Payout)
            var entry = new FamilyBalanceEntry
            {
                Id = Guid.NewGuid(),
                FamilyMemberId = line.FamilyMemberId,
                Direction = BalanceDirection.In,
                Amount = line.Amount,
                OccurredAtUtc = payout.OccurredAtUtc, // başlık zamanı ile uyumlu
                FamilyPayoutId = payout.Id,
                Notes = string.IsNullOrWhiteSpace(line.Notes) ? "توزیع خانوادگی" : line.Notes
            };
            _balanceDal.Add(entry);

            // 3) Header toplamını güncelle (invariant: Header.Total == sum(lines))
            payout.TotalFamilyAmount = Math.Round((payout.TotalFamilyAmount + line.Amount), 2, MidpointRounding.AwayFromZero);
            _payoutDal.Update(payout);

            return new SuccessResult("سطر توزیع اضافه شد.");
        }

        [SecuredOperation("familypayoutline.write,admin")]
        [ValidationAspect(typeof(FamilyPayoutLineValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyPayoutLineService.Get")]
        [CacheRemoveAspect("IFamilyPayoutService.Get")]
        [CacheRemoveAspect("IFamilyBalanceEntryService.Get")]
        public IResult Update(FamilyPayoutLine line)
        {
            var current = _lineDal.Get(x => x.Id == line.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            var payout = _payoutDal.Get(x => x.Id == current.FamilyPayoutId);
            if (payout == null) return new ErrorResult("توزیع خانوادگی یافت نشد.");

            if (_memberDal.Get(x => x.Id == line.FamilyMemberId) == null)
                return new ErrorResult("عضو خانواده یافت نشد.");

            // Header toplamından eski tutarı çıkar, yenisini ekle
            payout.TotalFamilyAmount = Math.Round((payout.TotalFamilyAmount - current.Amount + line.Amount), 2, MidpointRounding.AwayFromZero);
            _payoutDal.Update(payout);

            // Satırı güncelle
            current.FamilyMemberId = line.FamilyMemberId;
            current.Amount = line.Amount;
            current.Notes = line.Notes;
            _lineDal.Update(current);

            // İlgili FamilyBalanceEntry’yi bul (Member + Payout + In)
            var entry = _balanceDal.Get(x =>
                x.FamilyPayoutId == current.FamilyPayoutId &&
                x.FamilyMemberId == current.FamilyMemberId &&
                x.Direction == BalanceDirection.In);

            if (entry != null)
            {
                entry.Amount = current.Amount;
                entry.Notes = string.IsNullOrWhiteSpace(current.Notes) ? entry.Notes : current.Notes;
                entry.OccurredAtUtc = payout.OccurredAtUtc;
                _balanceDal.Update(entry);
            }
            else
            {
                // yoksa yeniden oluştur
                _balanceDal.Add(new FamilyBalanceEntry
                {
                    Id = Guid.NewGuid(),
                    FamilyMemberId = current.FamilyMemberId,
                    Direction = BalanceDirection.In,
                    Amount = current.Amount,
                    OccurredAtUtc = payout.OccurredAtUtc,
                    FamilyPayoutId = current.FamilyPayoutId,
                    Notes = string.IsNullOrWhiteSpace(current.Notes) ? "توزیع خانوادگی" : current.Notes
                });
            }

            return new SuccessResult("سطر بروزرسانی شد.");
        }

        [SecuredOperation("familypayoutline.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IFamilyPayoutLineService.Get")]
        [CacheRemoveAspect("IFamilyPayoutService.Get")]
        [CacheRemoveAspect("IFamilyBalanceEntryService.Get")]
        public IResult Delete(Guid id)
        {
            var current = _lineDal.Get(x => x.Id == id);
            if (current == null) return new ErrorResult("یافت نشد.");

            var payout = _payoutDal.Get(x => x.Id == current.FamilyPayoutId);
            if (payout == null) return new ErrorResult("توزیع خانوادگی یافت نشد.");

            // 1) BalanceEntry’yi kaldır
            var entry = _balanceDal.Get(x =>
                x.FamilyPayoutId == current.FamilyPayoutId &&
                x.FamilyMemberId == current.FamilyMemberId &&
                x.Direction == BalanceDirection.In);
            if (entry != null) _balanceDal.Delete(entry);

            // 2) Satırı sil
            _lineDal.Delete(current);

            // 3) Header toplamını düzelt
            payout.TotalFamilyAmount = Math.Round((payout.TotalFamilyAmount - current.Amount), 2, MidpointRounding.AwayFromZero);
            if (payout.TotalFamilyAmount < 0) payout.TotalFamilyAmount = 0; // güvenlik
            _payoutDal.Update(payout);

            return new SuccessResult("حذف سطر انجام شد.");
        }
    }
}
