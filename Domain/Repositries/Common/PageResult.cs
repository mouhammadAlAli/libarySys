namespace Domain.Repositries.Common;

public class PageResult<T>
{
    public int TotalRecords { get; set; }
    public IEnumerable<T> Data { get; set; }
}
