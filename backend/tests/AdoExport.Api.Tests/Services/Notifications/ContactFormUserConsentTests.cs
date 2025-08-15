using System.Text.Json;
using System.Text.Json.Serialization;

using AdoExport.Api.Services.Notifications;

namespace AdoExport.Api.Tests.Services.Notifications;

public class ContactFormUserConsentTests
{
    [Test]
    public void SerializesAsExpected()
    {
        var sut = new ContactFormUserConsent()
        {
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
            .Be("{\"consentOrigin\":\"contactForm\",\"userIdentifier\":\"my@mail.com\",\"consentTimestamp\":\"2025-08-07T07:07:33.2043474Z\"}");
    }
}