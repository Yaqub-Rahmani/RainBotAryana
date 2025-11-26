using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class BiometricValidator : AbstractValidator<Biometric>
    {
        public BiometricValidator()
        {
            RuleFor(x => x.OwnerId).NotEmpty().WithMessage("شناسه مالک اجباری است.");
            RuleFor(x => x.Template).NotEmpty().WithMessage("دادهٔ قالب بایومتریک نباید خالی باشد.");
            RuleFor(x => x.Template.Length)
                .GreaterThan(100).WithMessage("طول دادهٔ قالب خیلی کوتاه است (حداقل ۱۰۰ بایت).");
        }
    }
}
