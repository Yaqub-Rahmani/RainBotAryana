using Entities.Dtos.Collection;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.Collection
{
    public class CollectionCreateValidator : AbstractValidator<CollectionCreateDto>
    {
        public CollectionCreateValidator()
        {
            RuleFor(x => x.TenantId).NotEmpty().WithMessage("انتخاب مستأجر الزامی است.");
            RuleFor(x => x.ShopId).NotEmpty().WithMessage("انتخاب فروشگاه الزامی است.");
            RuleFor(x => x.ContractId).NotEmpty().WithMessage("انتخاب قرارداد الزامی است.");
            RuleFor(x => x.AmountReceived).GreaterThan(0).WithMessage("مبلغ دریافت‌شده باید بزرگتر از 0 باشد.");
            RuleFor(x => x.Method).IsInEnum().WithMessage("روش پرداخت نامعتبر است.");

            When(x => x.TargetYear.HasValue, () =>
            {
                RuleFor(x => x.TargetYear!.Value).InclusiveBetween(1300, 1500)
                    .WithMessage("سال هدف باید بین 1300 و 1500 باشد.");
            });

            When(x => x.TargetMonth.HasValue, () =>
            {
                RuleFor(x => x.TargetMonth!.Value).InclusiveBetween(1, 12)
                    .WithMessage("ماه هدف باید بین 1 و 12 باشد.");
            });

            RuleFor(x => x.ReferenceNo).MaximumLength(100).WithMessage("شناسه ارجاع حداکثر 100 کاراکتر است.");
            RuleFor(x => x.Notes).MaximumLength(500).WithMessage("توضیحات حداکثر 500 کاراکتر است.");
        }
    }

    public class CollectionUpdateValidator : AbstractValidator<CollectionUpdateDto>
    {
        public CollectionUpdateValidator()
        {
            Include(new CollectionCreateValidator());
            RuleFor(x => x.Id).NotEmpty().WithMessage("شناسه نامعتبر است.");
        }
    }
}
