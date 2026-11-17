using System.Net;
using System.Net.Http.Headers;
using System.Text;

using AdoExport.Api.Configuration;
using AdoExport.Api.Services.Notifications;

using Azure;
using Azure.Communication.Email;
using Azure.Core.Pipeline;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Tests.Services.Notifications;

/// <summary>
/// Runs the real SDK client against a transport that never leaves the process.
/// </summary>
/// <remarks>
/// What the SDK does with a send the service could not complete is not something to take
/// on trust: whether it throws or hands back a status decides how a lost consent record
/// surfaces. These cases pin the behaviour of the SDK version in use, so an update that
/// changes it fails here instead of in production.
/// </remarks>
public class AzureCommunicationsEmailClientTests
{
    private const string ConnectionString = "endpoint=https://example.communication.azure.com/;accesskey=dGVzdA==";

    private const string OperationId = "8540c0de-899f-5cce-acb5-3ec493af3800";

    private static AzureCommunicationsEmailClient CreateSut(FakeMailService service)
    {
        var options = new EmailClientOptions
        {
            Transport = new HttpClientTransport(service),
        };

        // A retry would repeat the request against the fake and only slow the test down.
        options.Retry.MaxRetries = 0;

        return new AzureCommunicationsEmailClient(new EmailClient(ConnectionString, options));
    }

    private static Task<EmailSendStatus> SendAsync(AzureCommunicationsEmailClient sut)
        => sut.SendAsync("DoNotReply@example.com", "consent@example.com", "User Consent", "{}");

    [Test]
    public async Task ReturnsSucceededWhenTheServiceCompletesTheSend()
    {
        var service = new FakeMailService(finalStatus: "Succeeded");

        var status = await SendAsync(CreateSut(service));

        status.Should().Be(EmailSendStatus.Succeeded);
    }

    [Test]
    public async Task HandsTheMailToTheServiceAsPlainText()
    {
        var service = new FakeMailService(finalStatus: "Succeeded");

        await SendAsync(CreateSut(service));

        service.SentBody.Should().Contain("\"senderAddress\":\"DoNotReply@example.com\"");
        service.SentBody.Should().Contain("\"address\":\"consent@example.com\"");
        service.SentBody.Should().Contain("\"subject\":\"User Consent\"");
        service.SentBody.Should().Contain("\"plainText\":\"{}\"");
    }

    [TestCase("Failed")]
    [TestCase("Canceled")]
    public async Task ThrowsWhenTheServiceFinishesTheSendWithoutSuccess(string finalStatus)
    {
        // The request itself was accepted with 202; the send failed afterwards. The SDK
        // reports that by throwing from the wait, not by returning the status.
        var service = new FakeMailService(finalStatus);

        var act = () => SendAsync(CreateSut(service));

        var thrown = (await act.Should().ThrowAsync<RequestFailedException>()).Which;

        // The HTTP status is the one of the poll that brought the bad news, so it reads
        // 200. What identifies the failure is the error code the service put in the body.
        thrown.Status.Should().Be(200);
        thrown.ErrorCode.Should().Be("EmailDropped");
    }

    [Test]
    public async Task ThrowsWhenTheServiceRejectsTheRequest()
    {
        var service = new FakeMailService(finalStatus: "Succeeded", sendStatusCode: HttpStatusCode.Unauthorized);

        var act = () => SendAsync(CreateSut(service));

        (await act.Should().ThrowAsync<RequestFailedException>()).Which.Status.Should().Be(401);
    }

    [Test]
    public void RefusesAMissingConfiguration()
    {
        var act = () => new AzureCommunicationsEmailClient((IOptions<AzureCommunicationServiceConfiguration>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void IsCreatedFromTheConfiguredConnectionString()
    {
        // Creating the client opens no connection, so this stays inside the process.
        var act = () => new AzureCommunicationsEmailClient(
            Options.Create(new AzureCommunicationServiceConfiguration { ConnectionString = ConnectionString }));

        act.Should().NotThrow();
    }

    /// <summary>
    /// Answers the two calls a send consists of: the request to send, and the poll for
    /// its outcome.
    /// </summary>
    private sealed class FakeMailService : HttpMessageHandler
    {
        private readonly string _finalStatus;

        private readonly HttpStatusCode _sendStatusCode;

        public FakeMailService(string finalStatus, HttpStatusCode sendStatusCode = HttpStatusCode.Accepted)
        {
            _finalStatus = finalStatus;
            _sendStatusCode = sendStatusCode;
        }

        public string? SentBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Post)
            {
                SentBody = await request.Content!.ReadAsStringAsync(cancellationToken);

                if (_sendStatusCode != HttpStatusCode.Accepted)
                {
                    return Json(_sendStatusCode, """{"error":{"code":"Denied","message":"The request was not accepted."}}""");
                }

                var accepted = Json(HttpStatusCode.Accepted, $$"""{"id":"{{OperationId}}","status":"Running"}""");
                accepted.Headers.Add("Operation-Location", $"https://example.communication.azure.com/emails/operations/{OperationId}?api-version=2023-03-31");

                return accepted;
            }

            var error = _finalStatus == "Succeeded"
                ? string.Empty
                : ""","error":{"code":"EmailDropped","message":"The mail could not be delivered."}""";

            return Json(HttpStatusCode.OK, $$"""{"id":"{{OperationId}}","status":"{{_finalStatus}}"{{error}}}""");
        }

        private static HttpResponseMessage Json(HttpStatusCode statusCode, string body)
        {
            // A response from the network cannot be rewound, and the SDK relies on that: it
            // only keeps a copy of a body it cannot seek in. A plain in-memory body would be
            // closed under it, which never happens against the real service.
            var content = new StreamContent(new ForwardOnlyStream(Encoding.UTF8.GetBytes(body)));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return new HttpResponseMessage(statusCode) { Content = content };
        }
    }

    private sealed class ForwardOnlyStream(byte[] bytes) : Stream
    {
        private readonly MemoryStream _inner = new(bytes);

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
