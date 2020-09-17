using FluentValidation;
using KtTest.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Dtos.Groups
{
    public class CreateGroupDto
    {
        public string Name { get; set; }
    }

    public class CreateGroupDtoValidator : AbstractValidator<CreateGroupDto>
    {
        public CreateGroupDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(DataConstraints.Group.MaxNameLength);
        }
    }
}
