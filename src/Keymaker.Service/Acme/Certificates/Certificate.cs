namespace Keymaker.Service.Acme.Certificates;

public sealed record Certificate
{
    public required string Pem { get; init; } = string.Empty;

    public required string PemKey { get; init; } = string.Empty;

    public required string Base64 { get; init; } = string.Empty;
}