using AdoExport.Api.Services;

namespace AdoExport.Api.Tests.Services;

public class ScopedAuditKeyBearerTests
{
    [Test]
    public void ThrowsWhenAuditKeyWasNotSet()
    {
        var sut = new ScopedAuditKeyBearer();

        Assert.Throws<InvalidOperationException>(() => _ = sut.AuditKey);
    }

    [TestFixture]
    public class ScopedAsync
    {
        [Test]
        public void GuardsAgainstNullAuditKey()
        {
            var sut = new ScopedAuditKeyBearer();

            Assert.ThrowsAsync<ArgumentNullException>(() => sut.ScopedAsync(null!, () => Task.CompletedTask));
        }

        [TestCase("")]
        [TestCase(" ")]
        public void GuardsAgainstInvalidAuditKey(string auditKey)
        {
            var sut = new ScopedAuditKeyBearer();

            Assert.ThrowsAsync<ArgumentException>(() => sut.ScopedAsync(auditKey, () => Task.CompletedTask));
        }

        [Test]
        public void GuardsAgainstNullScopedAction()
        {
            var auditKey = Guid.NewGuid().ToString();

            var sut = new ScopedAuditKeyBearer();

            Assert.ThrowsAsync<ArgumentNullException>(() => sut.ScopedAsync(auditKey, null!));
        }

        [Test]
        public async Task SetsAuditKeyForDurationOfScope()
        {
            var auditKey = Guid.NewGuid().ToString();

            var sut = new ScopedAuditKeyBearer();

            await sut.ScopedAsync(
                auditKey,
                () =>
                {
                    sut.AuditKey.Should().Be(auditKey);
                    return Task.CompletedTask;
                });

            Assert.Throws<InvalidOperationException>(() => _ = sut.AuditKey);
        }
    }

    [TestFixture]
    public class GenericScopedAsync
    {
        [Test]
        public void GuardsAgainstNullAuditKey()
        {
            var sut = new ScopedAuditKeyBearer();

            Assert.ThrowsAsync<ArgumentNullException>(() => sut.ScopedAsync(null!, () => Task.FromResult(true)));
        }

        [TestCase("")]
        [TestCase(" ")]
        public void GuardsAgainstInvalidAuditKey(string auditKey)
        {
            var sut = new ScopedAuditKeyBearer();

            Assert.ThrowsAsync<ArgumentException>(() => sut.ScopedAsync(auditKey, () => Task.FromResult(true)));
        }

        [Test]
        public void GuardsAgainstNullScopedAction()
        {
            var auditKey = Guid.NewGuid().ToString();

            var sut = new ScopedAuditKeyBearer();

            Assert.ThrowsAsync<ArgumentNullException>(() => sut.ScopedAsync(auditKey, null!));
        }

        [Test]
        public async Task SetsAuditKeyForDurationOfScope()
        {
            var auditKey = Guid.NewGuid().ToString();

            var sut = new ScopedAuditKeyBearer();

            var result = await sut.ScopedAsync(
                auditKey,
                () =>
                {
                    sut.AuditKey.Should().Be(auditKey);
                    return Task.FromResult(true);
                });
            result.Should().BeTrue();

            Assert.Throws<InvalidOperationException>(() => _ = sut.AuditKey);
        }
    }
}