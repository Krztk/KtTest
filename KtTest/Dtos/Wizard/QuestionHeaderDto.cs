using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Dtos.Wizard
{
    public class QuestionHeaderDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int NumberOfTimesUsedInTests { get; set; }
        public string Type { get; set; }
    }
}
