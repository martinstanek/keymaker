using System;

namespace Keymaker.Model;

public sealed record ChallengeStatus
{
    public required uint PerformedChecks { get; init; }

    public required string DnsRecordValue { get; init; }

    public required DateTime Requested { get; init; }

    public required CertificateRequestStatus Status { get; init; }

    public required CertificateRequestChallengeType Challenge { get; init; }

    public static ChallengeStatus Empty => new()
    {
        PerformedChecks = 0,
        DnsRecordValue = string.Empty,
        Requested = DateTime.MinValue,
        Status = CertificateRequestStatus.NotRequested,
        Challenge = CertificateRequestChallengeType.Http
    };
}