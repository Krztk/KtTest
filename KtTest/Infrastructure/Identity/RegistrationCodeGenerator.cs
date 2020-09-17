using KtTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Infrastructure.Identity
{
    public class RegistrationCodeGenerator : IRegistrationCodeGenerator
    {
        public string GenerateCode()
        {
            var guid = Guid.NewGuid();
            return guid.ToString();
        }
    }
}
