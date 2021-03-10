using System.Collections.Generic;

namespace KtTest.IntegrationTests.ApiResponses
{
    public class Paginated<T>
    {
        public bool LastPage { get; set; }
        public IEnumerable<T> Data { get; set; }
    }
}
