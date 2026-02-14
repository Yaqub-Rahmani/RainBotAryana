using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation.MoneyAccount;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Utilities.Aspects.Autofac.Validation;
using Core.Utilities.KabulDateTime;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos.MoneyAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class MoneyAccountManager : IMoneyAccountService
    {
        private readonly IMoneyAccountDal _dal;
        private readonly ITimeProvider _timeProvider;

        public MoneyAccountManager(IMoneyAccountDal dal, ITimeProvider timeProvider)
        {
            _dal = dal;
            _timeProvider = timeProvider;
        }

        [CacheAspect(10)]
        public IDataResult<List<MoneyAccountDto>> GetAll(bool includeInactive = false)
        {
            var query = _dal.QueryNoTracking();

            if (!includeInactive)
                query = query.Where(x => x.IsActive);

            var list = query
                .OrderBy(x => x.AccountType)
                .ThenBy(x => x.Name)
                .Select(x => new MoneyAccountDto
                {
                    Id = x.Id,
                    AccountType = x.AccountType,
                    Currency = x.Currency,
                    Name = x.Name,
                    IsActive = x.IsActive
                })
                .ToList();

            return new SuccessDataResult<List<MoneyAccountDto>>(list);
        }

        [CacheAspect(10)]
        public IDataResult<MoneyAccountDto> GetById(Guid id)
        {
            var e = _dal.QueryNoTracking().FirstOrDefault(x => x.Id == id);
            if (e == null)
                return new ErrorDataResult<MoneyAccountDto>(null,"حساب یافت نشد.");

            var dto = new MoneyAccountDto
            {
                Id = e.Id,
                AccountType = e.AccountType,
                Currency = e.Currency,
                Name = e.Name,
                IsActive = e.IsActive
            };

            return new SuccessDataResult<MoneyAccountDto>(dto);
        }

        [ValidationAspect(typeof(MoneyAccountCreateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IMoneyAccountService.Get")]
        [SecuredOperation("admin")]
        public IResult Add(MoneyAccountCreateDto dto)
        {
            var entity = new MoneyAccount
            {
                Id = Guid.NewGuid(),
                AccountType = dto.AccountType,
                Currency = dto.Currency,
                Name = dto.Name.Trim(),
                IsActive = true
            };

            _dal.Add(entity);
            return new SuccessResult("حساب جدید ثبت شد.");
        }

        [ValidationAspect(typeof(MoneyAccountUpdateValidator))]
        [TransactionScopeAspect, CacheRemoveAspect("IMoneyAccountService.Get")]
        [SecuredOperation("admin")]
        public IResult Update(MoneyAccountUpdateDto dto)
        {
            var existing = _dal.Query().FirstOrDefault(x => x.Id == dto.Id);
            if (existing == null)
                return new ErrorResult("حساب یافت نشد.");

            existing.AccountType = dto.AccountType;
            existing.Currency = dto.Currency;
            existing.Name = dto.Name.Trim();
            existing.IsActive = dto.IsActive;

            _dal.Update(existing);
            return new SuccessResult("حساب ویرایش شد.");
        }

        [TransactionScopeAspect, CacheRemoveAspect("IMoneyAccountService.Get")]
        [SecuredOperation("admin")]
        public IResult Delete(Guid id)
        {
            var existing = _dal.Query().FirstOrDefault(x => x.Id == id);
            if (existing == null)
                return new ErrorResult("حساب یافت نشد.");

            // Soft delete
            existing.IsActive = false;
            _dal.Update(existing);

            return new SuccessResult("حساب غیرفعال شد.");
        }
    }
}
