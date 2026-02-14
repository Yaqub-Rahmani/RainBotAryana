using Entities.Dtos.AccrualSettlement;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.AccrualSettlement
{
    public class AccrualSettlementCreateValidator : AbstractValidator<AccrualSettlementCreateDto>
    {
        public AccrualSettlementCreateValidator()
        {
            RuleFor(x => x.CollectionAllocationId)
                .NotEqual(Guid.Empty).WithMessage("سطر توزیع نامعتبر است.");

            RuleFor(x => x.AccrualId)
                .NotEqual(Guid.Empty).WithMessage("بدهی (Accrual) نامعتبر است.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("مبلغ باید بزرگتر از صفر باشد.");
        }
    }


}
