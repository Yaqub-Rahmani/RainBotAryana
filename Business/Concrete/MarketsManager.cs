using Business.Abstract;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class MarketsManager : IMarketsService
    {
        private readonly IMarketsDal _marketsDal;
        public MarketsManager(IMarketsDal marketsDal) { _marketsDal = marketsDal; }

        public IDataResult<List<Market>> GetAll(bool? onlyActive = null)
        {
            var list = onlyActive == true
                ? _marketsDal.GetList(x => x.Status == true).ToList()
                : _marketsDal.GetList().ToList();

            return new SuccessDataResult<List<Market>>(list);
        }

        public IDataResult<Market> GetById(Guid id)
        {
            var entity = _marketsDal.Get(x => x.Id == id);
            return entity == null
                ? new ErrorDataResult<Market>(null, "Market not found.")
                : new SuccessDataResult<Market>(entity);
        }

        [ValidationAspect(typeof(MarketsValidator))]
        public IResult Add(Market market)
        {
            // benzersizlik kontrolü (isteğe bağlı)
            var exists = _marketsDal.Get(x => x.MarketName == market.MarketName);
            if (exists != null) return new ErrorResult("Market name already exists.");
            _marketsDal.Add(market);
            return new SuccessResult("Market added.");
        }

        [ValidationAspect(typeof(MarketsValidator))]
        public IResult Update(Market market)
        {
            var current = _marketsDal.Get(x => x.Id == market.Id);
            if (current == null) return new ErrorResult("Market not found.");

            var nameInUse = _marketsDal.Get(x => x.MarketName == market.MarketName && x.Id != market.Id);
            if (nameInUse != null) return new ErrorResult("Market name already exists.");

            _marketsDal.Update(market);
            return new SuccessResult("Market updated.");
        }

        public IResult Delete(Guid id)
        {
            var entity = _marketsDal.Get(x => x.Id == id);
            if (entity == null) return new ErrorResult("Market not found.");
            _marketsDal.Delete(entity);
            return new SuccessResult("Market deleted.");
        }

        public IResult Activate(Guid id)
        {
            var entity = _marketsDal.Get(x => x.Id == id);
            if (entity == null) return new ErrorResult("Market not found.");
            entity.Status = true;
            _marketsDal.Update(entity);
            return new SuccessResult("Market activated.");
        }

        public IResult Deactivate(Guid id)
        {
            var entity = _marketsDal.Get(x => x.Id == id);
            if (entity == null) return new ErrorResult("Market not found.");
            entity.Status = false;
            _marketsDal.Update(entity);
            return new SuccessResult("Market deactivated.");
        }
    }
}
