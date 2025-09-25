using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class DistrictValidator : AbstractValidator<District>
    {
        public DistrictValidator()
        {
            RuleFor(x => x.ProvinceId).GreaterThan(0).WithMessage("شناسه ولایت الزامی است.");
            RuleFor(x => x.Name).NotEmpty().MaximumLength(120).WithMessage("نام ناحیه الزامی و حداکثر ۱۲۰ کاراکتر است.");
            RuleFor(x => x.Code).MaximumLength(10).When(x => !string.IsNullOrWhiteSpace(x.Code))
                .WithMessage("کد حداکثر ۱۰ کاراکتر است.");
        }
    }
}
