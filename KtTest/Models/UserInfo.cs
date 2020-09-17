using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Models
{
    public class UserInfo
    {
        public int Id { get; private set; }
        public bool IsTeacher { get; private set; }

        public UserInfo(int id, bool isTeacher)
        {
            Id = id;
            IsTeacher = isTeacher;
        }
    }
}
