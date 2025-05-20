using System;

namespace Keymaker.Service.Model;

public sealed record CertificateInfo
{
    public required string Domain { get; init; }

    public required DateTime Obtained { get; init; }

    public required DateTime Expiry { get; init; }

    public required string FullChainPem { get; init; }

    public required string PrivateKeyPem { get; init; }

    public required string Base64Pfx { get; init; }
}