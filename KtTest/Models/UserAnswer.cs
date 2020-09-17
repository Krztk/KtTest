using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Models
{
    public abstract class UserAnswer
    {
        public int TestId { get; set; }
        public int QuestionId { get; set; }
        public int UserId { get; set; }
    }
}
