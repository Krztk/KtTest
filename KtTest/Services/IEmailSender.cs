using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Services
{
    public interface IEmailSender
    {
        Task SendEmail(string email, string content);
    }
}
