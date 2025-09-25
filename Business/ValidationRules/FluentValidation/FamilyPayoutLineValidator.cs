using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class FamilyPayoutLineValidator : AbstractValidator<FamilyPayoutLine>
    {
        public FamilyPayoutLineValidator()
        {
            RuleFor(x => x.FamilyPayoutId).NotEmpty().WithMessage("شناسه توزیع خانوادگی اجباری است.");
            RuleFor(x => x.FamilyMemberId).NotEmpty().WithMessage("شناسه عضو خانواده اجباری است.");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ باید بزرگتر از صفر باشد.");
            RuleFor(x => x.Notes).MaximumLength(300).WithMessage("یادداشت حداکثر ۳۰۰ کاراکتر.");
        }
    }
}
