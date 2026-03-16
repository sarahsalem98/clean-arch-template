namespace CleanArchTemplate.Domain.Exceptions;

public class DomainValidationException : Exception
{
    public const string Code = "VALIDATION_ERROR";
    public IDictionary<string, string[]> Errors { get; }

    public DomainValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }

    public DomainValidationException(string field, string error)
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}
