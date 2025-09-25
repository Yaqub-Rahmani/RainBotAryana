using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class FranchiseOwnerValidator : AbstractValidator<FranchiseOwner>
    {
        public FranchiseOwnerValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("نام/عنوان الزامی است.")
                .MaximumLength(200).WithMessage("نام/عنوان حداکثر ۲۰۰ کاراکتر است.");

            RuleFor(x => x.Phone).MaximumLength(50).WithMessage("تلفن حداکثر ۵۰ کاراکتر است.");
            RuleFor(x => x.Email).MaximumLength(200).WithMessage("ایمیل حداکثر ۲۰۰ کاراکتر است.");
            RuleFor(x => x.Notes).MaximumLength(1000).WithMessage("یادداشت حداکثر ۱۰۰۰ کاراکتر است.");
        }
    }
}
