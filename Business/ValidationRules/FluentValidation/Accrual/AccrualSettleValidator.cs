using Entities.Dtos.Accrual;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.Accrual
{
    public class AccrualSettleValidator : AbstractValidator<AccrualSettleDto>
    {
        public AccrualSettleValidator()
        {
            RuleFor(x => x.AccrualId).NotEmpty().WithMessage("شناسه نامعتبر است.");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ تسویه باید بزرگتر از 0 باشد.");
        }
    }
}
