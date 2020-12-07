using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Dtos.Test
{
    public class ScheduledTestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TestTemplateId { get; set; }
        public int Duration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ScheduledAt { get; set; }
        public bool Ended { get; set; }
    }
}
