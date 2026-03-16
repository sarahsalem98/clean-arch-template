namespace CleanArchTemplate.Domain.Exceptions;

public class ServiceUnavailableException : Exception
{
    public const string Code = "SRV_002";

    public ServiceUnavailableException(string message = "Service is temporarily unavailable.") : base(message) { }
}
