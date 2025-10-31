using System;

namespace Keymaker.Service.Store;

public sealed record CertificatePersistenceInfo
{
    public required string Domain { get; init; }

    public required string Issuer { get; init; }

    public required string FullChainPem { get; init; }

    public required string PrivateKeyPem { get; init; }

    public required string Base64FullChainPem { get; init; }

    public required string Base64PrivateKeyPem { get; init; }

    public required string Base64Pfx { get; init; }

    public required string Password { get; init; }

    public required DateTime Obtained { get; init; }

    public required DateTime Expiry { get; init; }
}