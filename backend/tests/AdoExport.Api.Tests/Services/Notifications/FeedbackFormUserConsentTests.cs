using System.Text.Json;
using System.Text.Json.Serialization;

using AdoExport.Api.Services.Notifications;

namespace AdoExport.Api.Tests.Services.Notifications;

public class FeedbackFormUserConsentTests
{
    [Test]
    public void SerializesAsExpected()
    {
        var sut = new FeedbackFormUserConsent()
        {
            AuditId = "0ED6C74D-01A5-44E3-AC5E-26CAA0EE8339",
            Identifier = "my@mail.com",
            TimeOfConsent = DateTime.Parse("2025-08-07T07:07:33.2043474Z").ToUniversalTime(),
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
            .Be("{\"consentOrigin\":\"feedbackForm\",\"auditIdentifier\":\"0ED6C74D-01A5-44E3-AC5E-26CAA0EE8339\",\"userIdentifier\":\"my@mail.com\",\"consentTimestamp\":\"2025-08-07T07:07:33.2043474Z\"}");
    }
}