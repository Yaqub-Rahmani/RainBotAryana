using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class ReserveEntryValidator : AbstractValidator<ReserveEntry>
    {
        public ReserveEntryValidator()
        {
            RuleFor(x => x.ReserveAccountId).NotEmpty().WithMessage("شناسهٔ حساب رزرو الزامی است.");
            RuleFor(x => x.Direction).IsInEnum().WithMessage("جهت ورودی/خروجی نامعتبر است.");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ باید بیشتر از صفر باشد.");
            RuleFor(x => x.Description).MaximumLength(500).WithMessage("توضیح حداکثر ۵۰۰ کاراکتر است.");
            RuleFor(x => x.ExternalDocNo).MaximumLength(100).WithMessage("شماره سند حداکثر ۱۰۰ کاراکتر است.");
        }
    }
}
