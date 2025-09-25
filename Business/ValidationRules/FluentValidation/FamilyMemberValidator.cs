using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class FamilyMemberValidator : AbstractValidator<FamilyMember>
    {
        public FamilyMemberValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("نام نمایش الزامی است.")
                .MaximumLength(200).WithMessage("نام نمایش حداکثر ۲۰۰ کاراکتر است.");

            RuleFor(x => x.RelationTag)
                .MaximumLength(50).WithMessage("برچسب نسبت حداکثر ۵۰ کاراکتر است.");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("جنسیت معتبر نیست.");
        }
    }
}
