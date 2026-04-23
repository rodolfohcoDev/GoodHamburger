namespace GoodHamburger.Application.Dtos;

public record RuleEvaluation(
    Guid RuleId,
    string RuleName,
    int Priority,
    bool Matched,
    string Reason);

public record SimulationResult(
    decimal Subtotal,
    decimal AppliedPercent,
    decimal DiscountAmount,
    decimal Total,
    Guid? AppliedRuleId,
    string? AppliedRuleName,
    IReadOnlyList<RuleEvaluation> Evaluations);
