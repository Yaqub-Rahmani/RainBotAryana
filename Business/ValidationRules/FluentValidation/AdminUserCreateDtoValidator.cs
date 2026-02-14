using Entities.Dtos.AdminUsers;
using FluentValidation;
using System.Linq;

namespace Business.ValidationRules.FluentValidation
{
    public class AdminUserCreateDtoValidator : AbstractValidator<AdminUserCreateDto>
    {
        private static readonly string[] AllowedRoles = { "superadmin", "admin", "accountant", "viewer" };

        public AdminUserCreateDtoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("نام ضروری است.");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("تخلص ضروری است.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("ایمیل ضروری است.")
                                 .EmailAddress().WithMessage("ایمیل معتبر نیست.");

            RuleFor(x => x.Password).NotEmpty().WithMessage("پسورد ضروری است.")
                                    .MinimumLength(6).WithMessage("پسورد حداقل 6 کاراکتر باشد.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(32).WithMessage("شماره بیش از حد طولانی است.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("نقش ضروری است.")
                .Must(r => AllowedRoles.Contains((r ?? "").Trim().ToLower()))
                .WithMessage("نقش معتبر نیست.");
        }
    }
}
