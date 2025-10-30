using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Keymaker.Service.Background;

public sealed class CheckerBackgroundService : BackgroundService
{
    private readonly IKeymakerService _keymakerService;
    private readonly IRenewalChecker _renewalChecker;
    private readonly ILogger<CheckerBackgroundService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(15);

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
            _logger.LogDebug("Checking if the challenge should be triggered.");

            if (await _renewalChecker.ShouldTriggerChallengeAsync())
            {
                _keymakerService.RequestCertificate(stoppingToken);
            }
        }
    }
}