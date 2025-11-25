using FluentValidation;

namespace Keymaker.Service.Configuration.Certificate;

public sealed record CertificateConfiguration
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
}

internal sealed class CertificateConfigurationValidator : AbstractValidator<CertificateConfiguration>
{
    public CertificateConfigurationValidator()
    {
        RuleFor(r => r.Contact).NotEmpty();
        RuleFor(r => r.Domain).NotEmpty();
        RuleFor(r => r.CertificateName).NotEmpty();
        RuleFor(r => r.Password).NotEmpty();
        RuleFor(r => r.CountryName).NotEmpty();
        RuleFor(r => r.Organization).NotEmpty();
    }
}