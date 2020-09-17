using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Models
{
    public class TestResults
    {
        public int TestId { get; set; }
        public bool TestFinished { get; set; }
        public List<QuestionResult> QuestionResults { get; set; }

        public TestResults(int testId, bool testFinished, List<QuestionResult> questionResults)
        {
            TestId = testId;
            TestFinished = testFinished;
            QuestionResults = questionResults;
        }
    }

    public class QuestionResult
    {
        public int QuestionId { get; private set; }
        public int NumberOfValidAnswers { get; set; }
        public int TotalNumberOfAnswers { get; set; }

        public QuestionResult(int questionId,
            int numberOfValidAnswers,
            int totalNumberOfAnswers)
        {
            QuestionId = questionId;
            NumberOfValidAnswers = numberOfValidAnswers;
            TotalNumberOfAnswers = totalNumberOfAnswers;
        }
    }
}
