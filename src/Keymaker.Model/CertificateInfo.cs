using System;

namespace Keymaker.Model;

public sealed record CertificateInfo
{
    public required string Domain { get; init; }

    public required DateTime Obtained { get; init; }

    public required DateTime Expiry { get; init; }
}