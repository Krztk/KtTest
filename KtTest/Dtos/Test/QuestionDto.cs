using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Dtos.Test
{
    public abstract class QuestionDto
    {
        public int Id { get; set; }
        public string Question { get; set; }
    }

    public class QuestionWithWrittenAnswerDto : QuestionDto
    {
    }

    public class QuestionWithChoiceAnswersDto : QuestionDto
    {
        public ChoiceAnswerType ChoiceAnswerType { get; set; }
        public List<string> Choices { get; set; }
    }
}
