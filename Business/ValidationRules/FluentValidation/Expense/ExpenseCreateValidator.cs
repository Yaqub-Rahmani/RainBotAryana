using Entities.Dtos.Expense;
using FluentValidation;
using System;

namespace Business.ValidationRules.FluentValidation.Expense
{
    public class ExpenseCreateValidator : AbstractValidator<ExpenseCreateFromUiDto>
    {
        public ExpenseCreateValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("عنوان الزامی است.")
                .MaximumLength(200).WithMessage("عنوان نباید بیش از 200 حرف باشد.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("مبلغ باید بزرگتر از صفر باشد.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("کتگوری نامعتبر است.");

            RuleFor(x => x.Source)
                .IsInEnum().WithMessage("منبع نامعتبر است.");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum().WithMessage("روش پرداخت نامعتبر است.");

            RuleFor(x => x.MoneyAccountId)
                .NotEqual(Guid.Empty).WithMessage("حساب نامعتبر است.");

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage("واحد پولی نامعتبر است.");

            RuleFor(x => x.Direction)
                .IsInEnum().WithMessage("جهت حرکت نامعتبر است.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("یادداشت نباید بیش از 500 حرف باشد.")
                .When(x => x.Notes != null);
        }
    }
}
