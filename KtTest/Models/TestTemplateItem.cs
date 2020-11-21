using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Models
{
    public class TestTemplateItem
    {
        public Question Question { get; set; }
        public int QuestionId { get; set; }
        public TestTemplate TestTemplate { get; set; }
        public int TestTemplateId { get; set; }
    }
}
