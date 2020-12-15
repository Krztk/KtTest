using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Results.Errors
{
    public class DataNotFoundError : ErrorBase
    {
        public DataNotFoundError(string error = "") : base(error)
        {
        }
    }
}
