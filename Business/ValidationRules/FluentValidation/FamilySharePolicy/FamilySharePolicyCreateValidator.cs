using Entities.Dtos.FamilySharePolicy;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.FamilySharePolicy
{
    public class FamilySharePolicyCreateValidator : AbstractValidator<FamilySharePolicyCreateDto>
    {
        public FamilySharePolicyCreateValidator()
        {
            RuleFor(x => x.ParentPercent)
                .InclusiveBetween(0, 100)
                .WithMessage("سهم والد باید بین ۰ تا ۱۰۰ باشد.");

            RuleFor(x => x.FemaleWeight)
                .GreaterThan(0).WithMessage("وزن دختر باید بزرگتر از صفر باشد.");

            RuleFor(x => x.MaleWeight)
                .GreaterThan(0).WithMessage("وزن پسر باید بزرگتر از صفر باشد.");
        }
    }

    public class FamilySharePolicyUpdateValidator : AbstractValidator<FamilySharePolicyUpdateDto>
    {
        public FamilySharePolicyUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(System.Guid.Empty).WithMessage("شناسه نامعتبر است.");

            RuleFor(x => x.ParentPercent)
                .InclusiveBetween(0, 100)
                .WithMessage("سهم والد باید بین ۰ تا ۱۰۰ باشد.");

            RuleFor(x => x.FemaleWeight)
                .GreaterThan(0).WithMessage("وزن دختر باید بزرگتر از صفر باشد.");

            RuleFor(x => x.MaleWeight)
                .GreaterThan(0).WithMessage("وزن پسر باید بزرگتر از صفر باشد.");
        }
    }
}
