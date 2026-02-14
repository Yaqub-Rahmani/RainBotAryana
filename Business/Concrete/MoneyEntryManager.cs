using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation.MoneyEntry;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Utilities.Aspects.Autofac.Validation;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos.MoneyEntry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class MoneyEntryManager : IMoneyEntryService
    {
        private readonly IMoneyEntryDal _moneyEntryDal;
        private readonly IDollarRateDal _dollarRateDal;
        private readonly ITimeProvider _timeProvider;

        public MoneyEntryManager(
            IMoneyEntryDal moneyEntryDal,
            IDollarRateDal dollarRateDal,
            ITimeProvider timeProvider)
        {
            _moneyEntryDal = moneyEntryDal;
            _dollarRateDal = dollarRateDal;
            _timeProvider = timeProvider;
        }

        [ValidationAspect(typeof(MoneyEntryCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IMoneyEntryService.Get")]
        [SecuredOperation("admin")]
        public IResult Add(MoneyEntryCreateDto dto, int userId)
        {
            var now = _timeProvider.UtcNow();

            var rate = _dollarRateDal.QueryNoTracking()
                .Where(r => r.EffectiveFromUtc <= now)
                .OrderByDescending(r => r.EffectiveFromUtc)
                .ThenByDescending(r => r.Id)
                .FirstOrDefault();

            if (rate == null)
                return new ErrorResult("هنوز هیچ نرخ دالر ثبت نشده است.");

            if (dto.Amount <= 0)
                return new ErrorResult("مبلغ باید بزرگتر از صفر باشد.");

            decimal usdEquivalent;
            switch (dto.Currency)
            {
                case Currency.Afn:
                    if (rate.AfnPerUsd <= 0)
                        return new ErrorResult("نرخ دالر نامعتبر است.");
                    usdEquivalent = Math.Round(dto.Amount / rate.AfnPerUsd, 4, MidpointRounding.AwayFromZero);
                    break;

                case Currency.Usd:
                    usdEquivalent = dto.Amount;
                    break;

                default:
                    return new ErrorResult("واحد پولی نامعتبر است.");
            }

            var entity = new MoneyEntry
            {
                Id = dto.Id,
                MoneyAccountId = dto.MoneyAccountId,
                Direction = dto.Direction,
                Amount = dto.Amount,
                Currency = dto.Currency,
                AfnPerUsdAtThatTime = rate.AfnPerUsd,
                UsdEquivalent = usdEquivalent,
                OccurredAtUtc = now,
                CreatedByUserId = userId,
                CreatedAtUtc = now,
                ExpenseId = dto.ExpenseId,
                CollectionId = dto.CollectionId
            };

            _moneyEntryDal.Add(entity);
            return new SuccessResult("حركت پولی ثبت شد.");
        }
    }
}
