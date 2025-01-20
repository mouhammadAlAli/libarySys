namespace Domain.Entities;

public class Borrowing : BaseEntity
{
    public int BookId { get; set; }
    public int UserId { get; set; }
    public DateTime BorrowedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }

}