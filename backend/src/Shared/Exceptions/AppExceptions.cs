namespace MoleculeByMakeover.Shared.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) was not found.") { }

    public NotFoundException(string message) : base(message) { }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}

public class ForbiddenAccessException : AppException
{
    public ForbiddenAccessException(string message = "You do not have permission to perform this action.")
        : base(message) { }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message) { }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Invalid credentials.") : base(message) { }
}
