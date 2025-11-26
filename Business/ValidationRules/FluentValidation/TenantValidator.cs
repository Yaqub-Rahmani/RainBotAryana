using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class TenantValidator : AbstractValidator<Tenant>
    {
        public TenantValidator()
        {
            RuleFor(x => x.DisplayName).NotEmpty().WithMessage("نام مستأجر الزامی است.");
            RuleFor(x => x.DisplayName).MaximumLength(200).WithMessage("نام حداکثر ۲۰۰ کاراکتر باشد.");
            RuleFor(x => x.Email).MaximumLength(200).WithMessage("ایمیل حداکثر ۲۰۰ کاراکتر.");
            RuleFor(x => x.TaxOrIdNumber).MaximumLength(50).WithMessage("شماره مالیاتی/هویتی حداکثر ۵۰ کاراکتر.");
        }
    }
}
