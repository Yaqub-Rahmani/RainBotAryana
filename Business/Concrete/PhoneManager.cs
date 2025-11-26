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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class PhoneManager : IPhoneService
    {
        private readonly IPhoneDal _dal;
        public PhoneManager(IPhoneDal dal) { _dal = dal; }

        // --- Normalize (E.164 benzeri) ---
        //public string Normalize(string raw)
        //{
        //    if (string.IsNullOrWhiteSpace(raw)) return raw;
        //    var trimmed = Regex.Replace(raw, @"\s+|-|\(|\)", ""); // boşluk/kısa çizgi/parantezleri sil
        //    // ülke kodu yoksa (+) olmadan başlıyorsa, istersen buradan default ülke ekleyebilirsin
        //    if (!trimmed.StartsWith("+") && Regex.IsMatch(trimmed, @"^\d+$"))
        //        return "+" + trimmed;  // basit yaklaşım
        //    return trimmed;
        //}

        public string Normalize(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return raw;

            // boşluk, kısa çizgi, parantez temizle
            var trimmed = Regex.Replace(raw, @"\s+|-|\(|\)", "");

            // ❌ Artık hiçbir şartta + eklenmiyor
            return trimmed;
        }



        // ------------- Reads -------------
        [CacheAspect(10)]
        public IDataResult<List<Phone>> GetAll(Guid? ownerId = null, MediaOwnerType? ownerType = null)
        {
            var q = _dal.QueryNoTracking();
            if (ownerId.HasValue && ownerId.Value != Guid.Empty) q = q.Where(x => x.OwnerId == ownerId.Value);
            if (ownerType.HasValue) q = q.Where(x => x.OwnerType == ownerType.Value);

            return new SuccessDataResult<List<Phone>>(q.OrderByDescending(x => x.IsVerified).ThenBy(x => x.Label).ToList());
        }

        [CacheAspect(10)]
        public IDataResult<Phone> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<Phone>(null, "یافت نشد.") : new SuccessDataResult<Phone>(e);
        }

        [CacheAspect(10)]
        public IDataResult<PagedResult<PhoneDetailDto>> GetDetails(QueryOptions opt, Guid? ownerId = null, MediaOwnerType? ownerType = null)
        {
            opt ??= new QueryOptions();
            var q = _dal.QueryNoTracking();

            if (ownerId.HasValue && ownerId.Value != Guid.Empty) q = q.Where(x => x.OwnerId == ownerId.Value);
            if (ownerType.HasValue) q = q.Where(x => x.OwnerType == ownerType.Value);

            q = q.ApplyFilters(opt.Filters);

            if (!string.IsNullOrWhiteSpace(opt.Search))
            {
                var key = opt.Search.Trim();
                q = q.Where(x => x.Number.Contains(key));
            }

            q = q.ApplySort(opt.Sort);
            if (!q.Expression.ToString()!.Contains("OrderBy"))
                q = q.OrderByDescending(x => x.IsVerified).ThenBy(x => x.Label);

            var projected = q.Select(p => new PhoneDetailDto
            {
                Id = p.Id,
                OwnerType = p.OwnerType.ToString(),
                OwnerId = p.OwnerId,
                Number = p.Number,
                Label = p.Label.ToString(),
                IsVerified = p.IsVerified,
                CreatedAt = p.CreatedAt
            });

            var total = projected.Count();
            var items = projected.ApplyPaging(opt.Page, opt.PageSize).ToList();

            return new SuccessDataResult<PagedResult<PhoneDetailDto>>(
                new PagedResult<PhoneDetailDto> { Items = items, Page = opt.Page, PageSize = opt.PageSize, TotalCount = total }
            );
        }

        [CacheAspect(10)]
        public IDataResult<PhoneDetailDto> GetDetailById(Guid id)
        {
            var dto = _dal.QueryNoTracking().Where(x => x.Id == id)
                .Select(p => new PhoneDetailDto
                {
                    Id = p.Id,
                    OwnerType = p.OwnerType.ToString(),
                    OwnerId = p.OwnerId,
                    Number = p.Number,
                    Label = p.Label.ToString(),
                    IsVerified = p.IsVerified,
                    CreatedAt = p.CreatedAt
                }).FirstOrDefault();

            return dto == null ? new ErrorDataResult<PhoneDetailDto>(null, "یافت نشد.") : new SuccessDataResult<PhoneDetailDto>(dto);
        }

        // ------------- Writes -------------
        [SecuredOperation("phone.write,admin")]
        [ValidationAspect(typeof(PhoneValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IPhoneService.Get")]
        public IResult Add(Phone phone)
        {
            phone.Number = Normalize(phone.Number);
            phone.CreatedAt = DateTime.UtcNow;

            // Aynı owner + aynı numara zaten kayıtlı mı?
            var dup = _dal.Get(x => x.OwnerType == phone.OwnerType && x.OwnerId == phone.OwnerId && x.Number == phone.Number);
            if (dup != null) return new ErrorResult("این شماره برای این مالک قبلاً ثبت شده است.");

            // Aynı owner + aynı label tek olsun istiyorsan:
            // var labelDup = _dal.Get(x => x.OwnerType == phone.OwnerType && x.OwnerId == phone.OwnerId && x.Label == phone.Label);
            // if (labelDup != null) return new ErrorResult("برای این مالک قبلاً شماره‌ای با همین برچسب وجود دارد.");

            _dal.Add(phone);
            return new SuccessResult("شماره تلفن ثبت شد.");
        }

        [SecuredOperation("phone.write,admin")]
        [ValidationAspect(typeof(PhoneValidator))]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IPhoneService.Get")]
        public IResult Update(Phone phone)
        {
            var current = _dal.Get(x => x.Id == phone.Id);
            if (current == null) return new ErrorResult("یافت نشد.");

            var normalized = Normalize(phone.Number);

            // aynı owner + (başkası) + aynı numara var mı?
            var dup = _dal.Get(x => x.Id != phone.Id && x.OwnerType == phone.OwnerType && x.OwnerId == phone.OwnerId && x.Number == normalized);
            if (dup != null) return new ErrorResult("این شماره برای این مالک قبلاً ثبت شده است.");

            current.OwnerType = phone.OwnerType;
            current.OwnerId = phone.OwnerId;
            current.Number = normalized;
            current.Label = phone.Label;
            current.IsVerified = phone.IsVerified;

            _dal.Update(current);
            return new SuccessResult("بروزرسانی شد.");
        }

        [SecuredOperation("phone.write,admin")]
        [TransactionScopeAspect]
        [CacheRemoveAspect("IPhoneService.Get")]
        public IResult Delete(Guid id)
        {
            var e = _dal.Get(x => x.Id == id);
            if (e == null) return new ErrorResult("یافت نشد.");
            _dal.Delete(e);
            return new SuccessResult("حذف شد.");
        }
    }
}
