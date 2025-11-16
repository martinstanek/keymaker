using System.Threading;
using System.Threading.Tasks;
using Keymaker.Api.Logging.Store;
using Microsoft.AspNetCore.Http;
using Keymaker.Service;

namespace Keymaker.Api.Handlers;

public sealed class RequestHandler
{
    private readonly IKeymakerService _keymakerService;
    private readonly IInMemoryLoggerStore _loggerStore;

    public RequestHandler(IKeymakerService keymakerService, IInMemoryLoggerStore loggerStore)
    {
        _keymakerService = keymakerService;
        _loggerStore = loggerStore;
    }

    public async Task<IResult> GetChallengeInfoAsync()
    {
        var info = await _keymakerService.GetChallengeInfoAsync();

        return Results.Ok(info);
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

    public IResult GetConsole()
    {
        var messages = _loggerStore.GetAndJoinMessages();

        return Results.Text(messages);
    }

    public IResult ClearConsole()
    {
        _loggerStore.Clear();

        return Results.NoContent();
    }
}