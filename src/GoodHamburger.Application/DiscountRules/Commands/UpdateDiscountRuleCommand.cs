using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Exceptions;
using Mediator;

namespace GoodHamburger.Application.DiscountRules.Commands;

public sealed record UpdateDiscountRuleCommand(
    Guid Id,
    string Name,
    decimal Percent,
    DiscountMatchMode MatchMode,
    bool RequiresSandwich,
    bool RequiresFries,
    bool RequiresDrink,
    IReadOnlyCollection<Guid>? RequiredMenuItemIds,
    int Priority,
    bool IsActive,
    decimal? MinimumSubtotal,
    DateTime? ValidFrom,
    DateTime? ValidUntil) : IRequest<DiscountRuleDto>;

public sealed class UpdateDiscountRuleCommandHandler : IRequestHandler<UpdateDiscountRuleCommand, DiscountRuleDto>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDiscountRuleCommandHandler(IDiscountRepository discountRepository, IUnitOfWork unitOfWork)
    {
        _discountRepository = discountRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<DiscountRuleDto> Handle(UpdateDiscountRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _discountRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidDiscountRuleException("Regra de desconto não encontrada.", isNotFound: true);

        var allRules = await _discountRepository.GetAllAsync(cancellationToken);
        var otherActiveOnes = allRules.Where(r => r.IsActive && r.Id != request.Id).ToList();

        if (request.IsActive && otherActiveOnes.Any(r => r.Priority == request.Priority))
            throw new InvalidDiscountRuleException(
                $"Já existe uma regra ativa com prioridade {request.Priority}.", isConflict: true);

        // Build a temporary rule to compute fingerprint before actual update
        var tempRule = DiscountRule.Create(
            request.Name, request.Percent, request.MatchMode,
            request.RequiresSandwich, request.RequiresFries, request.RequiresDrink,
            request.RequiredMenuItemIds, request.Priority, request.IsActive,
            request.MinimumSubtotal, request.ValidFrom, request.ValidUntil);

        var newFingerprint = tempRule.Fingerprint;
        var conflicting = otherActiveOnes.FirstOrDefault(r =>
            r.IsActive &&
            r.Fingerprint == newFingerprint &&
            HasOverlappingValidity(r, request.ValidFrom, request.ValidUntil));

        if (conflicting is not null)
            throw new InvalidDiscountRuleException(
                $"Já existe uma regra ativa com a mesma combinação: '{conflicting.Name}'.", isConflict: true);

        rule.Update(
            request.Name, request.Percent, request.MatchMode,
            request.RequiresSandwich, request.RequiresFries, request.RequiresDrink,
            request.RequiredMenuItemIds, request.Priority, request.IsActive,
            request.MinimumSubtotal, request.ValidFrom, request.ValidUntil);

        _discountRepository.Update(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return rule.ToDto();
    }

    private static bool HasOverlappingValidity(DiscountRule existing, DateTime? newFrom, DateTime? newUntil)
    {
        if (existing.ValidFrom is null || existing.ValidUntil is null || newFrom is null || newUntil is null)
            return true;
        return existing.ValidFrom.Value < newUntil.Value && newFrom.Value < existing.ValidUntil.Value;
    }
}
