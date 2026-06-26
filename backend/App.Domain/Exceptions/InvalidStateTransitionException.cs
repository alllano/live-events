namespace App.Domain.Exceptions;

public class InvalidStateTransitionException : AppException
{
    public InvalidStateTransitionException(string message) : base(message, 409)
    {
    }
}
