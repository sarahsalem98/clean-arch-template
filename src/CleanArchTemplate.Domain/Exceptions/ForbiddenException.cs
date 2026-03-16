namespace CleanArchTemplate.Domain.Exceptions;

public class ForbiddenException : Exception
{
    public const string Code = "RES_003";

    public ForbiddenException(string message = "Access to this resource is denied.") : base(message) { }
}
