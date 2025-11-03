using System;

namespace Keymaker.Model;

public sealed record ChallengeInfo
{
    public required string Contact { get; init; }

    public required string Domain { get; init; }

    public required string CertificateName { get; init; }

    public required string Organization { get; init; }

    public required string ChallengeMode { get; init; }

    public required string DnsMode { get; init; }

    public required string StoreMode { get; init; }

    public required string StoreTarget { get; init; }

    public required string Issuer { get; init; }

    public required string Status { get; init; }

    public required string Server { get; init; }

    public required bool IsAutoRenewalEnabled { get; init; }

    public required int RenewEveryHours { get; init; }

    public required DateTime? Obtained { get; init; }

    public required DateTime? Expiry { get; init; }

    public required DateTime? NextRenewal { get; init; }

    public static ChallengeInfo Empty => new()
    {
        CertificateName = string.Empty,
        ChallengeMode = string.Empty,
        Contact = string.Empty,
        DnsMode = string.Empty,
        StoreMode = string.Empty,
        Domain = string.Empty,
        Issuer = string.Empty,
        StoreTarget = string.Empty,
        Organization = string.Empty,
        Status = string.Empty,
        Server = string.Empty,
        IsAutoRenewalEnabled = false,
        Expiry = null,
        NextRenewal = null,
        Obtained = null,
        RenewEveryHours = 0
    };
}