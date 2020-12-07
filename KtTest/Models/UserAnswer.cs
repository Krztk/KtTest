using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Models
{
    public abstract class UserAnswer
    {
        public int ScheduledTestId { get; private set; }
        public int QuestionId { get; private set; }
        public int UserId { get; private set; }

        public UserAnswer(int scheduledTestId, int questionId, int userId)
        {
            ScheduledTestId = scheduledTestId;
            QuestionId = questionId;
            UserId = userId;
        }

        protected UserAnswer()
        {
        }
    }
}
