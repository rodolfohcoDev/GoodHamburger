namespace GoodHamburger.Domain.Exceptions;

public class DuplicateItemException : DomainException
{
    public DuplicateItemException(string category)
        : base($"Já existe um item da categoria '{category}' neste pedido.") { }
}
