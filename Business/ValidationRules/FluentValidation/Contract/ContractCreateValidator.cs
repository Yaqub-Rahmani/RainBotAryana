using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.Contract
{
    public class ContractCreateValidator : AbstractValidator<ContractCreateDto>
    {
        public ContractCreateValidator()
        {
            RuleFor(x => x.ShopId).NotEmpty().WithMessage("انتخاب فروشگاه الزامی است.");
            RuleFor(x => x.TenantId).NotEmpty().WithMessage("انتخاب مستأجر الزامی است.");
            RuleFor(x => x.SignedOn).NotEmpty().WithMessage("تاریخ امضا الزامی است.");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("تاریخ شروع الزامی است.");
            RuleFor(x => x.EndDate).NotEmpty().WithMessage("تاریخ پایان الزامی است.")
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("تاریخ پایان باید پس از تاریخ شروع باشد.");

            RuleFor(x => x.DueDayOfMonth)
                .InclusiveBetween(1, 31).WithMessage("روز سررسید باید بین 1 تا 31 باشد.");

            RuleFor(x => x.OwnerPortionAmount).GreaterThanOrEqualTo(0).WithMessage("مبلغ مالک نمی‌تواند منفی باشد.");
            RuleFor(x => x.FranchisePortionAmount).GreaterThanOrEqualTo(0).WithMessage("مبلغ امتیاز نمی‌تواند منفی باشد.");
            RuleFor(x => x.MarketPortionAmount).GreaterThanOrEqualTo(0).WithMessage("مبلغ بازار نمی‌تواند منفی باشد.");
        }
    }
}
