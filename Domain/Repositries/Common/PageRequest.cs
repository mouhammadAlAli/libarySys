namespace Domain.Repositries.Common;

public class PageRequest
{
    public int Rows { get; set; } = 10;
    public int Page { get; set; } = 1;
}
