using System;
using System.Threading.Tasks;
using Keymaker.Service.Configuration;

namespace Keymaker.Service.Background;

public sealed class Checker : IChecker
{
    private readonly IKeymakerService _keymakerService;
    private readonly KeyMakerConfiguration _configuration;

    public Checker(IKeymakerService keymakerService, KeyMakerConfiguration configuration)
    {
        _keymakerService = keymakerService;
        _configuration = configuration;
    }

    public async Task<bool> ShouldTriggerChallengeAsync()
    {
        var certInfo = await _keymakerService.GetMostRecentCertificateInfoAsync();

        if (certInfo.IsEmpty())
        {
            return true;
        }

        var difference = DateTime.Now.Subtract(certInfo.Obtained).TotalDays;

        return difference > _configuration.RenewEveryDays;
    }
}