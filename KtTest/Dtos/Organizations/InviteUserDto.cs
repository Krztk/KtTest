using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Dtos.Organizations
{
    public class InviteUserDto
    {
        public string Email { get; set; }
        public bool IsTeacher { get; set; }
    }

    public class InviteUserDtoValidator : AbstractValidator<InviteUserDto>
    {
        public InviteUserDtoValidator()
        {
            RuleFor(x => x.Email).EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
        }
    }
}
