using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Results.Errors
{
    public class AuthorizationError : ErrorBase
    {
        public AuthorizationError(string error = "") : base(error)
        {
        }
    }
}
