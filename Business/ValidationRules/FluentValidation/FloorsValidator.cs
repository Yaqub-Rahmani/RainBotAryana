using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class FloorsValidator : AbstractValidator<Floor>
    {
        public FloorsValidator()
        {
            RuleFor(x => x.FloorName).NotEmpty().MinimumLength(2).MaximumLength(100);
            RuleFor(x => x.Level).GreaterThanOrEqualTo(-10).LessThanOrEqualTo(100); // örnek aralık
        }
    }
}
