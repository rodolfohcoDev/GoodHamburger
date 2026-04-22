namespace GoodHamburger.Domain.Exceptions;

public class InvalidOrderException : DomainException
{
    public bool IsNotFound { get; }

    public InvalidOrderException(string message, bool isNotFound = false)
        : base(message)
    {
        IsNotFound = isNotFound;
    }
}
