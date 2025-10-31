using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Keymaker.Service.Expiration;

public sealed class CheckerBackgroundService : BackgroundService
{
    private readonly IKeymakerService _keymakerService;
    private readonly IRenewalChecker _renewalChecker;
    private readonly ILogger<CheckerBackgroundService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(10);

    public CheckerBackgroundService(IKeymakerService keymakerService, IRenewalChecker renewalChecker, ILogger<CheckerBackgroundService> logger)
    {
        _keymakerService = keymakerService;
        _renewalChecker = renewalChecker;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var shouldTrigger = await _renewalChecker.ShouldTriggerChallengeAsync();

            _logger.LogDebug($"Should be the challenge triggered: {shouldTrigger}");

            if (shouldTrigger)
            {
                _keymakerService.RequestCertificate(stoppingToken);
            }
        }
    }
}