using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Models
{
    public class TestItem
    {
        public Question Question { get; set; }
        public int QuestionId { get; set; }
        public Test Test { get; set; }
        public int TestId { get; set; }
    }
}
