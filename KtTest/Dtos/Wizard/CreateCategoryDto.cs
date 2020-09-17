using FluentValidation;
using KtTest.Infrastructure.Data;

namespace KtTest.Dtos.Wizard
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }
    }

    public class CategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        public CategoryDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(DataConstraints.Category.MaxNameLength);
        }
    }
}
