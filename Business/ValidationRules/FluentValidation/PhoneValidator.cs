using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class PhoneValidator : AbstractValidator<Phone>
    {
        public PhoneValidator()
        {
            RuleFor(x => x.OwnerId).NotEmpty().WithMessage("شناسه مالک الزامی است.");
            RuleFor(x => x.Number).NotEmpty().WithMessage("شماره تلفن الزامی است.");
            RuleFor(x => x.Label).IsInEnum().WithMessage("برچسب تلفن معتبر نیست.");

            // basit + uluslararası format (opsiyonel: +90..., +93..., +1..., vs.)
            RuleFor(x => x.Number)
                .Must(BeValidPhone)
                .WithMessage("قالب شماره تلفن معتبر نیست. نمونه: +905551112233");
        }

        private bool BeValidPhone(string number)
        {
            if (string.IsNullOrWhiteSpace(number)) return false;
            // + ve 8–20 rakam arası
            return Regex.IsMatch(number, @"^\+?[0-9]{8,20}$");
        }
    }
}
