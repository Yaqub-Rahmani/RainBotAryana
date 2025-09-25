using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class AccrualValidator : AbstractValidator<Accrual>
    {
        public AccrualValidator()
        {
            RuleFor(x => x.ShopId).NotEmpty().WithMessage("شناسه دکان اجباری است.");
            RuleFor(x => x.TenantId).NotEmpty().WithMessage("شناسه کرایه‌دار اجباری است.");
            RuleFor(x => x.ContractId).NotEmpty().WithMessage("شناسه قرارداد اجباری است.");

            RuleFor(x => x.Kind).IsInEnum().WithMessage("نوع معتبر نیست.");

            RuleFor(x => x.Year).InclusiveBetween(2000, 2100)
                .WithMessage("سال باید بین 2000 تا 2100 باشد.");
            RuleFor(x => x.Month).InclusiveBetween(1, 12)
                .WithMessage("ماه باید بین 1 تا 12 باشد.");

            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0)
                .WithMessage("مبلغ نمی‌تواند منفی باشد.");
            RuleFor(x => x.OutstandingAmount).GreaterThanOrEqualTo(0)
                .WithMessage("بدهی باقیمانده نمی‌تواند منفی باشد.");

            // Elektrik ise sayaç okuması opsiyonel, ama varsa boş Guid olmasın
            RuleFor(x => x.MeterReadingId)
                .Must(id => id == null || id.Value != Guid.Empty)
                .WithMessage("شناسه قرائت برق نامعتبر است.");
        }
    }
}
