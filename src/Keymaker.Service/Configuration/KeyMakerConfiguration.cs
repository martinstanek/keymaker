using Keymaker.Model;

namespace Keymaker.Service.Configuration;

public sealed record KeyMakerConfiguration
{
    public required DnsMode DnsMode { get; init; }

    public required StorageMode StorageMode { get; init; }

    public required ChallengeMode ChallengeMode { get; init; }

    public required bool IsAutoRenewalEnabled { get; init; }

    public required int RenewEveryHours { get; init; }

    public required int CheckForExpirationEveryMinutes { get; init; }
}