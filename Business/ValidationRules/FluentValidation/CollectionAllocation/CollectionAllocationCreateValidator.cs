using Entities.Dtos.CollectionAllocation;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.CollectionAllocation
{
    public class CollectionAllocationCreateValidator : AbstractValidator<CollectionAllocationCreateDto>
    {
        public CollectionAllocationCreateValidator()
        {
            RuleFor(x => x.CollectionId).NotEmpty().WithMessage("شناسه دریافت معتبر نیست.");
            RuleFor(x => x.Kind).IsInEnum().WithMessage("نوع تخصیص معتبر نیست.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("مبلغ باید بیشتر از صفر باشد.");

            RuleFor(x => x.PeriodYear)
                .InclusiveBetween(1300, 1500)
                .When(x => x.PeriodYear.HasValue)
                .WithMessage("سال دوره نامعتبر است.");

            RuleFor(x => x.PeriodMonth)
                .InclusiveBetween(1, 12)
                .When(x => x.PeriodMonth.HasValue)
                .WithMessage("ماه دوره نامعتبر است.");

            RuleFor(x => x.Notes)
                .MaximumLength(300)
                .WithMessage("توضیحات حداکثر ۳۰۰ کاراکتر است.");
        }
    }
}
