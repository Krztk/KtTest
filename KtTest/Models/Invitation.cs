using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Models
{
    public class Invitation
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsTeacher { get; set; }
        public string Code { get; set; }
        public int InvitedBy { get; set; }
        public DateTime Date { get; set; }
    }
}
