using Entities.Dtos.MoneyAccount;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.MoneyAccount
{
    public class MoneyAccountCreateValidator : AbstractValidator<MoneyAccountCreateDto>
    {
        public MoneyAccountCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("نام لازم است.")
                .MaximumLength(150).WithMessage("نام زیاد طولانی است.");

            RuleFor(x => x.AccountType)
                .IsInEnum().WithMessage("نوع حساب نامعتبر است.");

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage("واحد پول نامعتبر است.");
        }
    }

    public class MoneyAccountUpdateValidator : AbstractValidator<MoneyAccountUpdateDto>
    {
        public MoneyAccountUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty).WithMessage("شناسه نامعتبر است.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("نام لازم است.")
                .MaximumLength(150).WithMessage("نام زیاد طولانی است.");

            RuleFor(x => x.AccountType)
                .IsInEnum().WithMessage("نوع حساب نامعتبر است.");

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage("واحد پول نامعتبر است.");
        }
    }
}
