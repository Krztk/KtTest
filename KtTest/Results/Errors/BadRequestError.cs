using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Results.Errors
{
    public class BadRequestError : ErrorBase
    {
        public BadRequestError(string error = "") : base(error)
        {
        }
    }
}
