using System;

namespace Keymaker.Service.Acme.Certificates;

public sealed record Certificate
{
    public required string Pem { get; init; }

    public required string PemKey { get; init; }

    public required string Base64 { get; init; }

    public required string Domain { get; init; }

    public required string Issuer { get; init; }

    public required DateTime Expiry { get; init; }
}