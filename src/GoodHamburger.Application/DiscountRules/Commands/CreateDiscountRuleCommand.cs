using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Exceptions;
using Mediator;

namespace GoodHamburger.Application.DiscountRules.Commands;

public sealed record CreateDiscountRuleCommand(
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

public sealed class CreateDiscountRuleCommandHandler : IRequestHandler<CreateDiscountRuleCommand, DiscountRuleDto>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDiscountRuleCommandHandler(IDiscountRepository discountRepository, IUnitOfWork unitOfWork)
    {
        _discountRepository = discountRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<DiscountRuleDto> Handle(CreateDiscountRuleCommand request, CancellationToken cancellationToken)
    {
        var activeRules = await _discountRepository.GetAllAsync(cancellationToken);
        var activeOnes = activeRules.Where(r => r.IsActive).ToList();

        if (activeOnes.Any(r => r.Priority == request.Priority))
            throw new InvalidDiscountRuleException(
                $"Já existe uma regra ativa com prioridade {request.Priority}.", isConflict: true);

        var rule = DiscountRule.Create(
            request.Name, request.Percent, request.MatchMode,
            request.RequiresSandwich, request.RequiresFries, request.RequiresDrink,
            request.RequiredMenuItemIds, request.Priority, request.IsActive,
            request.MinimumSubtotal, request.ValidFrom, request.ValidUntil);

        var newFingerprint = rule.Fingerprint;
        var conflicting = activeOnes.FirstOrDefault(r =>
            r.Fingerprint == newFingerprint && HasOverlappingValidity(r, request.ValidFrom, request.ValidUntil));

        if (conflicting is not null)
            throw new InvalidDiscountRuleException(
                $"Já existe uma regra ativa com a mesma combinação: '{conflicting.Name}'.", isConflict: true);

        await _discountRepository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return rule.ToDto();
    }

    private static bool HasOverlappingValidity(DiscountRule existing, DateTime? newFrom, DateTime? newUntil)
    {
        // Null window = permanent (always overlaps unless both have non-overlapping windows)
        if (existing.ValidFrom is null || existing.ValidUntil is null || newFrom is null || newUntil is null)
            return true;
        return existing.ValidFrom.Value < newUntil.Value && newFrom.Value < existing.ValidUntil.Value;
    }
}
