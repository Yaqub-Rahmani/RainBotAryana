using Entities.Dtos.MeterReadings;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.MeterReading
{
    public class MeterReadingCreateValidator : AbstractValidator<MeterReadingCreateDto>
    {
        public MeterReadingCreateValidator()
        {
            RuleFor(x => x.ShopId).NotEmpty().WithMessage("انتخاب فروشگاه الزامی است.");
            RuleFor(x => x.ContractId).NotEmpty().WithMessage("انتخاب قرارداد الزامی است.");


            RuleFor(x => x.PreviousIndex).GreaterThanOrEqualTo(0).WithMessage("اندیس قبلی نامعتبر است.");
            RuleFor(x => x.CurrentIndex).GreaterThanOrEqualTo(0).WithMessage("اندیس فعلی نامعتبر است.");
            RuleFor(x => x).Must(x => x.CurrentIndex >= x.PreviousIndex)
            .WithMessage("اندیس فعلی نمی‌تواند از اندیس قبلی کمتر باشد.");


            RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("قیمت واحد نامعتبر است.");
            RuleFor(x => x.ReserveRate).InclusiveBetween(0, 1).WithMessage("نرخ اندوخته باید بین 0 و 1 باشد.");


            RuleFor(x => x.Notes).MaximumLength(500).WithMessage("توضیحات حداکثر 500 کاراکتر است.");
        }
    }
}
