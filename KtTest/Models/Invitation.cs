using System;

namespace KtTest.Models
{
    public class Invitation
    {
        public int Id { get; private set; }
        public string Email { get; private set; }
        public bool IsTeacher { get; private set; }
        public string Code { get; private set; }
        public int InvitedBy { get; private set; }
        public DateTime Date { get; private set; }

        public Invitation(string email, bool isTeacher, string code, int invitedBy, DateTime date)
        {
            Email = email;
            IsTeacher = isTeacher;
            Code = code;
            InvitedBy = invitedBy;
            Date = date;
        }

        public Invitation(int id, string email, bool isTeacher, string code, int invitedBy, DateTime date)
            : this(email, isTeacher, code, invitedBy, date)
        {
            Id = id;
        }

        private Invitation()
        {

        }
    }
}
