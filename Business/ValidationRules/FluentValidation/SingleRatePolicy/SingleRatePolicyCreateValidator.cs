using Entities.Dtos.SingleRatePolicy;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.SingleRatePolicy
{
    public class SingleRatePolicyCreateValidator : AbstractValidator<SingleRatePolicyCreateDto>
    {
        public SingleRatePolicyCreateValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("نوع پالیسی نامعتبر است.");

            RuleFor(x => x.RatePercent)
                .GreaterThanOrEqualTo(0).WithMessage("نرخ نمی‌تواند منفی باشد.")
                .LessThanOrEqualTo(100).WithMessage("نرخ نباید از ۱۰۰٪ بیشتر باشد.");
        }
    }

    public class SingleRatePolicyUpdateValidator : AbstractValidator<SingleRatePolicyUpdateDto>
    {
        public SingleRatePolicyUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(System.Guid.Empty).WithMessage("شناسه نامعتبر است.");

            RuleFor(x => x.RatePercent)
                .GreaterThanOrEqualTo(0).WithMessage("نرخ نمی‌تواند منفی باشد.")
                .LessThanOrEqualTo(100).WithMessage("نرخ نباید از ۱۰۰٪ بیشتر باشد.");
        }
    }
}
