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
            RuleFor(x => x.CollectionId)
                .NotEqual(Guid.Empty).WithMessage("پرداخت نامعتبر است.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("مبلغ باید بزرگتر از صفر باشد.");

            RuleFor(x => x.Kind)
                .IsInEnum().WithMessage("نوع توزیع نامعتبر است.");
        }
    }
}
