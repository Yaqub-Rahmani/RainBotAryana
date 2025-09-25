using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class AddressValidator : AbstractValidator<Address>
    {
        public AddressValidator()
        {
            RuleFor(x => x.OwnerId).NotEmpty().WithMessage("شناسه مالک اجباری است.");
            RuleFor(x => x.OwnerType).IsInEnum().WithMessage("نوع مالک معتبر نیست.");

            RuleFor(x => x.ProvinceId).GreaterThan(0).WithMessage("ولایت الزامی است.");
            RuleFor(x => x.DistrictId).GreaterThan(0).WithMessage("ناحیه الزامی است.");

            RuleFor(x => x.Neighborhood).MaximumLength(100).WithMessage("محله حداکثر ۱۰۰ کاراکتر.");
            RuleFor(x => x.Street).MaximumLength(150).WithMessage("خیابان حداکثر ۱۵۰ کاراکتر.");
            RuleFor(x => x.BuildingNo).MaximumLength(50).WithMessage("شماره بنا حداکثر ۵۰ کاراکتر.");
            RuleFor(x => x.UnitNo).MaximumLength(50).WithMessage("واحد حداکثر ۵۰ کاراکتر.");
            RuleFor(x => x.PostalCode).MaximumLength(20).WithMessage("کدپستی حداکثر ۲۰ کاراکتر.");
            RuleFor(x => x.Notes).MaximumLength(500).WithMessage("یادداشت حداکثر ۵۰۰ کاراکتر.");
        }
    }
}
