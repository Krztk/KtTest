using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Dtos.Test
{
    public class TestDto
    {
        public string Name { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }
}
