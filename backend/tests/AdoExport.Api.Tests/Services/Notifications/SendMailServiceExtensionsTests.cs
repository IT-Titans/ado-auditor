using System.Text.Json;
using System.Text.Json.Serialization;

using AdoExport.Api.Services.Notifications;

namespace AdoExport.Api.Tests.Services.Notifications;

[TestFixture]
public static class SendMailServiceExtensionsTests
{
    private const string ValidSenderEmail = "sender@example.com";

    private const string ValidReceiverEmail = "receiver@example.com";
    
    [TestFixture]
    public class SendAuditMetricsMailAsync
    {
        [Test]
        public void SendAuditMetricsMailAsync_Throws_WhenSendMailServiceIsNull()
        {
            ISendMailService sut = null!;

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.SendAuditMetricsMailAsync(ValidSenderEmail, ValidReceiverEmail, CreateSampleAuditMetrics()));
        }

        [Test]
        public void SendAuditMetricsMailAsync_Throws_WhenSenderIsNull()
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendAuditMetricsMailAsync(null!, ValidReceiverEmail, CreateSampleAuditMetrics()));
        }

        [Test]
        public void SendAuditMetricsMailAsync_Throws_WhenReceiverIsNull()
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendAuditMetricsMailAsync(ValidSenderEmail, null!, CreateSampleAuditMetrics()));
        }

        [Test]
        public void SendAuditMetricsMailAsync_Throws_WhenAuditLogIsNull()
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendAuditMetricsMailAsync(ValidSenderEmail, ValidReceiverEmail, null!));
        }

        [Test]
        public async Task SendAuditMetricsMailAsync_FormatsAndSendsAuditMetricsCorrectly()
        {
            var auditMetrics = CreateSampleAuditMetrics();

            var body = JsonSerializer.Serialize(auditMetrics);

            var mock = new Mock<ISendMailService>();

            await mock
                .Object
                .SendAuditMetricsMailAsync(ValidSenderEmail, ValidReceiverEmail, auditMetrics)
                .ConfigureAwait(false);

            mock.Verify(m => m
                    .SendMailAsync(
                        ValidSenderEmail,
                        ValidReceiverEmail,
                        It.Is<string>(subject => subject.EndsWith("Audit metrics")),
                        body),
                Times.Once);
        }

        private static AuditMetrics CreateSampleAuditMetrics()
        {
            return new AuditMetrics
            {
                AuditId = Guid.NewGuid().ToString(),
            }
                .Add("my-metric", "my-data")
                .Add("my-other-metric", null);
        }
    }

    [TestFixture]
    public class SendContactMailAsync
    {
        private const string InquiryEmail = "email unknown";

        private const string InquiryName = "Doe";

        private const string InquiryPhone = "+49 ...";

        private const string InquiryText = "This is my inquiry: ...";

        [Test]
        public void SendContactMailAsync_Throws_WhenSendMailServiceIsNull()
        {
            ISendMailService sut = null!;

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.SendContactMailAsync(ValidSenderEmail, ValidReceiverEmail, CreateSampleContactInquiry()));
        }

        [Test]
        public void SendContactMailAsync_Throws_WhenSenderIsNull()
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendContactMailAsync(null!, ValidReceiverEmail, CreateSampleContactInquiry()));
        }

        [Test]
        public void SendContactMailAsync_Throws_WhenReceiverIsNull()
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendContactMailAsync(ValidSenderEmail, null!, CreateSampleContactInquiry()));
        }

        [Test]
        public void SendContactMailAsync_Throws_WhenAuditLogIsNull()
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendContactMailAsync(ValidSenderEmail, ValidReceiverEmail, null!));
        }

        [Test]
        public async Task SendContactMailAsync_FormatsAndSendsContactInquiryCorrectly()
        {
            var contactInquiry = CreateSampleContactInquiry();

            var mock = new Mock<ISendMailService>();

            await mock
                .Object
                .SendContactMailAsync(ValidSenderEmail, ValidReceiverEmail, contactInquiry)
                .ConfigureAwait(false);

            mock.Verify(m => m
                    .SendMailAsync(
                        ValidSenderEmail,
                        ValidReceiverEmail,
                        It.Is<string>(subject => subject.EndsWith("Contact Inquiry")),
                        It.Is<string>(value => value.Contains(InquiryEmail) &&
                                               value.Contains(InquiryName) &&
                                               value.Contains(InquiryPhone) &&
                                               value.Contains(InquiryText)
                        )),
                Times.Once);
        }

        private static ContactInquiry CreateSampleContactInquiry()
        {
            return new ContactInquiry
            {
                Email = InquiryEmail,
                Name = InquiryName,
                PhoneNumber = InquiryPhone,
                Text = InquiryText,
            };
        }
    }

    [TestFixture]
    public class SendFeedbackMailAsync
    {
        private const string FeedbackAuditId = "945BD36D-F21C-4762-A56F-E36C82042BC7";

        private const string FeedbackEmail = "email unknown";

        private const string FeedbackName = "Doe";

        private const string FeedbackText = "This is my inquiry: ...";

        [Test]
        public void SendFeedbackMailAsync_Throws_WhenSendMailServiceIsNull()
        {
            ISendMailService sut = null!;

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.SendFeedbackMailAsync(ValidSenderEmail, ValidReceiverEmail, CreateSampleFeedbackInquiry()));
        }

        [Test]
        public void SendFeedbackMailAsync_Throws_WhenSenderIsNull()
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendFeedbackMailAsync(null!, ValidReceiverEmail, CreateSampleFeedbackInquiry()));
        }

        [Test]
        public void SendFeedbackMailAsync_Throws_WhenReceiverIsNull()
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendFeedbackMailAsync(ValidSenderEmail, null!, CreateSampleFeedbackInquiry()));
        }

        [Test]
        public void SendFeedbackMailAsync_Throws_WhenAuditLogIsNull()
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendFeedbackMailAsync(ValidSenderEmail, ValidReceiverEmail, null!));
        }

        [Test]
        public async Task SendFeedbackMailAsync_FormatsAndSendsFeedbackCorrectly()
        {
            var feedbackInquiry = CreateSampleFeedbackInquiry();

            var mock = new Mock<ISendMailService>();

            await mock
                .Object
                .SendFeedbackMailAsync(ValidSenderEmail, ValidReceiverEmail, feedbackInquiry)
                .ConfigureAwait(false);

            mock.Verify(m => m
                    .SendMailAsync(
                        ValidSenderEmail,
                        ValidReceiverEmail,
                        It.Is<string>(subject => subject.EndsWith("Feedback")),
                        It.Is<string>(value => value.Contains(FeedbackAuditId) &&
                                               value.Contains(FeedbackEmail) &&
                                               value.Contains(FeedbackName) &&
                                               value.Contains(FeedbackText)
                        )),
                Times.Once);
        }

        private static Feedback CreateSampleFeedbackInquiry()
        {
            return new Feedback
            {
                AuditId = FeedbackAuditId,
                Email = FeedbackEmail,
                Name = FeedbackName,
                Text = FeedbackText,
            };
        }
    }

    [TestFixture]
    public class SendUserConsentMailAsync
    {
        private static IEnumerable<TestCaseData> UserConsentMailTestCases()
        {
            yield return new TestCaseData(CreateSampleStartAuditFormUserConsent());
            yield return new TestCaseData(CreateSampleFeedbackFormUserConsent());
            yield return new TestCaseData(CreateSampleContactFormUserConsent());
        }

        [TestCaseSource(nameof(UserConsentMailTestCases))]
        public void SendUserConsentMailAsync_Throws_WhenSendMailServiceIsNull(UserConsent userConsent)
        {
            ISendMailService sut = null!;

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.SendUserConsentMailAsync(ValidSenderEmail, ValidReceiverEmail, userConsent));
        }

        [TestCaseSource(nameof(UserConsentMailTestCases))]
        public void SendUserConsentMailAsync_Throws_WhenSenderIsNull(UserConsent userConsent)
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendUserConsentMailAsync(null!, ValidReceiverEmail, userConsent));
        }

        [TestCaseSource(nameof(UserConsentMailTestCases))]
        public void SendUserConsentMailAsync_Throws_WhenReceiverIsNull(UserConsent userConsent)
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendUserConsentMailAsync(ValidSenderEmail, null!, userConsent));
        }

        [Test]
        public void SendUserConsentMailAsync_Throws_WhenAuditLogIsNull()
        {
            var mock = new Mock<ISendMailService>();

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                mock.Object.SendUserConsentMailAsync<UserConsent>(ValidSenderEmail, ValidReceiverEmail, null!));
        }

        [Test]
        public async Task SendUserConsentMailAsync_FormatsAndSends_StartAuditFormUserConsent_Correctly()
        {
            var userConsent = CreateSampleStartAuditFormUserConsent();
            var expectedBody = JsonSerializer.Serialize(userConsent, new JsonSerializerOptions()
            {
                Converters =
                {
                    new JsonStringEnumConverter(),
                }
            });

            var mock = new Mock<ISendMailService>();

            await mock
                .Object
                .SendUserConsentMailAsync(ValidSenderEmail, ValidReceiverEmail, userConsent)
                .ConfigureAwait(false);

            mock.Verify(m => m
                    .SendMailAsync(
                        ValidSenderEmail,
                        ValidReceiverEmail,
                        It.Is<string>(subject => subject.EndsWith("User Consent")),
                        expectedBody),
                Times.Once);
        }

        [Test]
        public async Task SendUserConsentMailAsync_FormatsAndSends_FeedbackFormUserConsent_Correctly()
        {
            var userConsent = CreateSampleFeedbackFormUserConsent();
            var expectedBody = JsonSerializer.Serialize(userConsent, new JsonSerializerOptions()
            {
                Converters =
                {
                    new JsonStringEnumConverter(),
                }
            });

            var mock = new Mock<ISendMailService>();

            await mock
                .Object
                .SendUserConsentMailAsync(ValidSenderEmail, ValidReceiverEmail, userConsent)
                .ConfigureAwait(false);

            mock.Verify(m => m
                    .SendMailAsync(
                        ValidSenderEmail,
                        ValidReceiverEmail,
                        It.Is<string>(subject => subject.EndsWith("User Consent")),
                        expectedBody),
                Times.Once);
        }

        [Test]
        public async Task SendUserConsentMailAsync_FormatsAndSends_ContactFormUserConsent_Correctly()
        {
            var userConsent = CreateSampleContactFormUserConsent();
            var expectedBody = JsonSerializer.Serialize(userConsent, new JsonSerializerOptions()
            {
                Converters =
                {
                    new JsonStringEnumConverter(),
                }
            });

            var mock = new Mock<ISendMailService>();

            await mock
                .Object
                .SendUserConsentMailAsync(ValidSenderEmail, ValidReceiverEmail, userConsent)
                .ConfigureAwait(false);

            mock.Verify(m => m
                    .SendMailAsync(
                        ValidSenderEmail,
                        ValidReceiverEmail,
                        It.Is<string>(subject => subject.EndsWith("User Consent")),
                        expectedBody),
                Times.Once);
        }

        private static StartAuditFormUserConsent CreateSampleStartAuditFormUserConsent()
        {
            return new StartAuditFormUserConsent()
            {
                AuditId = "3C708417-B23E-4849-9476-B7F7D279BE6D",
                Identifier = "my mail",
                ConsentForAnonymousLogging = true,
                TimeOfConsent = DateTime.UtcNow,
            };
        }

        private static FeedbackFormUserConsent CreateSampleFeedbackFormUserConsent()
        {
            return new FeedbackFormUserConsent()
            {
                AuditId = "3C708417-B23E-4849-9476-B7F7D279BE6D",
                Identifier = "my mail",
                TimeOfConsent = DateTime.UtcNow,
            };
        }

        private static ContactFormUserConsent CreateSampleContactFormUserConsent()
        {
            return new ContactFormUserConsent()
            {
                Identifier = "my mail",
                TimeOfConsent = DateTime.UtcNow,
            };
        }
    }
}