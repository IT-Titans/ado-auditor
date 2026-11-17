using System.Text;

using AdoExport.Client.AdoApi;

using Microsoft.Extensions.Logging.Abstractions;

namespace AdoExport.Client.Tests.Support;

internal static class AdoApiClientFactory
{
    public const string Organization = "contoso";

    public const string Pat = "s3cret-pat-value";

    public static AdoApiClient Create(HttpMessageHandler handler, string organization = Organization, string pat = Pat)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler, disposeHandler: false));

        var config = new Mock<IClientConfigProvider>();
        config
            .Setup(c => c.GetConfigAsync())
            .ReturnsAsync(new ClientConfig { OrganizationName = organization, Pat = pat });

        return new AdoApiClient(NullLogger<AdoApiClient>.Instance, factory.Object, config.Object);
    }

    public static string Encode(string payload, string prefix = "aad")
        => prefix + "." + Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)).TrimEnd('=');

    public static string UserJson(string descriptor, string metaType = "member")
        => $$"""{"subjectKind":"user","metaType":"{{metaType}}","descriptor":"{{descriptor}}","displayName":"User {{descriptor}}"}""";

    public static string GroupJson(string descriptor)
        => $$"""{"subjectKind":"group","descriptor":"{{descriptor}}","displayName":"Group {{descriptor}}"}""";
}
