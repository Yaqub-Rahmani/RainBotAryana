using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class ContractValidator : AbstractValidator<Contract>
    {
        public ContractValidator()
        {
            RuleFor(x => x.ShopId).NotEmpty().WithMessage("شناسه فروشگاه الزامی است.");
            RuleFor(x => x.TenantId).NotEmpty().WithMessage("شناسه مستاجر الزامی است.");

            RuleFor(x => x.SignedOn).NotNull().WithMessage("تاریخ امضا الزامی است.");
            RuleFor(x => x.StartDate).NotNull().WithMessage("تاریخ شروع الزامی است.");
            RuleFor(x => x.EndDate).NotNull().WithMessage("تاریخ پایان الزامی است.");

            RuleFor(x => x.DueDayOfMonth)
                .InclusiveBetween(1, 31).WithMessage("روز سررسید باید بین ۱ تا ۳۱ باشد.");

            RuleFor(x => x.OwnerPortionAmount).GreaterThanOrEqualTo(0).WithMessage("مبلغ سهم مالک منفی نیست.");
            RuleFor(x => x.FranchisePortionAmount).GreaterThanOrEqualTo(0).WithMessage("مبلغ سهم امتیاز منفی نیست.");
            RuleFor(x => x.MarketPortionAmount).GreaterThanOrEqualTo(0).WithMessage("مبلغ سهم مارکت منفی نیست.");

            RuleFor(x => x.ElectricityNote).MaximumLength(500).WithMessage("یادداشت برق حداکثر ۵۰۰ کاراکتر.");
            RuleFor(x => x.Version).MaximumLength(20).WithMessage("نسخه حداکثر ۲۰ کاراکتر.");
            RuleFor(x => x.ArchiveReason).MaximumLength(500).WithMessage("علت آرشیو حداکثر ۵۰۰ کاراکتر.");

            // تاریخ tutarlılık
            RuleFor(x => x)
                .Must(c => c.StartDate <= c.EndDate)
                .WithMessage("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.")
                .Must(c => c.SignedOn <= c.EndDate)
                .WithMessage("تاریخ امضا نمی‌تواند بعد از تاریخ پایان باشد.");
        }
    }
}
