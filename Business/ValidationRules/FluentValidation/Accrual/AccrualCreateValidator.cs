using Entities.Dtos.Accrual;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.Accrual
{
    public class AccrualCreateValidator : AbstractValidator<AccrualCreateDto>
    {
        public AccrualCreateValidator()
        {
            RuleFor(x => x.ShopId).NotEmpty().WithMessage("انتخاب فروشگاه الزامی است.");
            RuleFor(x => x.TenantId).NotEmpty().WithMessage("انتخاب مستأجر الزامی است.");
            RuleFor(x => x.ContractId).NotEmpty().WithMessage("انتخاب قرارداد الزامی است.");

            RuleFor(x => x.Kind).IsInEnum().WithMessage("نوع بدهی نامعتبر است.");
            RuleFor(x => x.Year).InclusiveBetween(1300, 1500).WithMessage("سال باید بین 1300 و 1500 باشد.");
            RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("ماه باید بین 1 و 12 باشد.");

            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).WithMessage("مبلغ نمی‌تواند منفی باشد.");
        }
    }
}
