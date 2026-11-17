using AdoExport.Client.AdoApi.Helpers;
using AdoExport.Client.AdoApi.Models;

using static AdoExport.Client.Tests.Support.AdoApiClientFactory;

namespace AdoExport.Client.Tests.AdoApi.Helpers;

public class DescriptorExtensionsTests
{
    // Payload lengths 1..4 produce stripped base64 with every possible amount of missing padding.
    [TestCase("a")]
    [TestCase("ab")]
    [TestCase("abc")]
    [TestCase("abcd")]
    [TestCase("S-1-9-1551374245-1204400969-2402986413-2179408616-0-0-0-0-1")]
    public void DecodeDescriptor_restores_stripped_padding(string payload)
    {
        var descriptor = Encode(payload, "aad").DecodeDescriptor();

        descriptor.Prefix.Should().Be("aad");
        descriptor.Payload.Should().Be(payload);
    }

    [Test]
    public void DecodeDescriptor_reads_prefix_before_the_dot()
    {
        var encoded = Convert.ToBase64String("x"u8.ToArray()).TrimEnd('=');

        var descriptor = $"vssgp.{encoded}".DecodeDescriptor();

        descriptor.Should().Be(new DescriptorExtensions.Descriptor("vssgp", "x"));
    }

    [Test]
    public void DecodeDescriptor_throws_for_a_descriptor_without_payload()
    {
        FluentActions.Invoking(() => "nodot".DecodeDescriptor()).Should().Throw<IndexOutOfRangeException>();
    }

    [Test]
    public void UserDto_and_GroupDto_expose_the_decoded_payload()
    {
        var descriptor = Encode("decoded-value");

        new UserDto { Descriptor = descriptor }.DecodedDescriptor.Should().Be("decoded-value");
        new GroupDto { Descriptor = descriptor }.DecodedDescriptor.Should().Be("decoded-value");
    }
}
