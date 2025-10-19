using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Keymaker.Service;

namespace Keymaker.Api.Handlers;

public sealed class RequestHandler
{
    private readonly IKeymakerService _keymakerService;

    public RequestHandler(IKeymakerService keymakerService)
    {
        _keymakerService = keymakerService;
    }

    public IResult TriggerChallengeAsync()
    {
        var triggered = _keymakerService.RequestCertificate(CancellationToken.None);

        return triggered
            ? Results.NoContent()
            : Results.Problem("Rejected", "", StatusCodes.Status429TooManyRequests);
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

    public async Task<IResult> GetMostRecentCertificateInfoAsync()
    {
        var cert = await _keymakerService.GetMostRecentCertificateInfoAsync();

        return Results.Ok(cert);
    }
}