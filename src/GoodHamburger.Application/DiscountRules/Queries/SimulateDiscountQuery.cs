using FluentValidation;
using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using Mediator;

namespace GoodHamburger.Application.DiscountRules.Queries;

public sealed record SimulateDiscountQuery(
    IReadOnlyCollection<Guid> MenuItemIds,
    DateTime? AtUtc = null) : IRequest<SimulationResult>;

public sealed class SimulateDiscountQueryHandler : IRequestHandler<SimulateDiscountQuery, SimulationResult>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IMenuRepository _menuRepository;

    public SimulateDiscountQueryHandler(IDiscountRepository discountRepository, IMenuRepository menuRepository)
    {
        _discountRepository = discountRepository;
        _menuRepository = menuRepository;
    }

    public async ValueTask<SimulationResult> Handle(SimulateDiscountQuery request, CancellationToken cancellationToken)
    {
        var menuItems = await _menuRepository.GetByIdsAsync(request.MenuItemIds, cancellationToken);

        if (menuItems.Count != request.MenuItemIds.Count)
            throw new ValidationException([new FluentValidation.Results.ValidationFailure(
                "MenuItemIds", "Um ou mais itens do cardápio não foram encontrados.")]);

        var items = Order.BuildSimulationItems(menuItems);
        var subtotal = items.Sum(i => i.UnitPrice);
        var nowUtc = request.AtUtc ?? DateTime.UtcNow;

        var allRules = await _discountRepository.GetAllAsync(cancellationToken);
        var sortedRules = allRules.OrderBy(r => r.Priority).ToList();

        Guid? appliedRuleId = null;
        string? appliedRuleName = null;
        decimal appliedPercent = 0m;
        var evaluations = new List<RuleEvaluation>();

        foreach (var rule in sortedRules)
        {
            string reason;
            bool matched;

            if (!rule.IsActive)
            {
                reason = "Inativa";
                matched = false;
            }
            else if (rule.ValidFrom.HasValue && nowUtc < rule.ValidFrom.Value)
            {
                reason = "Fora da janela de validade";
                matched = false;
            }
            else if (rule.ValidUntil.HasValue && nowUtc >= rule.ValidUntil.Value)
            {
                reason = "Fora da janela de validade";
                matched = false;
            }
            else if (rule.MinimumSubtotal.HasValue && subtotal < rule.MinimumSubtotal.Value)
            {
                reason = "Subtotal abaixo do mínimo";
                matched = false;
            }
            else if (rule.Matches(items, subtotal, nowUtc))
            {
                reason = "Correspondência encontrada";
                matched = true;
            }
            else
            {
                reason = rule.MatchMode == DiscountMatchMode.SpecificItems
                    ? "Itens específicos não conferem"
                    : "Categorias não conferem";
                matched = false;
            }

            evaluations.Add(new RuleEvaluation(rule.Id, rule.Name, rule.Priority, matched, reason));

            if (matched && appliedRuleId is null)
            {
                appliedRuleId = rule.Id;
                appliedRuleName = rule.Name;
                appliedPercent = rule.Percent;
            }
        }

        var discountAmount = Math.Round(subtotal * (appliedPercent / 100), 2, MidpointRounding.AwayFromZero);
        return new SimulationResult(
            subtotal, appliedPercent, discountAmount,
            subtotal - discountAmount,
            appliedRuleId, appliedRuleName,
            evaluations);
    }
}
