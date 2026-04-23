namespace GoodHamburger.Domain.Exceptions;

public class InvalidDiscountRuleException : DomainException
{
    public bool IsConflict { get; }
    public bool IsNotFound { get; }

    public InvalidDiscountRuleException(string message, bool isConflict = false, bool isNotFound = false)
        : base(message)
    {
        IsConflict = isConflict;
        IsNotFound = isNotFound;
    }
}
