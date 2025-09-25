using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class MarketsValidator : AbstractValidator<Market>
    {
        public MarketsValidator()
        {
            RuleFor(x => x.MarketName)
                .NotEmpty().WithMessage("MarketName is required.")
                .MinimumLength(2).WithMessage("MarketName must be at least 2 characters.")
                .MaximumLength(100).WithMessage("MarketName must be at most 100 characters.");
        }
    }
}
