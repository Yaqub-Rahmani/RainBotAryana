using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class OwnerDistributionPolicyValidator : AbstractValidator<OwnerDistributionPolicy>
    {
        public OwnerDistributionPolicyValidator()
        {
            RuleFor(x => x.EffectiveFrom).NotEmpty().WithMessage("تاریخ شروع لازم است.");
            RuleFor(x => x).Must(x => !x.EffectiveTo.HasValue || x.EffectiveFrom <= x.EffectiveTo.Value)
                .WithMessage("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.");
            RuleFor(x => x.Notes).MaximumLength(300).WithMessage("یادداشت حداکثر ۳۰۰ کاراکتر.");
            RuleFor(x => x).Must(x =>
            {
                var sum = x.RateTax + x.RateZakat + x.RateCharity + x.RateOwnerReserve + x.RateFamilyPool;
                return Math.Round(sum, 2) == 100.00m;
            }).WithMessage("مجموع درصدها باید ۱۰۰٪ باشد.");
        }
    }
}
