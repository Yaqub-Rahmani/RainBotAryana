using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class MarketValidator : AbstractValidator<Market>
    {
        public MarketValidator()
        {
            RuleFor(x => x.MarketName)
                .NotEmpty().WithMessage("نام بازار الزامی است.")
                .MaximumLength(200).WithMessage("نام بازار حداکثر ۲۰۰ کاراکتر است.");
        }
    }
}
