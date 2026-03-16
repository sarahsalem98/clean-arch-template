namespace CleanArchTemplate.Domain.Exceptions;

public class ConflictException : Exception
{
    public const string Code = "RES_002";

    public ConflictException(string message) : base(message) { }
}
