using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class ShopValidator : AbstractValidator<Shop>
    {
        public ShopValidator()
        {
            RuleFor(x => x.MarketId).NotEmpty().WithMessage("مارکت الزامی است.");
            RuleFor(x => x.FloorId).NotEmpty().WithMessage("طبقه الزامی است.");
            RuleFor(x => x.Name).MaximumLength(200).WithMessage("نام فروشگاه حداکثر ۲۰۰ کاراکتر باشد.");
            RuleFor(x => x.UnitNumber).MaximumLength(50).WithMessage("شماره واحد حداکثر ۵۰ کاراکتر باشد.");
        }
    }
}
