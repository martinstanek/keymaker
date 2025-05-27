using System;
using System.Threading;
using System.Threading.Tasks;
using Keymaker.Model;
using Keymaker.Service;
using Keymaker.Service.Dns;
using Microsoft.AspNetCore.Http;

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
        _keymakerService.RequestCertificate(CertificateRequestChallengeType.Dns, CancellationToken.None);

        return Results.NoContent();
    }

    public IResult TriggerHttpChallenge()
    {
        _keymakerService.RequestCertificate(CertificateRequestChallengeType.Dns, CancellationToken.None);

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

    public async Task<IResult> GetDnsTxtEntryAsync(string domain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        var value = await _dnsService.GetTxtEntryAsync(domain);

        return Results.Ok(value);
    }
}