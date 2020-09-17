using System;
using System.Collections.Generic;
using System.Linq;

namespace KtTest.Results
{
    public class OperationResult
    {
        protected List<Failure> failures { get; set; }
        public IEnumerable<Failure> Failures => failures;
        public bool Succeeded => !failures.Any();
        public OperationResult()
        {
            failures = new List<Failure>();
        }

        public void AddFailure(Failure failure)
        {
            failures.Add(failure);
        }
    }

    public class OperationResult<T> : OperationResult
    {
        public T Data { get; set; }

        public OperationResult<TR> MapResult<TR>(Func<T, TR> projection)
        {
            var result = new OperationResult<TR>();
            result.failures = failures;
            if (Succeeded)
            {
                result.Data = projection(Data);
            }
            return result;
        }

        public OperationResult<TR> MapResult<TR>()
        {
            var result = new OperationResult<TR>();
            result.failures = failures;
            return result;
        }
    }
}
