namespace cl_be.Models
{
  public class ErrorPagedResult<T>
  {
    public List<LogErrorActivities> Items { get; set; }
    public int TotalItems { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }

    public ErrorPagedResult(List<LogErrorActivities> items, int totalItems, int pageIndex, int pageSize)
    {
      Items = items;
      TotalItems = totalItems;
      PageIndex = pageIndex;
      PageSize = pageSize;
    }
  }
}
