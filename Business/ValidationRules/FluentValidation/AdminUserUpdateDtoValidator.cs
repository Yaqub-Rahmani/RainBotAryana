using Entities.Dtos.AdminUsers;
using FluentValidation;
using System.Linq;

namespace Business.ValidationRules.FluentValidation
{
    public class AdminUserUpdateDtoValidator : AbstractValidator<AdminUserUpdateDto>
    {
        private static readonly string[] AllowedRoles = { "superadmin", "admin", "accountant", "viewer" };

        public AdminUserUpdateDtoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("نام ضروری است.");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("تخلص ضروری است.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("ایمیل ضروری است.")
                                 .EmailAddress().WithMessage("ایمیل معتبر نیست.");

            RuleFor(x => x.Password)
                .MinimumLength(6).When(x => !string.IsNullOrWhiteSpace(x.Password))
                .WithMessage("پسورد حداقل 6 کاراکتر باشد.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(32).WithMessage("شماره بیش از حد طولانی است.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("نقش ضروری است.")
                .Must(r => AllowedRoles.Contains((r ?? "").Trim().ToLower()))
                .WithMessage("نقش معتبر نیست.");
        }
    }
}
