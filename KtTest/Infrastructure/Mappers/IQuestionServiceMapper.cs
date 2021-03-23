using KtTest.Dtos.Test;
using KtTest.Models;

namespace KtTest.Infrastructure.Mappers
{
    public interface IQuestionServiceMapper
    {
        Answer MapToAnswer(Dtos.Wizard.QuestionDto dto);
        QuestionAnswerDto MapToTestQuestionAnswerDto(UserAnswer userAnswer);
        QuestionDto MapToTestQuestionDto(Question question);
        QuestionWithResultDto MapToTestQuestionWithResultDto(Question question, UserAnswer answer);
        Dtos.Wizard.QuestionDto MapToWizardQuestionDto(Question question);
    }
}