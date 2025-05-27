using Keymaker.Model;
using Keymaker.Service;
using Microsoft.AspNetCore.Http;

namespace Keymaker.Api.Handlers;

public sealed class CertificateHandler
{
    private readonly IKeymakerService _keymakerService;

    public CertificateHandler(IKeymakerService keymakerService)
    {
        _keymakerService = keymakerService;
    }

    public IResult TriggerDnsChallengeAsync()
    {
        _keymakerService.RequestCertificate(CertificateRequestChallengeType.Dns);

        return Results.NoContent();
    }

    public IResult TriggerHttpChallenge()
    {
        _keymakerService.RequestCertificate(CertificateRequestChallengeType.Dns);

        return Results.NoContent();
    }
}