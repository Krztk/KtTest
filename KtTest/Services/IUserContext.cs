using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Services
{
    public interface IUserContext
    {
        int UserId { get; }
        bool IsTeacher { get; }
        bool IsOwner { get; }
    }
}
