using AdoExport.Api.Formatters;

namespace AdoExport.Api.Tests.Formatters;

[TestFixture]
public class DateTimeFormatExtensionsTests
{
    private static IEnumerable<TestCaseData> FormatForFrontendTestCases()
    {
        yield return new TestCaseData(new DateTime(2021, 02, 03, 04, 05, 06))
            .Returns("2021-02-03 04:05:06Z");

        yield return new TestCaseData(new DateTime(2025, 08, 08, 08, 56, 31))
            .Returns("2025-08-08 08:56:31Z");
    }

    [TestCaseSource(nameof(FormatForFrontendTestCases))]
    public string FormatForFrontend_FormatsDateTimeAsExpected(DateTime dateTime)
    {
        return dateTime.FormatForFrontend();
    }
}