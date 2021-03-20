using System.Collections.Generic;

namespace KtTest.Dtos.Test
{
    public class TestResultsDto
    {
        public string Name { get; set; }
        public List<QuestionWithResultDto> QuestionsWithResult { get; set; }
    }

    public abstract class QuestionWithResultDto
    {
        public int QuestionId { get; set; }
        public string Question { get; set; }
    }

    public class QuestionWithWrittenResultDto : QuestionWithResultDto
    {
        public string CorrectAnswer { get; set; }
        public string UserAnswer { get; set; }
    }

    public class QuestionWithChoiceAnswerResultDto : QuestionWithResultDto
    {
        public List<ChoiceDto> Choices { get; set; }
    }

    public class ChoiceDto
    {
        public string Value { get; set; }
        public bool Correct { get; set; }
        public bool UserAnswer { get; set; }
    }

}
