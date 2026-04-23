using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Exceptions;
using Mediator;

namespace GoodHamburger.Application.DiscountRules.Queries;

public sealed record GetDiscountRulesQuery : IRequest<IReadOnlyList<DiscountRuleDto>>;

public sealed class GetDiscountRulesQueryHandler : IRequestHandler<GetDiscountRulesQuery, IReadOnlyList<DiscountRuleDto>>
{
    private readonly IDiscountRepository _discountRepository;

    public GetDiscountRulesQueryHandler(IDiscountRepository discountRepository)
        => _discountRepository = discountRepository;

    public async ValueTask<IReadOnlyList<DiscountRuleDto>> Handle(
        GetDiscountRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _discountRepository.GetAllAsync(cancellationToken);
        return rules.Select(r => r.ToDto()).ToList();
    }
}

public sealed record GetDiscountRuleByIdQuery(Guid Id) : IRequest<DiscountRuleDto>;

public sealed class GetDiscountRuleByIdQueryHandler : IRequestHandler<GetDiscountRuleByIdQuery, DiscountRuleDto>
{
    private readonly IDiscountRepository _discountRepository;

    public GetDiscountRuleByIdQueryHandler(IDiscountRepository discountRepository)
        => _discountRepository = discountRepository;

    public async ValueTask<DiscountRuleDto> Handle(
        GetDiscountRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var rule = await _discountRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidDiscountRuleException("Regra de desconto não encontrada.", isNotFound: true);

        return rule.ToDto();
    }
}
