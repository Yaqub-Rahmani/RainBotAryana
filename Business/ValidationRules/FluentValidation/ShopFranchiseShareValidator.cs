using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class ShopFranchiseShareValidator : AbstractValidator<ShopFranchiseShare>
    {
        public ShopFranchiseShareValidator()
        {
            RuleFor(x => x.ShopId).NotEmpty().WithMessage("شناسهٔ فروشگاه الزامی است.");
            RuleFor(x => x.FranchiseOwnerId).NotEmpty().WithMessage("شناسهٔ صاحب امتیاز الزامی است.");
            RuleFor(x => x.SharePercent).InclusiveBetween(0.01m, 100m).WithMessage("درصد باید بین 0.01 تا 100 باشد.");
            RuleFor(x => x).Must(x => !x.EffectiveTo.HasValue || x.EffectiveFrom <= x.EffectiveTo.Value)
                .WithMessage("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.");
            RuleFor(x => x.Notes).MaximumLength(500).WithMessage("یادداشت حداکثر ۵۰۰ کاراکتر.");
        }
    }
}
