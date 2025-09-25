using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class MarketMemberValidator : AbstractValidator<MarketMember>
    {
        public MarketMemberValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("نام و نام خانوادگی الزامی است.")
                .MaximumLength(200).WithMessage("نام حداکثر ۲۰۰ کاراکتر است.");

            RuleFor(x => x.MemberType)
                .MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.MemberType))
                .WithMessage("نوع عضو حداکثر ۵۰ کاراکتر است.");
        }
    }
}
