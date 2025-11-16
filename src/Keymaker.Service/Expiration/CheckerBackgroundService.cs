using System;
using System.Threading;
using System.Threading.Tasks;
using Keymaker.Service.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Keymaker.Service.Expiration;

public sealed class CheckerBackgroundService : BackgroundService
{
    private const int DelayFirstExecutionSeconds = 10;

    private readonly IKeymakerService _keymakerService;
    private readonly IRenewalChecker _renewalChecker;
    private readonly ILogger<CheckerBackgroundService> _logger;
    private readonly TimeSpan _period;
    private bool _deferredStart = true;

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
        if (_deferredStart)
        {
            await Task.Delay(TimeSpan.FromSeconds(DelayFirstExecutionSeconds), stoppingToken);

            _deferredStart = false;
        }

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