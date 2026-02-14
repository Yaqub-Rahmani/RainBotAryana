using Entities.Dtos.OwnerDistributionPolicy;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.OwnerDistributionPolicy
{
    public class OwnerDistributionPolicyCreateValidator : AbstractValidator<OwnerDistributionPolicyCreateDto>
    {
        public OwnerDistributionPolicyCreateValidator()
        {
            RuleFor(x => x.RateTax)
                .InclusiveBetween(0, 100).WithMessage("نرخ مالیات باید بین ۰ تا ۱۰۰ باشد.");

            RuleFor(x => x.RateZakat)
                .InclusiveBetween(0, 100).WithMessage("نرخ زکات باید بین ۰ تا ۱۰۰ باشد.");

            RuleFor(x => x.RateCharity)
                .InclusiveBetween(0, 100).WithMessage("نرخ صدقه باید بین ۰ تا ۱۰۰ باشد.");

            RuleFor(x => x.RateOwnerReserve)
                .InclusiveBetween(0, 100).WithMessage("نرخ ریزرف صاحب باید بین ۰ تا ۱۰۰ باشد.");

            RuleFor(x => x.RateFamilyPool)
                .InclusiveBetween(0, 100).WithMessage("نرخ خانواده باید بین ۰ تا ۱۰۰ باشد.");

            RuleFor(x => x)
                .Must(SumIsHundred)
                .WithMessage("مجموع فیصدی‌ها باید دقیقاً ۱۰۰٪ باشد.");
        }

        private bool SumIsHundred(OwnerDistributionPolicyCreateDto dto)
        {
            var sum = dto.RateTax + dto.RateZakat + dto.RateCharity + dto.RateOwnerReserve + dto.RateFamilyPool;
            return decimal.Round(sum, 2) == 100.00m;
        }
    }

    public class OwnerDistributionPolicyUpdateValidator : AbstractValidator<OwnerDistributionPolicyUpdateDto>
    {
        public OwnerDistributionPolicyUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(System.Guid.Empty).WithMessage("شناسه نامعتبر است.");

            RuleFor(x => x.RateTax)
                .InclusiveBetween(0, 100);

            RuleFor(x => x.RateZakat)
                .InclusiveBetween(0, 100);

            RuleFor(x => x.RateCharity)
                .InclusiveBetween(0, 100);

            RuleFor(x => x.RateOwnerReserve)
                .InclusiveBetween(0, 100);

            RuleFor(x => x.RateFamilyPool)
                .InclusiveBetween(0, 100);

            RuleFor(x => x)
                .Must(SumIsHundred)
                .WithMessage("مجموع فیصدی‌ها باید دقیقاً ۱۰۰٪ باشد.");
        }

        private bool SumIsHundred(OwnerDistributionPolicyUpdateDto dto)
        {
            var sum = dto.RateTax + dto.RateZakat + dto.RateCharity + dto.RateOwnerReserve + dto.RateFamilyPool;
            return decimal.Round(sum, 2) == 100.00m;
        }
    }
}
