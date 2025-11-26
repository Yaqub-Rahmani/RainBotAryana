using Entities.Dtos.MeterReadings;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.MeterReading
{
    public class MeterReadingUpdateValidator : AbstractValidator<MeterReadingUpdateDto>
    {
        public MeterReadingUpdateValidator()
        {
            Include(new MeterReadingCreateValidator());
            RuleFor(x => x.Id).NotEmpty().WithMessage("شناسه نامعتبر است.");
            RuleFor(x => x.RowVersion).NotNull().Must(rv => rv != null && rv.Length > 0)
            .WithMessage("نسخه ردیف (RowVersion) الزامی است.");
        }
    }
}
