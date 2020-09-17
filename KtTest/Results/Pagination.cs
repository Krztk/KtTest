namespace KtTest.Results
{
    public class Pagination
    {
        public int Offset { get; set; }
        public int Limit { get; set; }

        public Pagination(int offset, int limit)
        {
            Offset = offset;
            Limit = limit;
        }

        public Pagination()
        {

        }
    }
}
