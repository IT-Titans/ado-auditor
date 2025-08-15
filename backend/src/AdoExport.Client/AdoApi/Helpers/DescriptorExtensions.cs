using System.Text;

namespace AdoExport.Client.AdoApi.Helpers;

public static class DescriptorExtensions
{
    /// <summary>
    /// Decodes the given Azure DevOps <paramref name="encodedDescriptor" /> into a <see cref="Descriptor" /> instance.
    /// </summary>
    /// <param name="encodedDescriptor"></param>
    /// <returns></returns>
    public static Descriptor DecodeDescriptor(this string encodedDescriptor)
    {
        var split = encodedDescriptor.Split('.', 2);
        var prefix = split[0];
        var encodedPayload = WellFormBase64(split[1]);

        return new Descriptor(prefix, Encoding.UTF8.GetString(Convert.FromBase64String(encodedPayload)));
    }

    /// <summary>
    /// Takes a stripped or malformed base64 encoded string and tries to wellform it.<br />
    /// Note that the provided string should be a generally valid base64 encoded string.
    /// </summary>
    /// <param name="strippedBase64">The base64 encoded string to wellform.</param>
    /// <returns>A well-formed base64 encoded string.</returns>
    private static string WellFormBase64(string strippedBase64)
    {
        int padding = 4 - (strippedBase64.Length % 4);
        if (padding < 4)
        {
            return strippedBase64 + new string('=', padding);
        }

        return strippedBase64;
    }

    public record Descriptor(string Prefix, string Payload);
}