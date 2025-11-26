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
    public class SingleRatePolicyManager : ISingleRatePolicyService
    {
        private readonly ISingleRatePolicyDal _dal;
        public SingleRatePolicyManager(ISingleRatePolicyDal dal) { _dal = dal; }

        // Helpers
        private static SingleRatePolicyDetailDto Project(SingleRatePolicy p) => new()
        {
            Id = p.Id,
            Type = p.Type.ToString(),
            RatePercent = p.RatePercent,
            Notes = p.Notes
        };

        private bool Overlaps(SingleRateType type, DateOnly from, DateOnly? to, Guid? excludeId = null)
        {
            var q = _dal.QueryNoTracking().Where(x => x.Type == type);
            if (excludeId.HasValue) q = q.Where(x => x.Id != excludeId.Value);

            // Kural: [p.From, p.To]  ile  [from, to] çakışır ⇔
            // p.From <= (to ?? +∞)  VE  (p.To == null || p.To >= from)
            return q.Any();
        }


        // Reads

    }
}
