using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Results.Errors
{
    public abstract class ErrorBase
    {
        public string Error { get; }

        public ErrorBase(string error)
        {
            Error = error;
        }
    }
}
