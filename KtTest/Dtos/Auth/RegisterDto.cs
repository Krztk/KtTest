using FluentValidation;
using FluentValidation.Validators;
using KtTest.Infrastructure.Data;

namespace KtTest.Dtos.Auth
{
    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email).EmailAddress(EmailValidationMode.AspNetCoreCompatible);
            RuleFor(x => x.Username).NotEmpty().MaximumLength(DataConstraints.User.MaxUsernameLength);
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
