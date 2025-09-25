using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class SingleRatePolicyValidator : AbstractValidator<SingleRatePolicy>
    {
        public SingleRatePolicyValidator()
        {
            RuleFor(x => x.Type).IsInEnum().WithMessage("نوع سیاست نامعتبر است.");
            RuleFor(x => x.EffectiveFrom).NotEmpty().WithMessage("تاریخ شروع لازم است.");
            RuleFor(x => x).Must(x => !x.EffectiveTo.HasValue || x.EffectiveFrom <= x.EffectiveTo.Value)
                .WithMessage("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.");
            RuleFor(x => x.RatePercent).InclusiveBetween(0m, 100m)
                .WithMessage("درصد باید بین 0 تا 100 باشد.");
            RuleFor(x => x.Notes).MaximumLength(300).WithMessage("یادداشت حداکثر ۳۰۰ کاراکتر است.");
        }
    }
}
