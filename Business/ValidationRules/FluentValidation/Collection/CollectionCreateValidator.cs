using Entities.Dtos.Collection;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.Collection
{
    public class CollectionCreateValidator : AbstractValidator<CollectionCreateDto>
    {
        public CollectionCreateValidator()
        {
            RuleFor(x => x.TenantId)
                .NotEqual(Guid.Empty).WithMessage("کرایه‌دار نامعتبر است.");

            RuleFor(x => x.ShopId)
                .NotEqual(Guid.Empty).WithMessage("دکان نامعتبر است.");

            RuleFor(x => x.ContractId)
                .NotEqual(Guid.Empty).WithMessage("قرارداد نامعتبر است.");

            RuleFor(x => x.AmountReceived)
                .GreaterThan(0).WithMessage("مبلغ باید بزرگتر از صفر باشد.");

            RuleFor(x => x.Method)
                .IsInEnum().WithMessage("طریقه پرداخت نامعتبر است.");

            RuleFor(x => x.MoneyAccountId)
                .NotEqual(Guid.Empty).WithMessage("حساب دریافت نامعتبر است.");
        }
    }

    public class CollectionApproveValidator : AbstractValidator<CollectionApproveDto>
    {
        public CollectionApproveValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty).WithMessage("شناسه پرداخت نامعتبر است.");
        }
    }
}
