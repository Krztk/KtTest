using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Dtos.Test
{
    public class SendTestAnswersDto
    {
        public List<QuestionAnswerDto> Answers { get; set; }
    }

    public class SendTestAnswersDtoValidator : AbstractValidator<SendTestAnswersDto>
    {
        public SendTestAnswersDtoValidator()
        {
            RuleForEach(x => x.Answers).SetInheritanceValidator(v =>
            {
                v.Add(new WrittenAnswerDtoValidator());
                v.Add(new ChoiceAnswerDtoValidator());
            });
        }
    }
}
