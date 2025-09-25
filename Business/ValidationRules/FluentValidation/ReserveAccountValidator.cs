using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class ReserveAccountValidator : AbstractValidator<ReserveAccount>
    {
        public ReserveAccountValidator()
        {
            RuleFor(x => x.Type).IsInEnum().WithMessage("نوع حساب نامعتبر است.");
            RuleFor(x => x.DisplayName).MaximumLength(200).WithMessage("عنوان حداکثر ۲۰۰ کاراکتر است.");
        }
    }
}
