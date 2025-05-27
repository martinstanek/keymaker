namespace Keymaker.Model;

public sealed record DnsServiceConfiguration
{
    public required string Key { get; init; } = string.Empty;

    public required string Zone { get; init; } = string.Empty;

    public required string Email { get; init; } = string.Empty;

    public required string DnsChallengeSetDomain { get; init; } = string.Empty;

    public required string DnsChallengeCheckDomain { get; init; } = string.Empty;

    public static DnsServiceConfiguration Empty => new()
    {
        Zone = string.Empty,
        Email = string.Empty,
        Key = string.Empty,
        DnsChallengeCheckDomain = string.Empty,
        DnsChallengeSetDomain = string.Empty
    };
}