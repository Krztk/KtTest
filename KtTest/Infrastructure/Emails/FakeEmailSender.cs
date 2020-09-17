using KtTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Infrastructure.Emails
{
    public class FakeEmailSender : IEmailSender
    {
        public Task SendEmail(string email, string content)
        {
            return Task.CompletedTask;
        }
    }
}
