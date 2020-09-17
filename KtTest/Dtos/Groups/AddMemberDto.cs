using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Dtos.Groups
{
    public class AddMemberDto
    {
        public int UserId { get; set; }
    }

    public class AddMemberDtoValidator : AbstractValidator<AddMemberDto>
    {
        public AddMemberDtoValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
