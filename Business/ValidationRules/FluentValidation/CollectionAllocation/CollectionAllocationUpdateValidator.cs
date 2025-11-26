using Entities.Dtos.CollectionAllocation;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.CollectionAllocation
{
    public class CollectionAllocationUpdateValidator : AbstractValidator<CollectionAllocationUpdateDto>
    {
        public CollectionAllocationUpdateValidator()
        {
            Include(new CollectionAllocationCreateValidator());

            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("شناسه تخصیص نامعتبر است.");
        }
    }
}
