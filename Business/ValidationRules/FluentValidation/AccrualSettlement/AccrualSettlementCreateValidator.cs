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
            RuleFor(x => x.CollectionAllocationId).NotEmpty().WithMessage("انتخاب تخصیص الزامی است.");
            RuleFor(x => x.AccrualId).NotEmpty().WithMessage("انتخاب بدهی الزامی است.");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ باید بزرگتر از 0 باشد.");
        }
    }
}
