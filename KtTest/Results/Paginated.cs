using System;
using System.Collections.Generic;
using System.Linq;

namespace KtTest.Results
{
    public class Paginated<T>
    {
        private readonly int limit;
        public bool LastPage { get; private set; }
        public IEnumerable<T> Data { get; private set; }

        public Paginated(int limit, IEnumerable<T> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            this.limit = limit;
            LastPage = data.Count() <= limit;
            Data = data.Take(limit);
        }

        private Paginated(int limit)
        {
            this.limit = limit;
        }

        public Paginated<TR> MapResult<TR>(Func<T, TR> projection)
        {
            var result = new Paginated<TR>(limit);
            result.Data = Data.Select(projection);
            result.LastPage = LastPage;
            return result;
        }
    }
}
