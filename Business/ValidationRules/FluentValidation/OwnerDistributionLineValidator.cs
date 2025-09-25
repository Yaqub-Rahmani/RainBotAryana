using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class OwnerDistributionLineValidator : AbstractValidator<OwnerDistributionLine>
    {
        public OwnerDistributionLineValidator()
        {
            RuleFor(x => x.OwnerDistributionId).NotEmpty().WithMessage("شناسه توزیع مالک الزامی است.");
            RuleFor(x => x.Part).IsInEnum().WithMessage("بخش توزیع نامعتبر است.");
            RuleFor(x => x.RateUsed).GreaterThanOrEqualTo(0).WithMessage("درصد نمی‌تواند منفی باشد.");
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).WithMessage("مبلغ نمی‌تواند منفی باشد.");
            RuleFor(x => x.Notes).MaximumLength(300).WithMessage("یادداشت حداکثر ۳۰۰ کاراکتر است.");
        }
    }
}
