using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Dtos.Test
{
    public class TeacherTestResultsDto
    {
        public int TestId { get; set; }
        public bool TestFinished { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; }
    }

    public class QuestionResultDto
    {
        public int QuestionId { get;  set; }
        public int NumberOfValidAnswers { get; set; }
        public int TotalNumberOfAnswers { get; set; }
    }
}
