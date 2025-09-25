using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class CollectionValidator : AbstractValidator<Collection>
    {
        public CollectionValidator()
        {
            RuleFor(x => x.TenantId).NotEmpty().WithMessage("شناسه مستاجر الزامی است.");
            RuleFor(x => x.ShopId).NotEmpty().WithMessage("شناسه فروشگاه الزامی است.");
            RuleFor(x => x.ContractId).NotEmpty().WithMessage("شناسه قرارداد الزامی است.");

            RuleFor(x => x.AmountReceived).GreaterThan(0).WithMessage("مبلغ باید بزرگتر از صفر باشد.");
            RuleFor(x => x.ReferenceNo).MaximumLength(100).WithMessage("شماره مرجع حداکثر ۱۰۰ کاراکتر.");
            RuleFor(x => x.Notes).MaximumLength(500).WithMessage("یادداشت حداکثر ۵۰۰ کاراکتر.");
        }

    }
}
