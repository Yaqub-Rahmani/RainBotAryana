using Entities.Dtos.TaxPaymentEntry;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.TaxPaymentEntry
{
    public class TaxPaymentEntryCreateValidator : AbstractValidator<TaxPaymentEntryCreateDto>
    {
        public TaxPaymentEntryCreateValidator()
        {
            RuleFor(x => x.Direction)
                .IsInEnum()
                .WithMessage("جهت تراکنش نامعتبر است.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("مبلغ باید بزرگتر از 0 باشد.");

            RuleFor(x => x.OccurredAtUtc)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("تاریخ پرداخت نمی‌تواند در آینده باشد.");

            RuleFor(x => x.Notes)
                .MaximumLength(300)
                .WithMessage("توضیحات حداکثر 300 کاراکتر است.");
            RuleFor(x => x.PaymentReference)
                .MaximumLength(100)
                .WithMessage("شماره مرجع بیش از حد مجاز است.");
        }
    }
}
