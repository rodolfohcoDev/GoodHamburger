using FluentValidation;
using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.DiscountRules.Commands;
using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Application.Validators;

public class CreateDiscountRuleCommandValidator : AbstractValidator<CreateDiscountRuleCommand>
{
    public CreateDiscountRuleCommandValidator(IMenuRepository menuRepository)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Percent).InclusiveBetween(0.01m, 100m);
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(1);
        RuleFor(x => x.MatchMode).IsInEnum();

        When(x => x.MatchMode == DiscountMatchMode.SpecificItems, () =>
        {
            RuleFor(x => x.RequiredMenuItemIds)
                .NotNull().WithMessage("SpecificItems requer ao menos um ID de item.")
                .Must(ids => ids!.Any()).WithMessage("SpecificItems requer ao menos um ID de item.")
                .Must(ids => ids!.Distinct().Count() == ids!.Count).WithMessage("IDs de itens duplicados.")
                .MustAsync(async (ids, ct) => await menuRepository.AllExistAsync(ids!, ct))
                    .WithMessage("Um ou mais IDs de itens não existem no cardápio.");
        });

        When(x => x.MatchMode != DiscountMatchMode.SpecificItems, () =>
        {
            RuleFor(x => x).Must(x => x.RequiresSandwich || x.RequiresFries || x.RequiresDrink)
                .WithMessage("Ao menos uma categoria deve ser marcada para o modo selecionado.");
        });

        When(x => x.MinimumSubtotal.HasValue, () =>
        {
            RuleFor(x => x.MinimumSubtotal!.Value).GreaterThan(0m);
        });

        When(x => x.ValidFrom.HasValue && x.ValidUntil.HasValue, () =>
        {
            RuleFor(x => x).Must(x => x.ValidFrom!.Value < x.ValidUntil!.Value)
                .WithMessage("ValidFrom deve ser anterior a ValidUntil.");
        });
    }
}
