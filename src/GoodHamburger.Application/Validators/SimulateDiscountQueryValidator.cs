using FluentValidation;
using GoodHamburger.Application.DiscountRules.Queries;

namespace GoodHamburger.Application.Validators;

public class SimulateDiscountQueryValidator : AbstractValidator<SimulateDiscountQuery>
{
    public SimulateDiscountQueryValidator()
    {
        RuleFor(x => x.MenuItemIds)
            .NotEmpty().WithMessage("A lista de itens não pode ser vazia.")
            .Must(ids => ids.All(id => id != Guid.Empty))
                .WithMessage("IDs não podem ser vazios.");
    }
}
