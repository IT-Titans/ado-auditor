using AdoExport.Common.Errors;

using FluentResults;

namespace AdoExport.Common.Tests.Errors;

public static class ErrorExtensionsTests
{
    [TestFixture]
    public class RequestWasCanceledExtensionTests
    {
        [Test]
        public void RequestWasCanceled_WhenResultIsNull_ThrowsArgumentNullException()
        {
            Result nullResult = null!;
            Assert.Throws<ArgumentNullException>(() => nullResult.RequestWasCanceled());
        }

        private static IEnumerable<TestCaseData> RequestWasCanceledTestCases()
        {
            yield return new TestCaseData(Result.Ok())
                .Returns(false)
                .SetName("Returns false when result is ok.");

            yield return new TestCaseData(Result.Fail(new ExceptionalError(new Exception())))
                .Returns(false)
                .SetName("Returns false when result has no request canceled error.");

            yield return new TestCaseData(Result.Fail(new RequestWasCanceledError()))
                .Returns(true)
                .SetName("Returns true when result has request canceled error.");
        }

        [TestCaseSource(nameof(RequestWasCanceledTestCases))]
        public bool RequestWasCanceled_ReturnsExpectedResult(Result result)
        {
            return result.RequestWasCanceled();
        }
    }
}