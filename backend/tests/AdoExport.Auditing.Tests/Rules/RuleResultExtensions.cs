using AdoExport.Auditing.Rules;

namespace AdoExport.Auditing.Tests.Rules;

public static class RuleResultExtensions
{
    public static void AssertRuleResult<TReason>(this RuleResult result, Categories expectedCategory, Severities expectedSeverity)
    {
        Guard.IsNotNull(result);

        result.AssertRuleResult(typeof(TReason), expectedCategory, expectedSeverity);
    }

    public static void AssertRuleResult(this RuleResult result, Type expectedType, Categories expectedCategory, Severities expectedSeverity)
    {
        Guard.IsNotNull(result);
        Guard.IsNotNull(expectedType);

        result.Reason.GetType().Should().Be(expectedType);
        result.Reason.Category.Should().Be(expectedCategory);
        result.Reason.Severity.Should().Be(expectedSeverity);
    }
}