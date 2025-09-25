using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class CategoryValidator : AbstractValidator<Category>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("CategoryName is required.")
                .MinimumLength(2).WithMessage("CategoryName must be at least 2 characters.")
                .MaximumLength(100).WithMessage("CategoryName must be at most 100 characters.");
        }
    }
}
