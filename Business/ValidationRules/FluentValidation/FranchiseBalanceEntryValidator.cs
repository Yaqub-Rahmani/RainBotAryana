using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class FranchiseBalanceEntryValidator : AbstractValidator<FranchiseBalanceEntry>
    {
        public FranchiseBalanceEntryValidator()
        {
            RuleFor(x => x.FranchiseOwnerId).NotEmpty().WithMessage("شناسه صاحب امتیاز الزامی است.");
            RuleFor(x => x.Direction).IsInEnum().WithMessage("جهت تراکنش معتبر نیست.");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ باید بزرگتر از صفر باشد.");
            RuleFor(x => x.Notes).MaximumLength(300).WithMessage("یادداشت حداکثر ۳۰۰ کاراکتر.");
        }
    }
}
