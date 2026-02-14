using Entities.Dtos.DollarRate;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.DollarRate
{
    public class DollarRateCreateValidator : AbstractValidator<DollarRateCreateDto>
    {
        public DollarRateCreateValidator()
        {
            RuleFor(x => x.AfnPerUsd)
                .GreaterThan(0).WithMessage("نرخ باید بزرگتر از صفر باشد.");

            RuleFor(x => x.EffectiveFromUtc)
                .NotEmpty().WithMessage("تاریخ لازم است.");
        }
    }

    public class DollarRateUpdateValidator : AbstractValidator<DollarRateUpdateDto>
    {
        public DollarRateUpdateValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("شناسه نامعتبر است.");

            RuleFor(x => x.AfnPerUsd)
                .GreaterThan(0).WithMessage("نرخ باید بزرگتر از صفر باشد.");

            RuleFor(x => x.EffectiveFromUtc)
                .NotEmpty().WithMessage("تاریخ لازم است.");
        }
    }
}
