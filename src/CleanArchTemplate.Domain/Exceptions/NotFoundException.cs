namespace CleanArchTemplate.Domain.Exceptions;

public class NotFoundException : Exception
{
    public const string Code = "RES_001";

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.") { }
}
