using System;

namespace Keymaker.Model;

public sealed record CertificateInfo
{
    public required string Domain { get; init; }

    public required string Issuer { get; init; }

    public required DateTime Expiry { get; init; }

    public required DateTime Obtained { get; init; }

    public static CertificateInfo Empty => new()
    {
        Domain = string.Empty,
        Issuer = string.Empty,
        Expiry = DateTime.MinValue,
        Obtained = DateTime.MinValue
    };
}