namespace Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, string key) : base(string.Format("entity {0} with key {1} not found", entityName, key))
    {
    }
}
