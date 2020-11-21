using System.Collections.Generic;

namespace KtTest.Models
{
    public class TestResults
    {
        public int ScheduledTestId { get; set; }
        public bool TestFinished { get; set; }
        public List<QuestionResult> QuestionResults { get; set; }

        public TestResults(int scheduledTestId, bool testFinished, List<QuestionResult> questionResults)
        {
            ScheduledTestId = scheduledTestId;
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
