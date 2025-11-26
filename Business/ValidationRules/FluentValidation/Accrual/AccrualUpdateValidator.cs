using Entities.Dtos.Accrual;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.Accrual
{
    public class AccrualUpdateValidator : AbstractValidator<AccrualUpdateDto>
    {
        public AccrualUpdateValidator()
        {
            Include(new AccrualCreateValidator());
            RuleFor(x => x.Id).NotEmpty().WithMessage("شناسه نامعتبر است.");
            RuleFor(x => x.Status).IsInEnum().WithMessage("وضعیت نامعتبر است.");
            RuleFor(x => x.OutstandingAmount).GreaterThanOrEqualTo(0).WithMessage("بدهی باقی‌مانده نمی‌تواند منفی باشد.");
        }
    }
}
