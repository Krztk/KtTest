using FluentValidation;
using KtTest.Infrastructure.Data;
using System.Collections.Generic;

namespace KtTest.Dtos.Wizard
{
    public class CreateTestDto
    {
        public string Name { get; set; }
        public List<int> QuestionIds { get; set; }
    }

    public class CreateTestDtoValidator : AbstractValidator<CreateTestDto>
    {
        public CreateTestDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(DataConstraints.Test.MaxNameLength);
            RuleFor(x => x.QuestionIds).NotEmpty();
        }
    }
}
