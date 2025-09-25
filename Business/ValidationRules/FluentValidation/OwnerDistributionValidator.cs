using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class OwnerDistributionValidator : AbstractValidator<OwnerDistribution>
    {
        public OwnerDistributionValidator()
        {
            RuleFor(x => x.OwnerAmountInput).GreaterThan(0)
                .WithMessage("مبلغ ورودی باید بیشتر از صفر باشد.");
            RuleFor(x => x.RateTax).GreaterThanOrEqualTo(0);
            RuleFor(x => x.RateZakat).GreaterThanOrEqualTo(0);
            RuleFor(x => x.RateCharity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.RateOwnerReserve).GreaterThanOrEqualTo(0);
            RuleFor(x => x.RateFamilyPool).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Notes).MaximumLength(500)
                .WithMessage("یادداشت حداکثر ۵۰۰ کاراکتر.");
        }
    }
}
