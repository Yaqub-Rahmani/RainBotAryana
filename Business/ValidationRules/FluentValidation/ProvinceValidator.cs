using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class ProvinceValidator : AbstractValidator<Province>
    {
        public ProvinceValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("نام ولایت الزامی است.");
            RuleFor(x => x.Name).MaximumLength(200).WithMessage("نام ولایت حداکثر ۲۰۰ کاراکتر باشد.");
            RuleFor(x => x.Code).MaximumLength(20).WithMessage("کد حداکثر ۲۰ کاراکتر باشد.");
        }
    }
}
