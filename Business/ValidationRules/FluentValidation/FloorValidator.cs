using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class FloorValidator : AbstractValidator<Floor>
    {
        public FloorValidator()
        {
            RuleFor(x => x.MarketId).NotEmpty().WithMessage("شناسه بازار الزامی است.");
            RuleFor(x => x.FloorName)
                .NotEmpty().WithMessage("نام طبقه الزامی است.")
                .MaximumLength(100).WithMessage("نام طبقه حداکثر ۱۰۰ کاراکتر است.");
            // Level için iş kuralın yoksa aralık vermiyoruz; negatif/pozitif serbest.
        }
    }
}
