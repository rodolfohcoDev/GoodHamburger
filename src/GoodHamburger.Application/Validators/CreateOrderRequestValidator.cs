using FluentValidation;
using GoodHamburger.Application.Dtos;

namespace GoodHamburger.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.MenuItemIds)
            .NotEmpty().WithMessage("A lista de itens não pode ser vazia.")
            .Must(ids => ids.Count <= 3).WithMessage("O pedido pode ter no máximo 3 itens.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Não é permitido IDs duplicados na lista.");
    }
}
