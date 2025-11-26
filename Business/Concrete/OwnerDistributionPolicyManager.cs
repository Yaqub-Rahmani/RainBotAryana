using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
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
using Core.Aspects.Autofac.Validation;

namespace Business.Concrete
{
    public class OwnerDistributionPolicyManager : IOwnerDistributionPolicyService
    {
        private readonly IOwnerDistributionPolicyDal _dal;

        public OwnerDistributionPolicyManager(IOwnerDistributionPolicyDal dal) { _dal = dal; }

      
    }
}
