namespace Keymaker.Service.Model;

public sealed record CertificateParameters
{
    public required string Contact { get; init; } = string.Empty;

    public required string Domain { get; init; } = string.Empty;

    public required string CertificateName { get; init; } = string.Empty;

    public required string Password { get; init; } = string.Empty;

    public required string CountryName { get; init; } = string.Empty;

    public required string State { get; init; } = string.Empty;

    public required string Locality { get; init; } = string.Empty;

    public required string Organization { get; init; } = string.Empty;

    public required string OrganizationUnit { get; init; } = string.Empty;

    public static CertificateParameters Empty => new()
    {
        Contact = string.Empty,
        Domain = string.Empty,
        CertificateName = string.Empty,
        Password = string.Empty,
        CountryName = string.Empty,
        State = string.Empty,
        Locality = string.Empty,
        Organization = string.Empty,
        OrganizationUnit = string.Empty
    };
}