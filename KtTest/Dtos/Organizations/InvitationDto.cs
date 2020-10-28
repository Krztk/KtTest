using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Dtos.Organizations
{
    public class InvitationDto
    {
        public string Email { get; set; }
        public bool IsTeacher { get; set; }
        public DateTime Date { get; set; }
    }
}
