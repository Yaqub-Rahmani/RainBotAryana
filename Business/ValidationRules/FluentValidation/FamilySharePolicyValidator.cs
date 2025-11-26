using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class FamilySharePolicyValidator : AbstractValidator<FamilySharePolicy>
    {
        public FamilySharePolicyValidator()
        {
            RuleFor(x => x.ParentPercent)
                .InclusiveBetween(0, 100).WithMessage("درصد والد باید بین ۰ تا ۱۰۰ باشد.");

            RuleFor(x => x.FemaleWeight)
                .GreaterThan(0).WithMessage("وزن دختر باید بزرگتر از صفر باشد.");

            RuleFor(x => x.MaleWeight)
                .GreaterThan(0).WithMessage("وزن پسر باید بزرگتر از صفر باشد.");


            RuleFor(x => x.Notes).MaximumLength(300).WithMessage("یادداشت حداکثر ۳۰۰ کاراکتر.");
        }
    }
}
