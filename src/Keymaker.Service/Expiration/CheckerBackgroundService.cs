using System;
using System.Threading;
using System.Threading.Tasks;
using Keymaker.Service.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Keymaker.Service.Expiration;

public sealed class CheckerBackgroundService : BackgroundService
{
    private readonly IKeymakerService _keymakerService;
    private readonly IRenewalChecker _renewalChecker;
    private readonly ILogger<CheckerBackgroundService> _logger;
    private readonly TimeSpan _period;

    public CheckerBackgroundService(
        IKeymakerService keymakerService,
        IRenewalChecker renewalChecker,
        KeyMakerConfiguration configuration,
        ILogger<CheckerBackgroundService> logger)
    {
        _keymakerService = keymakerService;
        _renewalChecker = renewalChecker;
        _logger = logger;
        _period = TimeSpan.FromMinutes(configuration.CheckForExpirationEveryMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested)
        {
            var nextChallenge = await _renewalChecker.ShouldTriggerChallengeAsync();

            _logger.LogInformation($"Next challenge: {nextChallenge.NextNegotiation:yy.MM.dd HH:mm:ss}, Hours left: {nextChallenge.HoursLeft}");

            if (nextChallenge.ShouldTrigger)
            {
                _keymakerService.RequestCertificateInternal(stoppingToken);
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }
}