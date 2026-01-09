namespace cl_be.Models.Pagination
{
    public class PagedResult<T>
    {
        public int TotalCount { get; set; }
        public IEnumerable<T> Items { get; set; } = null;
      
    }
}
