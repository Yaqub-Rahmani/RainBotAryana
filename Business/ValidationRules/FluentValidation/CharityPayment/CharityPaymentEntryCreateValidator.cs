using Entities.Dtos.CharityPayment;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.CharityPayment
{
    public class CharityPaymentEntryCreateValidator : AbstractValidator<CharityPaymentEntryCreateDto>
    {
        public CharityPaymentEntryCreateValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("مبلغ باید بزرگتر از 0 باشد.");

            RuleFor(x => x.OccurredAtUtc)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("تاریخ پرداخت نمی‌تواند در آینده باشد.");

            RuleFor(x => x.Notes)
                .MaximumLength(300)
                .WithMessage("طول توضیحات بیش از حد مجاز است.");

            RuleFor(x => x.PaymentReference)
                .MaximumLength(100)
                .WithMessage("شناسه پرداخت بیش از حد مجاز است.");
        }
    }

    public class CharityPaymentEntryUpdateValidator : AbstractValidator<CharityPaymentEntryUpdateDto>
    {
        public CharityPaymentEntryUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("شناسه نامعتبر است.");

            Include(new CharityPaymentEntryCreateValidator());
        }
    }
}
