using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Keymaker.Service.Expiration;

public sealed class CheckerBackgroundService : BackgroundService
{
    private readonly IRenewalChecker _renewalChecker;
    private readonly ILogger<CheckerBackgroundService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(10);

    public CheckerBackgroundService(IRenewalChecker renewalChecker, ILogger<CheckerBackgroundService> logger)
    {
        _renewalChecker = renewalChecker;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var nextChallenge = await _renewalChecker.ShouldTriggerChallengeAsync();

            _logger.LogInformation($"Should be the challenge triggered: {nextChallenge}");
        }
    }
}