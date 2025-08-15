using System.Text.Json;
using System.Text.Json.Serialization;

using AdoExport.Api.Services.Notifications;

namespace AdoExport.Api.Tests.Services.Notifications;

public class StartAuditFormUserConsentTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void SerializesAsExpected(bool consentForAnonymousLogging)
    {
        var sut = new StartAuditFormUserConsent()
        {
            AuditId = "0ED6C74D-01A5-44E3-AC5E-26CAA0EE8339",
            Identifier = "my@mail.com",
            TimeOfConsent = DateTime.Parse("2025-08-07T07:07:33.2043474Z").ToUniversalTime(),
            ConsentForAnonymousLogging = consentForAnonymousLogging,
        };

        JsonSerializer
            .Serialize(sut, new JsonSerializerOptions()
            {
                Converters =
                {
                    new JsonStringEnumConverter(),
                },
            })
            .Should()
            .Be($"{{\"consentOrigin\":\"startAuditForm\",\"auditIdentifier\":\"0ED6C74D-01A5-44E3-AC5E-26CAA0EE8339\",\"consentToAnonymousLogging\":{JsonSerializer.Serialize(consentForAnonymousLogging)},\"userIdentifier\":\"my@mail.com\",\"consentTimestamp\":\"2025-08-07T07:07:33.2043474Z\"}}");
    }
}