using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class AccrualSettlementValidator : AbstractValidator<AccrualSettlement>
    {
        public AccrualSettlementValidator()
        {
            RuleFor(x => x.CollectionAllocationId).NotEmpty().WithMessage("شناسه تخصیص گردآوری اجباری است.");
            RuleFor(x => x.AccrualId).NotEmpty().WithMessage("شناسه بدهی اجباری است.");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ باید بزرگتر از صفر باشد.");
            RuleFor(x => x.Notes).MaximumLength(300).WithMessage("یادداشت حداکثر ۳۰۰ کاراکتر.");
        }
    }
}
