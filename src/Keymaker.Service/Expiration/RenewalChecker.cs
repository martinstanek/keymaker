using System;
using System.Threading.Tasks;
using Keymaker.Service.Configuration;
using Keymaker.Service.Configuration.Service;
using Keymaker.Service.Store;

namespace Keymaker.Service.Expiration;

public sealed class RenewalChecker : IRenewalChecker
{
    private readonly ICertStoreService _storeService;
    private readonly KeyMakerConfiguration _configuration;

    public RenewalChecker(ICertStoreService storeService, KeyMakerConfiguration configuration)
    {
        _storeService = storeService;
        _configuration = configuration;
    }

    public async Task<NextChallenge> ShouldTriggerChallengeAsync()
    {
        var certInfo = await _storeService.GetMostRecentCertificateInfoAsync();
        var difference = DateTime.Now.Subtract(certInfo.Obtained).TotalHours;
        var nextNegotiation = certInfo.Obtained.AddHours(_configuration.RenewEveryHours);
        var nextChallenge = certInfo.IsEmpty()
            ? new NextChallenge
            {
                HoursLeft = 0,
                NextNegotiation = DateTime.Now,
                ShouldTrigger = true
            }
            : new NextChallenge
            {
                HoursLeft = Convert.ToInt32(Math.Round(nextNegotiation.Subtract(DateTime.Now).TotalHours)),
                NextNegotiation = nextNegotiation,
                ShouldTrigger = difference >= _configuration.RenewEveryHours
            };

        NextChallengeChecked.Invoke(this, nextChallenge);

        return nextChallenge;
    }

    public event EventHandler<NextChallenge> NextChallengeChecked = (_, _) => { };
}