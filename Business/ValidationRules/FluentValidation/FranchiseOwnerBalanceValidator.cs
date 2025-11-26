using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class FranchiseOwnerBalanceValidator : AbstractValidator<FranchiseOwnerBalance>
    {
        public FranchiseOwnerBalanceValidator()
        {
            RuleFor(x => x.FranchiseOwnerId)
                .NotEmpty().WithMessage("شناسه مالک امتیاز الزامی است.");

            RuleFor(x => x.Balance)
                .PrecisionScale(18, 2, true)   // eski ScalePrecision yerine
                .WithMessage("Balance formatı decimal(18,2) olmalı.");

            RuleFor(x => x.TotalIn)
                .PrecisionScale(18, 2, true)
                .WithMessage("TotalIn formatı decimal(18,2) olmalı.");

            RuleFor(x => x.TotalOut)
                .PrecisionScale(18, 2, true)
                .WithMessage("TotalOut formatı decimal(18,2) olmalı.");
        }
    }
}
