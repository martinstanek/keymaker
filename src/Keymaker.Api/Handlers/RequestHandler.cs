using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Keymaker.Model;
using Keymaker.Service;
using Keymaker.Service.Dns;

namespace Keymaker.Api.Handlers;

public sealed class RequestHandler
{
    private readonly IKeymakerService _keymakerService;
    private readonly IDnsService _dnsService;

    public RequestHandler(IKeymakerService keymakerService, IDnsService dnsService)
    {
        _keymakerService = keymakerService;
        _dnsService = dnsService;
    }

    public IResult TriggerDnsChallengeAsync()
    {
        var triggered = _keymakerService.RequestCertificate(CertificateRequestChallengeType.Dns, CancellationToken.None);

        return triggered
            ? Results.NoContent()
            : Results.Problem("Rejected", "", StatusCodes.Status429TooManyRequests);
    }

    public IResult TriggerHttpChallenge()
    {
        _keymakerService.RequestCertificate(CertificateRequestChallengeType.Http, CancellationToken.None);

        return Results.NoContent();
    }

    public IResult CancelCurrentChallenge()
    {
        _keymakerService.CancelCurrentChallenge();

        return Results.NoContent();
    }

    public IResult GetChallengeStatus()
    {
        var status = _keymakerService.GetCurrentRequestStatus();

        return Results.Ok(status);
    }

    public async Task<IResult> GetCertificatesAsync()
    {
        var certs = await _keymakerService.GetPersistedCertificatesAsync();

        return Results.Ok(certs);
    }

    public async Task<IResult> GetDnsTxtEntryAsync()
    {
        var value = await _dnsService.GetTxtEntryAsync();

        return Results.Ok(value);
    }
}