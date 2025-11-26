using Entities.Concrete;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation
{
    public class MediaAssetValidator : AbstractValidator<MediaAsset>
    {
        public MediaAssetValidator()
        {
            RuleFor(x => x.OwnerId).NotEmpty().WithMessage("شناسهٔ مالک الزامی است.");
            RuleFor(x => x.StorageUrl).NotEmpty().WithMessage("نشانی ذخیره‌سازی الزامی است.");
            RuleFor(x => x.StorageUrl).MaximumLength(400).WithMessage("طول URL حداکثر ۴۰۰ کاراکتر است.");
            RuleFor(x => x.HashChecksum).MaximumLength(128).WithMessage("طول هش بیش از حد مجاز است.");
        }
    }
}
