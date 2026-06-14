namespace WorkPac.Recruitment.Shared.Exceptions;

public abstract class DomainException(string message) : Exception(message);

public class NotFoundException(string resourceType, Guid id)
    : DomainException($"{resourceType} with id '{id}' was not found.")
{
    public string ResourceType { get; } = resourceType;
    public Guid ResourceId { get; } = id;
}

public class ConflictException(string message) : DomainException(message);

public class ValidationException(string message, Dictionary<string, string[]>? errors = null)
    : DomainException(message)
{
    public Dictionary<string, string[]>? Errors { get; } = errors;
}

public class ForbiddenException(string message) : DomainException(message);

public class InvalidStatusTransitionException(string from, string to)
    : DomainException($"Cannot transition from '{from}' to '{to}'.")
{
    public string FromStatus { get; } = from;
    public string ToStatus { get; } = to;
}
