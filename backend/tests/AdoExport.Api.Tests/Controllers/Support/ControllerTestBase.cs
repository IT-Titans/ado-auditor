using System.Net.Http.Json;
using System.Text.Json;

namespace AdoExport.Api.Tests.Controllers.Support;

/// <summary>
/// Gives every fixture its own in-memory host and a helper to read JSON responses.
/// </summary>
public abstract class ControllerTestBase
{
    protected ControllerApiFactory Factory { get; private set; } = null!;

    [OneTimeSetUp]
    public void CreateFactory()
    {
        Factory = new ControllerApiFactory();

        // Start the host once, up front. Tests in a fixture run in parallel, and the first
        // ones would otherwise race each other while it is being built.
        _ = Factory.Services;
    }

    [OneTimeTearDown]
    public void DisposeFactory() => Factory.Dispose();

    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<JsonElement>();
}
