using Entities.Dtos.MoneyEntry;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.MoneyEntry
{
    public class MoneyEntryCreateValidator : AbstractValidator<MoneyEntryCreateDto>
    {
        public MoneyEntryCreateValidator()
        {
            RuleFor(x => x.MoneyAccountId)
                .NotEqual(Guid.Empty).WithMessage("حساب نامعتبر است.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("مبلغ باید بزرگتر از صفر باشد.");

            RuleFor(x => x.Currency)
                .IsInEnum().WithMessage("واحد پول نامعتبر است.");

            RuleFor(x => x.Direction)
                .IsInEnum().WithMessage("جهت حركت نامعتبر است.");
        }
    }
}
