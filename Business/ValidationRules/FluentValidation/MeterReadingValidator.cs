using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class MeterReadingValidator : AbstractValidator<MeterReading>
    {
        public MeterReadingValidator()
        {
            RuleFor(x => x.ShopId).NotEmpty().WithMessage("شناسهٔ فروشگاه الزامی است.");
            RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("سال معتبر نیست.");
            RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("ماه معتبر نیست.");
            RuleFor(x => x.ReadingDate).NotEmpty().WithMessage("تاریخ قرائت الزامی است.");
            RuleFor(x => x.CurrentIndex).GreaterThanOrEqualTo(x => x.PreviousIndex)
                .WithMessage("عدد فعلی نمی‌تواند کمتر از عدد قبلی باشد.");
            RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("قیمت واحد نمی‌تواند منفی باشد.");
            RuleFor(x => x.ReserveRate).GreaterThanOrEqualTo(0).WithMessage("درصد رزرو نمی‌تواند منفی باشد.");
            RuleFor(x => x.Notes).MaximumLength(500).WithMessage("یادداشت حداکثر ۵۰۰ کاراکتر.");
        }
    }
}
