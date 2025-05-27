namespace Keymaker.Model;

public sealed record ChallengeParameters
{
    public required string Contact { get; init; } = string.Empty;

    public required string Domain { get; init; } = string.Empty;

    public required string CertificateName { get; init; } = string.Empty;

    public required string CountryName { get; init; } = string.Empty;

    public required string State { get; init; } = string.Empty;

    public required string Locality { get; init; } = string.Empty;

    public required string Organization { get; init; } = string.Empty;

    public required string OrganizationUnit { get; init; } = string.Empty;

    public required string DnsChallengeSetDomain { get; init; } = string.Empty;

    public required string DnsChallengeCheckDomain { get; init; } = string.Empty;
}