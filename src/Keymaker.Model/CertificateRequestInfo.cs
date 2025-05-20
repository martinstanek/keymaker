namespace Keymaker.Model;

public sealed record CertificateRequestInfo
{
    public required DateTime Requested { get; init; }

    public required DateTime ValidUntil { get; init; }

    public required uint PerformedChecks { get; init; }

    public required string DnsRecordValue { get; init; }

    public required CertificateRequestStatus Status { get; init; }

    public required CertificateRequestChallengeType Challenge { get; init; }
}