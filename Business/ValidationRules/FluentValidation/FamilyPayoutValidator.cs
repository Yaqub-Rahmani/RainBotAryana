using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class FamilyPayoutValidator : AbstractValidator<FamilyPayout>
    {
        public FamilyPayoutValidator()
        {
            RuleFor(x => x.OwnerDistributionId).NotEmpty().WithMessage("شناسه توزیع مالک اجباری است.");
            RuleFor(x => x.TotalFamilyAmount).GreaterThanOrEqualTo(0).WithMessage("مبلغ کل خانواده نمی‌تواند منفی باشد.");
            RuleFor(x => x.RuleSummary).MaximumLength(500).WithMessage("خلاصه قاعده حداکثر ۵۰۰ کاراکتر.");
        }
    }
}
