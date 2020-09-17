using FluentValidation;
using KtTest.Infrastructure.Data;

namespace KtTest.Dtos.Test
{
    public class QuestionAnswerDto
    {
        public int QuestionId { get; set; }
    }

    public class WrittenAnswerDto : QuestionAnswerDto
    {
        public string Text { get; set; }
    }

    public class ChoiceAnswerDto : QuestionAnswerDto
    {
        public int Value { get; set; }
    }

    public class QuestionAnswerDtoValidator<T> : AbstractValidator<T> where T : QuestionAnswerDto
    {
        public QuestionAnswerDtoValidator()
        {
            RuleFor(x => x.QuestionId).NotEmpty();
        }
    }

    public class WrittenAnswerDtoValidator : QuestionAnswerDtoValidator<WrittenAnswerDto>
    {
        public WrittenAnswerDtoValidator()
        {
            RuleFor(x => x.Text).MaximumLength(DataConstraints.Answer.MaxWrittenAnswerLength);
        }
    }

    public class ChoiceAnswerDtoValidator : QuestionAnswerDtoValidator<ChoiceAnswerDto>
    {
        public ChoiceAnswerDtoValidator()
        {
        }
    }
}
