using Entities.Dtos.ZakatPayment;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.ZakatPayment
{
    public class ZakatPaymentEntryCreateValidator : AbstractValidator<ZakatPaymentEntryCreateDto>
    {
        public ZakatPaymentEntryCreateValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("مبلغ باید بزرگتر از 0 باشد.");

            RuleFor(x => x.OccurredAtUtc)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("تاریخ پرداخت نمی‌تواند در آینده باشد.");

            RuleFor(x => x.Notes)
                .MaximumLength(300)
                .WithMessage("متن توضیح بیش از حد مجاز است.");

            RuleFor(x => x.PaymentReference)
                .MaximumLength(100)
                .WithMessage("شماره مرجع بیش از حد مجاز است.");
        }
    }

    public class ZakatPaymentEntryUpdateValidator : AbstractValidator<ZakatPaymentEntryUpdateDto>
    {
        public ZakatPaymentEntryUpdateValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("شناسه نامعتبر است.");
            Include(new ZakatPaymentEntryCreateValidator());
        }
    }
}
