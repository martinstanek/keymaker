namespace Keymaker.Service.Integrations;

public sealed record WebHookPayload
{
    public required string FullChain { get; init; }

    public required string PrivateKey { get; init; }
}