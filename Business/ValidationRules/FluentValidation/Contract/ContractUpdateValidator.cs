using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.Contract
{
    public class ContractUpdateValidator : AbstractValidator<ContractUpdateDto>
    {
        public ContractUpdateValidator()
        {
            Include(new ContractCreateValidator());
            RuleFor(x => x.Id).NotEmpty().WithMessage("شناسه نامعتبر است.");
        }
    }
}