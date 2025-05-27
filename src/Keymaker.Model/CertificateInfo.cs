using System;

namespace Keymaker.Model;

public sealed record CertificateInfo
{
    public required string Domain { get; init; } = string.Empty;

    public required string FullChainPem { get; init; } = string.Empty;

    public required string PrivateKeyPem { get; init; } = string.Empty;

    public required string Base64Pfx { get; init; } = string.Empty;

    public required DateTime Obtained { get; init; }

    public required DateTime Expiry { get; init; }

    public static CertificateInfo Empty => new()
    {
        Domain = string.Empty,
        FullChainPem = string.Empty,
        PrivateKeyPem = string.Empty,
        Base64Pfx = string.Empty,
        Expiry = DateTime.MinValue,
        Obtained = DateTime.MinValue
    };
}