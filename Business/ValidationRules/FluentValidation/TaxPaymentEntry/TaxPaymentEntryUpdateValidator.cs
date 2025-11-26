using Entities.Dtos.TaxPaymentEntry;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.TaxPaymentEntry
{
    public class TaxPaymentEntryUpdateValidator : AbstractValidator<TaxPaymentEntryUpdateDto>
    {
        public TaxPaymentEntryUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("شناسه نامعتبر است.");

            Include(new TaxPaymentEntryCreateValidator());
        }
    }
}
