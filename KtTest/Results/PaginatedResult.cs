using System;
using System.Collections.Generic;

namespace KtTest.Results
{
    public class PaginatedResult<T> : OperationResult<Paginated<T>>
    {
        public PaginatedResult<TR> MapResult<TR>(Func<T, TR> projection)
        {
            var result = new PaginatedResult<TR>();
            result.Data = Data.MapResult(projection);
            return result;
        }

        public void SetResult(int limit, IEnumerable<T> data)
        {
            Data = new Paginated<T>(limit, data);
        }
    }
}
